# Architecture Decision Record

## Context

This repository is a small invoice-approval demo app. The current runtime is a .NET 8 ASP.NET Core API with a static web UI and an in-memory repository, as shown in [README.md](../README.md), [AGENTS.md](../AGENTS.md), and [docs/About.txt](About.txt).

Evidence from the codebase:

- The approval state machine is defined in [backend/src/InvoiceApp.Domain/InvoiceStatus.cs](../backend/src/InvoiceApp.Domain/InvoiceStatus.cs): `Draft -> PendingApproval -> Approved/Rejected`.
- The business workflow is implemented in [backend/src/InvoiceApp.Domain/ApprovalService.cs](../backend/src/InvoiceApp.Domain/ApprovalService.cs), where `Submit`, `Approve`, and `Reject` validate transitions and persist state changes.
- The HTTP controller layer is thin in [backend/src/InvoiceApp.Api/Controllers/ApprovalsController.cs](../backend/src/InvoiceApp.Api/Controllers/ApprovalsController.cs); it delegates to `ApprovalService` instead of embedding decision logic.
- The domain model stores the approval outcome in [backend/src/InvoiceApp.Domain/Invoice.cs](../backend/src/InvoiceApp.Domain/Invoice.cs) using `DecidedBy`, `DecidedAt`, and `DecisionNote`.
- The target-state plan in [docs/About.txt](About.txt) explicitly calls for a durable database, worker, message bus, and PayEx integration. It also requires payment requests to be idempotent and approval actions to be auditable.

Important distinction:

- Evidence: the current repo is a demo app with in-memory state and no external processing.
- Assumption: the future production design will need exactly-once semantics because approval decisions may trigger downstream payments and retries.

## Problem Statement

The current approval flow has no explicit replay protection. In the present implementation, `ApprovalService.Approve` and `ApprovalService.Reject` apply a state transition immediately and update the single authoritative invoice record without an idempotency key or deduplication ledger. That is acceptable for a demo, but it is unsafe for a production workflow where a retry, duplicate API call, or delayed side effect could re-trigger the same decision.

At 3 a.m. on the last day of the month, the risk is especially acute. In a financial close scenario, a retry of the same approval request could occur during a peak period, while a downstream payment system or notification worker is still processing the previous attempt. Without deduplication, a duplicate approval decision could result in duplicate settlement or conflicting ledger entries.

Evidence:

- The approval methods in [backend/src/InvoiceApp.Domain/ApprovalService.cs](../backend/src/InvoiceApp.Domain/ApprovalService.cs) do not track a unique operation identifier or a processed-operation ledger.
- [docs/About.txt](About.txt) states that payment requests must be idempotent and that approval decisions must be auditable.
- [backend/src/InvoiceApp.Domain/Invoice.cs](../backend/src/InvoiceApp.Domain/Invoice.cs) stores only the current state, not a durable history of processed decision operations.

Assumption:

- The future architecture will include durable storage and asynchronous processing, because the repo explicitly describes the production target as a future system.

## Decision

We will implement exactly-once processing for the approval workflow by using an idempotency key plus a dedupe table in the target production architecture.

This is the preferred design because the approval event is a business-critical decision and may later trigger a payment workflow. The first valid decision for a given invoice-operation pair should win, and any retries with the same ID should return the same outcome without reapplying side effects.

The selected design will:

1. Assign a unique operation ID to each approval or rejection request.
2. Persist that ID and outcome in a durable dedupe table.
3. Reject duplicate requests before mutating invoice state a second time.
4. Keep the approval state transition authoritative in the invoice record.
5. Ensure downstream payment or notification actions only fire once for a given operation.

This aligns with the current domain ownership model in [backend/src/InvoiceApp.Domain/ApprovalService.cs](../backend/src/InvoiceApp.Domain/ApprovalService.cs) and the future-state production constraints in [docs/About.txt](About.txt).

## Alternatives Considered

### 1. Idempotency key + dedupe table

This approach creates a unique business operation record and checks it before applying state transitions. It is the simplest operationally-sound answer when the workflow must be replay-safe and auditable.

Pros:

- Strong guarantee against duplicate approvals.
- Easy to reason about at the domain boundary.
- Naturally supports audit rows and traceability.

Cons:

- Requires durable persistence and a migration for a dedupe ledger.
- Adds a bit of API contract complexity.

