# ADR Analysis: Ensuring an Approved Invoice Reaches PayEx Exactly Once

## Context

When an invoice is approved, the system must send a payment request to PayEx without risking a duplicate submission. In production, the service will run on Azure SQL and Azure Service Bus, with a team of six engineers and 24x7 support. The design must be safe under crashes, retries, and duplicate delivery races while remaining understandable and maintainable for a small team.

The real goal is not theoretical "perfect exactly-once" processing across multiple systems. In distributed systems, the practical target is an operationally reliable pattern that provides:

- at-most-once duplicate prevention at the payment boundary
- durable retry after failures
- observability and repairability
- low operational burden for a six-person team

## Decision drivers

- Prevent duplicate payment submissions to PayEx
- Keep the invoice approval flow resilient to process crashes and retries
- Preserve the integrity of the invoice state in Azure SQL
- Use the Azure platform choices already assumed in the target architecture
- Keep implementation and support complexity realistic for a production team of six engineers

## Options considered

### 1. Idempotency key with a dedupe table

Description:

- Each payment attempt carries a stable idempotency key derived from the invoice approval or payment request.
- The system stores a dedupe record in Azure SQL before or at the time of the payment call.
- If the same invoice is retried, the service checks the dedupe table before sending to PayEx.

Benefits:

- Simple to reason about and fast to implement.
- Strong protection against duplicate external calls.
- Good fit when the payment boundary is the only critical integration point.
- Requires no asynchronous worker if the API can call PayEx synchronously.

Trade-offs:

- A single API request still holds the payment call, so a crash between the dedupe record and the PayEx call can leave ambiguity unless the write and outbound call are carefully sequenced.
- If the API is downstream of a failed or retried request, the dedupe logic must be correct on every retry path.
- Produces the highest coupling between the approval workflow and the external payment call.

Best fit:

- Small systems with a single payment path and a low number of outbound side effects.

### 2. Transactional outbox with a worker

Description:

- The invoice approval transaction inserts the invoice state and an outbox event in the same Azure SQL transaction.
- A background worker reads the outbox and sends the payment request to PayEx.
- The worker uses a payment idempotency key and a dedupe ledger to prevent duplicates.

Benefits:

- Strong durability: the approval and the need to pay are recorded atomically.
- Avoids the problem of performing an external call inside the user request path.
- Natural fit for 24x7 production operations and reliable retry behavior.
- Better decoupling between the business transaction and external integration.

Trade-offs:

- More moving parts: SQL table, worker, retries, monitoring, and dead-letter handling.
- Requires operational discipline to keep the outbox draining and to prevent backlog growth.
- Slightly more implementation complexity than a direct idempotent API call.

Best fit:

- Production systems where payment sending must be reliable, retryable, and decoupled from the main API flow.

### 3. Optimistic submission with nightly reconciliation

Description:

- The system submits payment requests optimistically and assumes the majority of processing will succeed.
- A nightly job compares expected payments to actual PayEx settlement records and flags misses or duplicates.

Benefits:

- Very low implementation complexity.
- Minimal real-time integration logic.
- Cost-effective for low-risk or non-urgent workflows.

Trade-offs:

- Leaves significant exposure to duplicate or missed payments between reconciliation windows.
- Poor fit for 24x7 customer-facing financial operations.
- Requires manual or semi-manual intervention when the mismatch is detected.
- Weakens the guarantee needed for an approval workflow that is meant to settle money promptly.

Best fit:

- Non-critical reconciliation flows or low-risk batch reconciliation, not high-confidence payment dispatch.

## Comparison

| Option | Reliability | Operational complexity | Duplicate protection | Best for |
| --- | --- | --- | --- | --- |
| Idempotency key + dedupe table | Medium-High | Low-Medium | High at the payment boundary | Small, direct payment integrations |
| Transactional outbox + worker | High | Medium-High | High, especially when paired with idempotency keys | Production systems with retries and 24x7 support |
| Optimistic submission + nightly reconciliation | Low | Low | Low | Low-risk reconciliation, not finance-grade payment dispatch |

## Recommendation

Recommend the transactional outbox with a worker, with the worker sending PayEx calls using a stable idempotency key and a dedupe ledger in Azure SQL.

This is the best fit for the stated assumptions:

- Azure SQL gives us a durable transactional source of truth.
- Azure Service Bus allows the system to separate the approval transaction from later payment dispatch.
- A six-person team benefits from a clear, established reliability pattern instead of ad hoc retries.
- 24x7 support means the system must survive restarts, retries, and partial failures without requiring manual repair for every duplicate event.

In other words, the chosen pattern should be:

1. Approve invoice and write payment intent + outbox row in one SQL transaction.
2. Worker reads outbox and publishes or invokes the payment message.
3. Worker includes a stable idempotency key and writes a dedupe record before calling PayEx.
4. If the worker retries, PayEx sees the same idempotency key and rejects or ignores duplicate requests.
5. If the worker fails after a successful PayEx call but before final logging, the dedupe record prevents resubmission.

This gives us the closest practical implementation of "exactly once" in a distributed payment workflow: durable delivery, retry-safe processing, and downstream duplicate prevention.

## Consequences

Positive:

- Payment dispatch becomes resilient to API crashes and worker restarts.
- The approval transaction remains consistent with the payment intent.
- Operational recovery is easier because the outbox provides a replay mechanism.
- The system complies with the production expectations of a payment-oriented workflow.

Negative:

- More infrastructure and monitoring are needed than with a simple direct-call design.
- The team must maintain the worker and dedupe logic correctly.
- "Exactly once" is still best treated as effectively-once semantics with a downstream idempotency contract.

## Final decision

