---
name: architecture-review
description: Generate architecture diagrams, citations, inferred items and architecture tests.
---

# Architecture Review

## Discover Architecture

Identify:

- Entry points
- Controllers
- Services
- Repositories
- Data stores
- External dependencies

## Generate Diagrams

Generate:

1. As-Built Container Diagram
2. Approval Sequence Diagram
3. Target Architecture Diagram

Use the diagramming technology requested by the prompt.

## Citations

For every diagram element provide:

- File path
- Class
- Method or Symbol

## Inferred Items

Anything not directly evidenced by code must be listed separately.

Include a reason.

## Validation

Verify:

- Diagram nodes exist
- Relationships are supported by code
- Citations exist

Move unsupported items to Inferred Items.

## Target Architecture

When requested add:

- Azure SQL
- Service Bus
- Worker
- PayEx

Mark future-only components as:

[PLANNED]

## Architecture Test

Generate one runnable architecture test.

Example:

Controllers must not directly depend on repositories.

Include execution command.

## Rules

- Do not invent components.
- Every diagram element must have a citation.
- Unsupported items belong in Inferred Items.