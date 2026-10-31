# Target Architecture

```mermaid
flowchart LR
    User[User / Approver] --> Browser[Web UI]
    Browser --> API[Invoice API<br/>ASP.NET Core]
    API --> Domain[Approval Domain Logic]
    Domain --> SQL["Azure SQL<br/>[PLANNED]"]
    Domain --> Topic["Service Bus<br/>[PLANNED]"]
    Topic --> Worker["Background Worker<br/>[PLANNED]"]
    Worker --> PayEx["PayEx<br/>[PLANNED]"]
    Worker --> Notify[Notifications / Reconciliation]
    API --> Audit[Audit Trail]
    Domain --> Audit

    classDef planned fill:#fef3c7,stroke:#b45309,stroke-width:1px,color:#78350f;
    class SQL,Topic,Worker,PayEx planned;
```

# Planned Components

- Azure SQL [PLANNED]: durable storage for invoice, approval, and audit data.
- Service Bus [PLANNED]: asynchronous event processing for downstream notifications and workflows.
- Worker [PLANNED]: background consumer that processes queued events and performs operational tasks.
- PayEx [PLANNED]: external payment provider integration for settlement or payout processing.

These are future-only components described in [ops/README.md](../ops/README.md) and [docs/About.txt](../docs/About.txt). They are intentionally not present in the current in-memory demo implementation.
