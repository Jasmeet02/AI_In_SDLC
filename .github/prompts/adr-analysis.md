Review options for ensuring an approved invoice reaches PayEx exactly once.

Consider:

- Idempotency key with a dedupe table
- Transactional outbox with a worker
- Optimistic submission with nightly reconciliation

Assume:

- Azure SQL
- Azure Service Bus
- Team of six engineers
- Production system with 24x7 support

Compare the options, explain the trade-offs, and recommend one approach.

Save the analysis to:

docs/adr-analysis.md

Generate an audit log entry using interaction-logging.