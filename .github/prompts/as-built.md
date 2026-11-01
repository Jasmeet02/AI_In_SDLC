Use architecture-analyst.

Analyze the repository and create:

1. An As-Built C4 Container Diagram of the current system.
2. A Sequence Diagram for the invoice approval workflow.

Requirements:

- Read only the source code and repository contents.
- Do not use README descriptions, external documentation, or assumptions as primary evidence.
- For every container, interaction, dependency, API call, datastore, middleware, service, repository, controller, or workflow step:
  - cite the supporting file path(s)
  - cite the supporting symbol name(s) where possible (class, interface, method, endpoint, middleware, etc.)

- Distinguish between:
  - Supported by code
  - Inferred from code

- Do not include anything that is not supported by code evidence.
- Any inferred element must appear in a separate "Inferred Items" section with justification.

C4 Container Diagram Rules:

- Follow C4 Container abstraction.
- Represent deployable/runtime containers, not source files.
- Do not use file names as container names.
- Do not use classes as containers.
- Do not use domain models as containers.
- Containers may include:
  - Browser UI
  - API Application
  - Background Worker
  - Authentication Service
  - Database
  - Queue
  - External System
  - Similar runtime/deployable units

For each container include:

- Name
- Responsibility
- Technology (if supported by code)
- Supporting Evidence

Sequence Diagram Rules:

- Show invoice approval end-to-end.
- Include all actors and participating systems found in code.
- Identify controller, service, repository, middleware, datastore, and external interactions when present.
- Include success path.
- Include validation/authentication steps if supported by code.

Mermaid Requirements:

- Output valid Mermaid syntax.
- Validate Mermaid syntax before returning.
- Fix any parse errors.
- Avoid unsupported Mermaid constructs.
- Ensure diagrams render without modification.

Output Structure:

# As-Built C4 Container Diagram

## Mermaid

<container diagram>

## Evidence

### Container Evidence

### Interaction Evidence

## Inferred Items

# Invoice Approval Sequence Diagram

## Mermaid

<sequence diagram>

## Evidence

### Step Evidence

## Inferred Items

# Sources

List every supporting file path and symbol used to create the diagrams.

Save outputs to:

docs/as-built-container.mmd
docs/invoice-approval-sequence.mmd

Then use interaction-logging.

Generate a JSONL audit record referencing:

- docs/as-built-container.md
- docs/invoice-approval-sequence.md
- docs/architecture-evidence.md

Record:
- stage: architecture-analysis
- skills used
- artifacts generated
- assumptions identified
- inferred items identified
- human validation status