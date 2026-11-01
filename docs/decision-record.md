# Architecture Decision Record

## Context

The invoice-approval workflow must trigger a payment request to PayEx without creating duplicate settlements. The target-state design assumes Azure SQL, Azure Service Bus, a production team of six engineers, and 24x7 operational support.

The system must remain safe under retries, worker restarts, partial failures, and end-of-month spikes. This is not a low-risk batch workflow; it is a production payment path that must be reliable and recoverable.

## Problem Statement

We need a pattern for sending approved invoices to PayEx with the closest practical guarantee of exactly-once behavior.

The design must balance:

- delivery reliability,
- duplicate prevention,
- operational recovery,
- team maintainability,
- and cost over the full lifetime of the system.

## Alternatives Considered

### 1. Idempotency key with a dedupe table

- Simple and fast to implement.
- Strong protection at the PayEx boundary if the dedupe check and send are managed carefully.
- Best for direct, low-complexity integrations.
- Weakness: a crash between the dedupe write and the outbound payment can leave ambiguity unless carefully sequenced.

### 2. Transactional outbox with a worker

- Durable and resilient under retries and restarts.
- Decouples approval from external delivery.
- Best matches production payment patterns.
- Weakness: requires a worker, monitoring, and operational discipline.

### 3. Optimistic submission with nightly reconciliation

- Cheapest to implement.
- Lowest initial engineering cost.
- Not acceptable for a 24x7 payment workflow because it allows a window of missed or duplicate payment activity before reconciliation runs.

## Decision

Select the transactional outbox with a worker, with a stable PayEx idempotency key and dedupe ledger in Azure SQL.

The invoice approval transaction will record the payment intent and outbox event in one SQL transaction. A worker will process the outbox and send the outbound request to PayEx using a stable idempotency key. The dedupe record prevents duplicate submissions during retries or replay.

This approach gives the strongest production trade-off between correctness, recoverability, and operational clarity.

## Consequences

Positive:

- Durable event capture for approved invoices.
- Payment delivery is decoupled from the approval transaction.
- Retry and restart behavior is predictable.
- Duplicate submissions are reduced at the PayEx boundary with idempotency enforcement.

Negative:

- More infrastructure and monitoring are required than in the simplest design.
- Worker lag and outbox backlog must be actively monitored.
- The team must maintain operational run-books and alerting.

## Trade-offs

The simpler options are more attractive initially, but they weaken safety. The optimistic reconciliation model is inexpensive to prototype but expensive to operate in reality because the team must manually reconcile misses and duplicates.

The idempotency-key approach is cleaner than nightly reconciliation, but it still keeps payment submission in the request path and increases coupling to the external system. The outbox pattern adds complexity, but it provides a durable and recoverable workflow that is suitable for production payment processing.

## Recommendation Stress Test

### Preference Challenge

Even when the team prefers the cheaper and simpler optimistic approach, the recommendation does not change. The risk of delayed, missed, or duplicated payment processing is too high for a 24x7 financial workflow. Simplicity is not a valid reason to accept a known gap in correctness.

### Failure Scenario Review

At 3 a.m. on the last day of the month, the main failure mode is not a complete outage. It is operational overload: outbox backlog grows, worker throughput falls behind, retries pile up, and duplicate submission risk increases if dedupe enforcement is not atomic. This can create delayed settlement, duplicate PayEx calls, and costly manual reconciliation.

The recommended design remains the correct one, but it demands strong monitoring, queue health checks, retry controls, and idempotency enforcement to survive month-end pressure.

### Cost Review

- Idempotency key + dedupe table: low to medium build cost; medium operating cost; moderate hidden risk cost.
- Transactional outbox + worker: medium to high build cost; medium operating cost; low hidden risk cost.
- Optimistic submission + nightly reconciliation: low build cost; high operational cost in practice; high hidden risk cost due to manual reconciliation.

The cheapest-to-build option is not the cheapest-to-operate. The hidden cost of reconciliation and exception handling makes the optimistic approach the least attractive for a production payment path.

## Revisit Conditions

Revisit this decision if:

- the payment volume is very low and the financial risk is negligible,
- the external integration is no longer mission critical,
- the system changes to a non-financial, low-risk reconciliation model,
- or the operating team has a strong reason to accept more manual recovery overhead.

## Where the Agent Was Wrong or Useless

The initial recommendation focused heavily on technical reliability and correctly favored the transactional outbox pattern. However, it understated several practical concerns:

- It did not initially discuss worker-operability requirements such as alerting, backlog monitoring, and dead-letter queue management.
- It treated "exactly once" as a technical capability rather than clarifying that distributed systems typically provide effectively-once semantics through idempotency.
- It did not distinguish between implementation cost and operational cost until explicitly challenged.
- It did not explain the organizational burden of maintaining reconciliation processes in the optimistic submission model.

These points only emerged after follow-up questions regarding cost and month-end operational failure scenarios.

## Human Corrections

During review, I made the following adjustments:

1. I required the analysis to consider operational support effort separately from implementation effort, since a system may be inexpensive to build but expensive to run.
2. I challenged the recommendation from the perspective of a stakeholder prioritizing simplicity and lower delivery cost.
3. I requested analysis of end-of-month operational pressure because payment systems often fail under peak-volume conditions rather than normal traffic.
4. I required explicit discussion of idempotency at the PayEx boundary rather than relying solely on internal retry controls.

### Recommendation Stability Assessment

The recommendation remained unchanged after all three challenges.

The analysis consistently favored the transactional outbox pattern because the reliability requirements of a production payment workflow outweighed the implementation simplicity offered by the alternatives.

## Final Rationale

The recommendation from the agent was accepted after review, but not without modification.

The final decision places greater emphasis on operational recovery, monitoring, and long-term maintenance effort than the initial analysis did.

After considering implementation cost, production failure scenarios, and alternative preferences, I concluded that the transactional outbox pattern combined with PayEx idempotency provides the best balance between reliability and maintainability for this system.