Use transactional outbox plus worker as the primary pattern, and embed a PayEx idempotency key plus dedupe ledger inside that flow. This is the strongest production trade-off between correctness, operational safety, and team efficiency.

## Recommendation Stress Test

### Preference Challenge

Assume the team prefers optimistic submission with nightly reconciliation because it is cheaper and simpler. Would the recommendation change?

No. The recommendation should not change for a payment workflow with 24x7 production support and a requirement to send an approved invoice to PayEx exactly once.

The cheaper option is attractive because it is quick to implement and light to operate. However, the trade-off is unacceptable for a financial integration. Nightly reconciliation creates a window in which a payment can be missed, duplicated, or only discovered after the fact. That operating model is acceptable for some low-risk batch processes, but it is not acceptable for production invoice settlement where a payment is a material business event and the platform is expected to be continuously available.

In other words, the cost of a missed or duplicated external payment is higher than the cost of adding a durable outbox, a worker, and a dedupe check. The operational simplicity of optimistic submission is purchased by accepting a known gap in correctness. For this architecture decision, that gap is not acceptable.

The recommendation therefore remains the same:

- transactional outbox with a worker as the primary delivery pattern,
- durable Azure SQL persistence for the payment intent,
- Azure Service Bus or equivalent asynchronous handoff where appropriate,
- positive idempotency enforcement at the PayEx boundary using a stable idempotency key and dedupe ledger.

This keeps the design aligned with the real production risk: reliable payment delivery, replay safety, and bounded operational recovery, rather than a cheaper but riskier "fix it later" pattern.

### Failure Scenario Review

How does the recommended solution fail at 3 a.m. on the last day of the month?

The recommended solution does not fail in the sense of an outright crash or permanent inability to process; it fails in the more dangerous way that many distributed systems fail: it becomes vulnerable to a backlog, duplicate replay, and operational drift under peak pressure.

At 3 a.m. on month-end, the system is likely to see a spike in invoice approvals, high retry volumes, and the classic sequence of events where the worker is processing the outbox while supporting infrastructure is under stress. Under that load, the weak point is not the core architecture itself but the operational assumptions around timing and recovery. Specifically:

- the outbox worker can lag behind the approval stream,
- retries can arrive faster than the worker can drain them,
- a message can be processed more than once if the dedupe record is not committed atomically with the external call,
- a PayEx timeout can result in a retry that is accepted as a new payment attempt if the idempotency key is not persisted or reused consistently,
- and the support team can be flooded with reconciliation work at the worst possible time.

This is the critical operational failure mode: the design remains robust, but it still depends on correct and timely worker execution, instrumentation, and dedupe enforcement. At 3 a.m. on month-end, if the outbox backlog grows or the worker stalls, the system can be delayed in sending payments just when volume is highest, causing a cascade of partial settlements and manual investigations.

The point is not that the recommended design is defective. The point is that it fails only under severe operational pressure when failure handling is weak or the worker is under-provisioned. In other words, the design is still the right choice, but it requires strong operational controls:

- backlog monitoring,
- alerting on outbox lag,
- duplicate detection metrics,
- retry budget controls,
- and explicit support procedures for partial or timed-out PayEx calls.

Without those controls, the pattern degrades into the same operational failure pattern that many payment systems experience during end-of-month peaks: delayed settlement, duplicate payments, and expensive manual reconciliation. The recommended design remains the best option, but it is only safe when paired with disciplined run-book and monitoring practices.

### Cost Review

What does each option cost to build and operate?

The options vary significantly in implementation cost and ongoing operational cost, and the difference is driven by the reliability target rather than by the approval logic alone.

#### 1. Idempotency key with a dedupe table

Build cost:

- Low to medium: a few database tables, a stable key, and retry checks are straightforward.
- Implementation is relatively fast for a small team.

Operational cost:

- Medium: the team must maintain the retry logic, dedupe cleanup, and any ambiguity handling if a crash happens between the record write and the outbound call.
- The burden is manageable for a small system but grows as the payment flow becomes more complex.

Best value:

- Good for a direct, lower-complexity integration where the workflow is not deeply asynchronous.

#### 2. Transactional outbox with a worker

Build cost:

- Medium to high: requires an outbox table, worker service, monitoring, retry policy, dead-letter handling, and operational dashboards.
- The initial setup is more expensive than the simple idempotency approach.

Operational cost:

- Medium: infrastructure and monitoring are more expensive, but the cost is offset by lower failure-handling burden and better recovery behavior.
- The main operating cost is making sure the worker keeps up with backlog and that the dead-letter or retry path remains healthy.

Best value:

- Strongest value when reliability and recovery are critical for continuous production service.

#### 3. Optimistic submission with nightly reconciliation

Build cost:

- Low: easiest to implement because it minimizes real-time processing and message routing.

Operational cost:

- High in practice: the cost is not in building the code; it is in the aftermath. Manual review, reconciliation, exception handling, and customer support are expensive when mismatches occur.
- The system appears cheap until a missed payment or duplicate payment requires correction.

Best value:

- Only attractive for weakly critical, non-financial, or low-risk workflows.

### Cost comparison summary

| Option | Build cost | Ongoing operational cost | Hidden risk cost |
| --- | --- | --- | --- |
| Idempotency key + dedupe table | Low-Medium | Medium | Medium |
| Transactional outbox + worker | Medium-High | Medium | Low |
| Optimistic submission + nightly reconciliation | Low | High | High |

The cheapest-to-build option is not the cheapest-to-operate. The optimistic reconciliation model looks inexpensive upfront, but it creates a large hidden cost in manual exception handling and failed settlement reconciliation. For a production payment flow, that hidden cost outweighs the initial simplicity gain.
