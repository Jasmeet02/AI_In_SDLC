# Target Architecture

```mermaid
flowchart LR
    User["Operator / Approver\nREADME.md"]
    Browser["Browser UI\nbackend/src/InvoiceApp.Api/wwwroot/index.html\nbackend/src/InvoiceApp.Api/wwwroot/app.js"]

    subgraph CurrentApp["Current app (as-built)"]
        Web["Static Web UI\nserved from wwwroot"]
        API["ASP.NET Core API\nInvoicesController + AuthController"]
        Auth["SessionAuthMiddleware\nRequireRoleAttribute"]
        Service["ApprovalService\nbackend/src/InvoiceApp.Domain/ApprovalService.cs"]
        Repo["InMemoryInvoiceRepository\nbackend/src/InvoiceApp.Api/InMemoryInvoiceRepository.cs"]
        Session["SessionStore\nbackend/src/InvoiceApp.Api/Auth/SessionStore.cs"]
    end

    subgraph Planned["Target production architecture [PLANNED]"]
        SQL["Azure SQL\n[PLANNED]"]
        Bus["Service Bus\n[PLANNED]"]
        Worker["Worker\n[PLANNED]"]
        PayEx["PayEx\n[PLANNED]"]
    end

    User --> Browser
    Browser -->|HTTP /api| API
    Browser --> Web
    Web --> API
    API --> Auth
    API --> Service
    Auth --> Session
    Service --> Repo

    Service -.->|persist approved state| SQL
    Service -.->|publish invoice-approved event| Bus
    Bus -.->|deliver event| Worker
    Service -.->|payment request| PayEx

    classDef current fill:#edf7ed,stroke:#2e8b57,color:#111;
    classDef planned fill:#fff4e5,stroke:#d97706,color:#111,stroke-dasharray: 5 5;
    classDef user fill:#e8f1ff,stroke:#3b82f6,color:#111;

    class User user;
    class Web,API,Auth,Service,Repo,Session current;
    class SQL,Bus,Worker,PayEx planned;
```

# Planned Components

- Azure SQL [PLANNED] — durable relational storage for invoices, approvals, audit records, and payment references.
- Service Bus [PLANNED] — event topic for invoice approval events and downstream processing.
- Worker [PLANNED] — background consumer for notifications and reconciliation work.
- PayEx [PLANNED] — external payment provider integration for approved invoices.

- Current app baseline: [README.md](../README.md), [backend/src/InvoiceApp.Api/Program.cs](../backend/src/InvoiceApp.Api/Program.cs), [backend/src/InvoiceApp.Domain/ApprovalService.cs](../backend/src/InvoiceApp.Domain/ApprovalService.cs), and [backend/src/InvoiceApp.Api/InMemoryInvoiceRepository.cs](../backend/src/InvoiceApp.Api/InMemoryInvoiceRepository.cs).
- Planned production evidence: [ops/README.md](../ops/README.md), [docs/About.txt](../docs/About.txt), [ops/incidents/INC-2047/servicebus-status.txt](../ops/incidents/INC-2047/servicebus-status.txt), and [ops/incidents/INC-2047/worker-logs.txt](../ops/incidents/INC-2047/worker-logs.txt).