### 2. Transactional outbox + worker

This pattern writes the approval decision as an outbox record and emits a worker-driven downstream process. It is excellent for ensuring reliability after the database commit, but it does not by itself solve duplicate approval execution at the domain boundary.

Pros:

- Very strong downstream reliability.
- Good for async processing and long-running tasks.

Cons:

- More moving parts than necessary for a small workflow.
- Requires a queue/worker topology and careful ordering.
- Still needs dedupe semantics for the original approval action itself.

### 3. Optimistic submission + nightly reconciliation

This approach accepts the initial operation and relies on a later reconciliation job to fix mismatches. It is low-friction and cheap to start with, but it is not acceptable for a decision that may trigger payment or financial control events.

Pros:

- Simple operational model.
- Low upfront build cost.

Cons:

- Uneven correctness under retries or network issues.
- High operational risk during month-end or peak periods.
- Not acceptable when payment safety matters.

## Consequences

Positive consequences:

- Duplicate approval requests become harmless.
- Payment or downstream actions are safer and easier to audit.
- Retry behavior is deterministic and explainable.
- The architecture better matches the planned production model in [docs/About.txt](About.txt).

Negative consequences:

- Requires additional persistence, schema work, and testing.
- Adds API and data-model complexity.
- Makes the workflow slightly more operationally demanding.

## Trade-offs

This decision prioritizes correctness and financial safety over raw simplicity. The current demo app is intentionally lightweight, but the target-state production requirement is not a lightweight and non-critical workflow. Once payment or settlement risk exists, the cost of a duplicate approval is higher than the cost of adding a dedupe record.

The trade-off is straightforward: a small increase in build complexity buys dramatically safer behavior under retries, outages, and end-of-month spikes.

## Failure Analysis

At 3 a.m. on the last day of the month, a retry storm or duplicate client request can happen while finance is reconciling close-out actions. If the system does not enforce exactly-once semantics, the same invoice approval could be replayed, resulting in multiple downstream payment or notification triggers. A worker or API retry might succeed on the second attempt while the first is still in flight. Without a dedupe ledger, the system would not know the operation was already processed.

In that situation, the failure mode is not just a confusing status update. It becomes a financial integrity issue: duplicate settlement, duplicate notification, or disputed approval history. The dedupe table turns this from an ambiguous retry into a closed, auditable operation.

## Cost Analysis

Build cost:

- 1 to 2 engineer days for API contract work, data model changes, and tests.
- Additional time for integration tests covering retries and duplicate requests.
- Minor governance effort for review and auditability checks.

Run cost:

- Low additional database overhead for storing a small dedupe ledger and operation metadata.
- Modest worker and monitoring costs if the workflow sends downstream events.
- Negligible compared with the financial and operational cost of a duplicate settlement or denied approval.

## Revisit Conditions

This decision should be reconsidered if:

- the invoice approval workflow never triggers external side effects,
- the business no longer performs financial settlement from approval events,
- the system is fundamentally re-scoped to a low-risk internal workflow with limited retries, or
- the dedupe ledger proves too expensive relative to the business impact.

## Where the Agent Was Wrong or Useless

The agent was wrong in a few ways:

1. It could confuse the planned production architecture with the current runtime architecture.
2. It could overstate the maturity of the approval flow without checking whether duplicate side effects were modeled.
3. It could underweight the operational risk at month-end when retries and duplicate submissions are most likely.

The agent was also less useful when it inferred production safety without validating the actual code path that replays decisions.

## Human Corrections

The human corrected the reasoning by clarifying that:

- the repo’s future-state production plan is not the current runtime,
- the decision must be framed as a target-state design requirement, not an existing implementation,
- the ADR must compare explicit alternatives instead of assuming one approach was obvious,
- the failure analysis and cost analysis must be grounded in the actual risk of duplicate approval under retries and month-end activity.

## Final Rationale

The correct design choice is idempotent, exactly-once approval processing with a durable dedupe ledger. The repository shows a sound domain workflow, but it lacks the replay-safety required for a real financial approval pipeline. The target-state design in [docs/About.txt](About.txt) explicitly requires idempotent payments and auditability, and the failure analysis shows the risk is highest precisely during high-volume, close-of-month conditions.

This is the strongest choice because it preserves the current service boundary, keeps the approval decision authoritative, and makes duplicate retries safe without leaving financial ambiguity unresolved.
