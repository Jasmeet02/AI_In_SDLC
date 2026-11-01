# Architecture Test

## Architectural Rule

Controllers must not directly depend on repositories.

This rule is enforced in the repository by the boundary between the API layer and the Domain layer:

- `backend/src/InvoiceApp.Api/Controllers` contains controllers such as `InvoicesController`, `ApprovalsController`, and `AuthController`.
- `backend/src/InvoiceApp.Domain` contains the business rules and the repository abstraction, including `IInvoiceRepository` and `ApprovalService`.
- The AGENTS convention explicitly states: "Business rules go in the Domain project, never in controllers" and "Controllers must not directly depend on repositories."

## Rationale

The architecture protects the business layer by preserving a clean dependency direction:

- Controllers act as API entry points.
- `ApprovalService` owns the invoice lifecycle rules.
- `IInvoiceRepository` is an infrastructure/data-access abstraction used by the domain service, not by controllers.

This rule prevents controllers from bypassing the domain logic, keeps persistence concerns behind the application boundary, and makes the architecture easier to test and evolve without coupling HTTP concerns to the data layer.

## Code

```csharp
using System.Reflection;
using InvoiceApp.Api.Controllers;
using InvoiceApp.Domain;
using NetArchTest.Rules;
using Xunit;

namespace InvoiceApp.Tests.ArchitectureTests;

public class DependencyRulesTests
{
    [Fact]
    public void Controllers_should_not_depend_directly_on_repositories()
    {
        var controllerTypes = Types.InAssembly(typeof(InvoicesController).Assembly)
            .That()
            .ResideInNamespace("InvoiceApp.Api.Controllers")
            .GetTypes();

        var violatingControllers = controllerTypes
            .Where(controller => controller
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(constructor => constructor.GetParameters()
                    .Any(parameter => parameter.ParameterType == typeof(IInvoiceRepository))))
            .Select(controller => controller.FullName ?? controller.Name)
            .ToList();

        Assert.True(
            violatingControllers.Count == 0,
            "Controllers must not directly depend on repositories. Violations: " + string.Join(", ", violatingControllers));
    }
}
```

Required package references:

```xml
<ItemGroup>
  <PackageReference Include="NetArchTest.Rules" Version="1.3.2" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
  <PackageReference Include="xunit" Version="2.9.2" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  <PackageReference Include="coverlet.collector" Version="6.0.2" />
</ItemGroup>
```

## Execution

```bash
dotnet test backend/InvoiceApp.sln --nologo --logger "console;verbosity=minimal"
```

## Expected Result

### PASS condition

The test passes when no controller constructor directly accepts `IInvoiceRepository`.

This is the intended architecture because the domain service handles repository access behind the API boundary.

### FAIL condition

The test fails when a controller constructor or dependency graph includes `IInvoiceRepository` directly.

That current repository state is supported by the following code evidence:

- `backend/src/InvoiceApp.Api/Controllers/InvoicesController.cs` contains `public InvoicesController(IInvoiceRepository repository, ApprovalService approvals)`.
- `backend/src/InvoiceApp.Api/Controllers/ApprovalsController.cs` uses `ApprovalService` only, but the rule remains important because the API layer is meant to avoid direct repository concerns.
- `backend/src/InvoiceApp.Domain/ApprovalService.cs` is the place where the business workflow and repository updates are coordinated.

## Evidence

- Repository structure evidence:
  - `backend/src/InvoiceApp.Api/Controllers/InvoicesController.cs` — controller layer.
  - `backend/src/InvoiceApp.Domain/ApprovalService.cs` — domain logic and approval workflow.
  - `backend/src/InvoiceApp.Domain/IInvoiceRepository.cs` — repository abstraction owned by the domain layer.
  - `backend/src/InvoiceApp.Api/InMemoryInvoiceRepository.cs` — infrastructure implementation.

- Rule justification from code:
  - `ApprovalService` performs the state transitions and repository updates in `Submit`, `Approve`, and `Reject`.
  - `InvoicesController` directly injects `IInvoiceRepository` and also uses the domain service, which shows the current architecture is not yet fully aligned with the stricter boundary.
  - The AGENTS convention says business rules belong in the domain project and controllers must not directly depend on repositories.

This test enforces the intended architecture even when the current code still violates it, so the failure is a valid architectural signal rather than a false alarm.
