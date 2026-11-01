Use architecture-analyst.

Using the validated As-Built C4 Container Diagram as the baseline:

Create a Target Architecture Diagram showing the future state after persistence and PayEx integration are implemented.

Requirements:

- Read the validated as-built architecture artifacts first.
- Use Mermaid MCP to generate the diagram.
- Preserve all existing containers and interactions unless there is evidence they would change.
- Add proposed architecture elements required for:
  - Azure SQL persistence
  - Service Bus messaging
  - Background Worker processing
  - PayEx integration

Clearly separate:

- Existing Components
- Planned Components

Mark all future-only elements as:

[PLANNED]

Mark all future-only interactions as:

[PLANNED]

Do not remove existing components unless justified by code evidence.

For every new component or interaction:

- Explain why it was added.
- State whether it is supported by current code or is a proposed design item.

List all proposed elements explicitly.

Generate valid Mermaid syntax.

Validate Mermaid syntax before returning.

Fix Mermaid parse errors if any are found.

Outputs:

docs/target-architecture.md

Structure:

# Target Architecture

## Mermaid Diagram

## Existing Components

## Planned Components

## Existing Interactions

## Planned Interactions

## Design Rationale

## Inferred Future-State Assumptions

## Sources

For every existing component and interaction:
- cite the supporting file path or symbol from the repository.

For every planned component and interaction:
- explain why it is required for the target architecture.

Then use interaction-logging.

Generate a JSONL audit record referencing:

- docs/target-architecture.md
