# Architecture Evidence

## Supported by code

- The application is a single ASP.NET Core web app served from the API project.
  - Evidence: `backend/src/InvoiceApp.Api/Program.cs` calls `AddControllers()`, `UseStaticFiles()`, `UseMiddleware<SessionAuthMiddleware>()`, and `MapControllers()`.
  - Symbols: `Program`, `SessionAuthMiddleware.InvokeAsync`, `app.MapControllers`.

- Browser UI is served from the same app and makes authenticated HTTP requests.
  - Evidence: `backend/src/InvoiceApp.Api/wwwroot/index.html` and `backend/src/InvoiceApp.Api/wwwroot/app.js`.
  - Symbols: `api(path, options)`, `render()`, `signOut()`, `login-form` listeners.

- Authentication is in-memory and role-based.
  - Evidence: `backend/src/InvoiceApp.Api/Auth/SessionStore.cs`, `backend/src/InvoiceApp.Api/Auth/UserSession.cs`, and `backend/src/InvoiceApp.Api/Auth/RequireRoleAttribute.cs`.
  - Symbols: `SessionStore.Create`, `SessionStore.Get`, `UserSession`, `RequireRoleAttribute.OnActionExecuting`.

- The invoice store is in-memory rather than database-backed.
  - Evidence: `backend/src/InvoiceApp.Api/InMemoryInvoiceRepository.cs` implements `IInvoiceRepository` and stores `List<Invoice>` in process.
  - Symbols: `InMemoryInvoiceRepository.GetById`, `InMemoryInvoiceRepository.Update`, `IInvoiceRepository`.

- The invoice approval workflow is implemented in the domain service.
  - Evidence: `backend/src/InvoiceApp.Domain/ApprovalService.cs` manages `Draft -> PendingApproval -> Approved/Rejected` transitions.
  - Symbols: `ApprovalService.Submit`, `ApprovalService.Approve`, `ApprovalService.Reject`, `ApprovalService.EnsurePending`.

## Inferred items

- Inferred from code: The runtime deployment is a single process hosting both the browser UI and API services.
  - Justification: the static web files are served by the API host and the API controllers are mapped in the same ASP.NET Core app. There is no separate host/distributed container declared in the code.

- Inferred from code: There are no separate database, queue, or external authentication containers in the current as-built runtime.
  - Justification: state is held in memory (`SessionStore`, `InMemoryInvoiceRepository`), credentials are loaded from configuration (`appsettings.json`), and there are no service clients or message brokers in the code.

## Sources

- `backend/src/InvoiceApp.Api/Program.cs`
- `backend/src/InvoiceApp.Api/Auth/SessionAuthMiddleware.cs`
- `backend/src/InvoiceApp.Api/Auth/SessionStore.cs`
- `backend/src/InvoiceApp.Api/Auth/RequireRoleAttribute.cs`
- `backend/src/InvoiceApp.Api/Auth/UserSession.cs`
- `backend/src/InvoiceApp.Api/InMemoryInvoiceRepository.cs`
- `backend/src/InvoiceApp.Domain/ApprovalService.cs`
- `backend/src/InvoiceApp.Domain/IInvoiceRepository.cs`
- `backend/src/InvoiceApp.Domain/Invoice.cs`
- `backend/src/InvoiceApp.Api/appsettings.json`
- `backend/src/InvoiceApp.Api/wwwroot/index.html`
- `backend/src/InvoiceApp.Api/wwwroot/app.js`
