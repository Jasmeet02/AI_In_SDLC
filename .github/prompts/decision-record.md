# Architecture Decision Record

## Context

## Problem Statement

## Decision

## Alternatives Considered

(Must compare:
- Idempotency key + dedupe table
- Transactional outbox + worker
- Optimistic submission + nightly reconciliation)

## Consequences

## Trade-offs

## Failure Analysis
(How does it fail at 3 a.m. on the last day of the month?)

## Cost Analysis
(Build cost and run cost)

## Revisit Conditions
(What would cause this decision to be reconsidered?)

## Where the Agent Was Wrong or Useless

## Human Corrections

## Final Rationale

Then use interaction-logging and generate a JSONL audit record for:

logs/ai-assistance-log.jsonl

The audit record must reference:

- docs/decision-record.md
- Skills used
- AI actions performed
- Human validation performed
- Outcome