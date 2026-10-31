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
