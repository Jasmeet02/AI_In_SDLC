# Risk Audit

## Agent Setup Reviewed

### Repository evidence

- Agent: `architecture-analyst` is defined in `.github/agents/architecture-analyst.agent.md` and is responsible for architecture analysis, documentation, validation, and citations.
- Subagents: `.github/subagents/code-scanner.agent.md` and `.github/subagents/flow-tracer.agent.md` are configured as delegated analysis helpers. The flow tracer explicitly mentions tracing external integrations.
- Skills: `.github/skills/architecture-review.md`, `.github/skills/governance-review.md`, and `.github/skills/interaction-logger.md` define the operational model for architecture work, governance decisions, and JSONL audit logging.
- MCP Servers: `.mcp.json` configures two MCP servers: Playwright and Mermaid. These are external package-based tools invoked via `npx`.
- External integrations: `ops/README.md` explicitly describes a planned production deployment with Azure SQL, Service Bus, and a payment provider called PayEx. The incident packs under `ops/incidents/INC-2041` show outbound PayEx calls and timeout-related failure behavior.

### Assumptions

- The repository is intentionally a demo/training environment, not a production system. The "A note on the scenario" section in `ops/README.md` says names, hosts, and figures are invented and that none of the planned production elements are in the code yet.
- The AI setup is used for analysis, review, and artifact generation in a controlled environment rather than unsupervised production automation.

## Lethal Trifecta Assessment

### Private Data

#### Present?

Yes, but only in controlled, synthetic form.

#### Repository evidence

- `AGENTS.md` includes demo credentials: `demo.approver` / `ChangeMe123!` and `demo.admin` / `ChangeMe123!`.
- `ops/README.md` says the materials are fictional and no real customer or vendor appears anywhere in the folder.
- The app includes session-based authentication in `backend/src/InvoiceApp.Api/Auth/` and in-memory storage in `backend/src/InvoiceApp.Api/InMemoryInvoiceRepository.cs`.
- The incident logs and review artifacts contain document-like evidence with invoice IDs, time stamps, and network failure data.

#### Associated risk

- Even in a synthetic environment, AI agents may accidentally mirror or expose sensitive-looking data if such data is later introduced into the repo or prompts.
- The main risk is not current exposure of real secrets; it is the possibility that a future task introduces real customer, payment, or credential data into the same AI workflow without additional controls.
- A related risk is data minimization drift: the repo deliberately stores more operational context (logs, diffs, payment flows) than a small demo app needs, which increases the blast radius if accidental leakage occurs.

### Untrusted Content

#### Present?

Yes.

#### Repository evidence

- `ops/README.md` explicitly warns that the incident bundles and review cases are intentionally designed to include evidence without telling the agent the root cause or correct answer.
- `ops/evals/README.md` says the evaluator gives the agent a case diff and a review prompt, and nothing else. This is a classic untrusted-input pattern.
- `ops/incidents/INC-2047` and `INC-2041` contain logs, diffs, and status data from an operational incident scenario that the model must analyze without the full narrative.
- `.github/prompts/risk-audit.md` and other prompt files instruct the agent to evaluate operational incident material, diffs, and design artifacts.

#### Associated risk

- The agent is asked to reason over incident data, code diffs, and external system artifacts that may contain deceptive or malicious instructions.
- Prompt injection risk is higher here than in a clean codebase because upstream content is intentionally designed to be ambiguous, partial, and operationally noisy.
- The repository is intentionally a challenge environment, which means the model must be resilient to misleading, partial, or hostile context.

### External Communication

#### Present?

Yes.

#### Repository evidence

- `.mcp.json` declares two MCP servers, both launched via `npx` and therefore outbound package fetches during agent setup.
- `ops/README.md` states the production target includes a database, a message topic, and an external payment provider called PayEx.
- `docs/target-architecture.md` adds Azure SQL, Azure Service Bus, a background worker, and PayEx as planned future components.
- `ops/incidents/INC-2041/app-logs.txt` includes `PayExClient.SubmitAsync` calls and timeout/socket errors, showing the repository models a real external payment integration and its failure mode.

#### Associated risk

- The system is designed around external calls and operational dependencies, which raises the chance of outbound communication leaking context or triggering unintended actions.
- The MCP toolchain adds networked dependency risk, especially because packages are fetched through `npx` rather than being pinned and reviewed in a hardened configuration.
- In an operational setting, this would require strict allowlisting and network controls; in the current repo, the exercise is intentionally educational and therefore more permissive than a production AI runbook.

## Redesign Or Risk Acceptance

### Decision

Accept the risk, with explicit controls and a narrow execution model.

### Rationale

- The repository is an intentionally controlled learning environment. The operations folder is synthetic and explicitly marked as a production-like scenario rather than real production state.
- The setup intentionally combines incident review, code analysis, and external-system design because the point is to teach governance and architecture under realistic constraints.
- A redesign would remove the educational value of evaluating untrusted incident content and planned external dependencies. The safer and more realistic choice is to keep the scenario and add controls.

### Why not redesign to remove one leg?

- Removing Private Data would strip away the realistic session and credential scenarios that are necessary to discuss data handling.
- Removing Untrusted Content would undermine the entire incident-analysis challenge and the exercise value of reviewing diffs and logs.
- Removing External Communication would eliminate the PayEx and MCP integration context that the architecture and governance work depends on.

### Final decision record

The team accepts the combined Lethal Trifecta risk only in the controlled, synthetic context of this repo. This acceptance is valid while the system remains offline from real customer data, uses synthetic incident packs, and requires human review before any live action or external request is allowed.

## Risk Review

### Prompt Injection

#### Risk Description

A malicious or misleading prompt embedded in a diff, incident log, or external artifact could cause the agent to trust the wrong source of truth or to prioritize the wrong remediation path.

#### Existing Controls

- `ops/README.md` explicitly states that the evidence is intentionally incomplete and designed to require analysis instead of direct answers.
- `AGENTS.md` enforces conventions and domain boundaries, reducing the chance of the agent inventing behavior or taking action outside the repo.
- `interaction-logger.md` records actual tools, skills, and artifacts used for transparency.

#### Recommended Controls

- Treat all incident evidence as untrusted input and separate it from trusted instructions.
- Require a human approval step before any change based on incident analysis.
- Use a clear trust boundary between user-provided content and system prompts.
- Prefer narrow, deterministic prompts and tool restrictions over open-ended analysis.

#### Residual Risk

Medium. The repo is built around untrusted content as part of the exercise, so the model must maintain skepticism even when the content looks authoritative.

### Data Leakage

#### Risk Description

Sensitive tokens, credentials, or customer data could be exposed in logs, outputs, or generated artifacts if a task is ever repurposed for real environments.

#### Existing Controls

- `AGENTS.md` contains an explicit rule: "Never commit real credentials, tokens or customer data."
- `ops/README.md` states that all names, hosts, and figures are invented.
- The repository is set up to use demo credentials rather than real secrets.

#### Recommended Controls

- Keep a strict synthetic-data policy for all evaluation folders and incident packs.
- Add automated secret scanning and redaction to all AI-generated output and patch files.
- Require a DLP review step before publishing generated artifacts or sharing them externally.
- Prohibit real customer or production secrets from entering the workspace.

#### Residual Risk

Low to medium. The current repo is synthetic, but the same pattern would become risky if real data were introduced or copied into incident packs.

### Insecure Or Unreviewed Code

#### Risk Description

The agent can create or modify code, documentation, and tests. If it contributes insecure logic or unreviewed patches, the repository could drift into unsafe behavior or incorrect architecture decisions.

#### Existing Controls

- `AGENTS.md` requires domain-layer discipline and test coverage.
- Architecture rules are enforced by tests in `backend/tests/InvoiceApp.Tests/ArchitectureTests/DependencyRulesTests.cs`.
- The project includes conventional .NET test execution and review-style artifacts.
- `governance-review.md` requires assumptions, human decisions, and explicit documentation of where AI assistance was wrong or incomplete.

#### Recommended Controls

- Require human review for all non-trivial generated code changes.
- Run targeted tests and static analysis before merging any AI-generated patch.
- Limit the agent to documentation, design, and proof-of-concept work unless a human approves code changes.
- Keep a documented exception process for changes that are intentionally exploratory.

#### Residual Risk

Medium. The model is powerful enough to create convincing but unsafe code when left unsupervised, even in a demo repository.

### Supply Chain Risk

#### Risk Description

The repo relies on `npx` package execution for both Playwright and Mermaid MCP servers. A compromised package or registry path could execute untrusted code with the developer’s permissions.

#### Existing Controls

- The setup is local to the developer workspace; the package references are configured in `.mcp.json` rather than hidden in a production workload.
- The repository is a local development environment rather than a production deployment.

#### Recommended Controls

- Pin exact package versions instead of using floating `@latest` or unversioned package sources.
- Use a trusted package registry, lockfiles, and software bill of materials (SBOM) generation.
- Review package provenance and install scripts before enabling any MCP server.
- Disable or block package installation outside an approved, reviewed toolchain.

#### Residual Risk

Medium to high. The current MCP setup is convenient but not hardened for untrusted supply chains.

### Excessive Agency

#### Risk Description

The system gives the agent powerful abilities: reading repo contents, generating docs, running tests, and invoking external tooling. Without strict boundaries, the agent can act beyond the intended scope.

#### Existing Controls

- `architecture-analyst.agent.md` defines specific responsibilities and uses delegation deliberately.
- `interaction-logger.md` records what was done and which artifacts were produced.
- `governance-review.md` requires explicit decisions and human validation.

#### Recommended Controls

- Limit AI agent actions to read/analysis unless a human explicitly authorizes writes or external calls.
- Require an approval gate for any command that can modify code, install packages, or open external network access.
- Keep the default mode read-only; escalate only for approved tasks.

#### Residual Risk

Medium. The model is clearly trusted for analysis and documentation, but the repo still allows tool execution that can affect the developer environment.

## Controls

### Data controls

- Keep all operations data synthetic and invented.
- Keep a no-real-data rule for incident packs, logs, credentials, and payment artifacts.
- Use secret scanning and pre-merge review for generated outputs.

### Content controls

- Treat all incident evidence as untrusted content unless approved by a human.
- Separate trusted instructions from untrusted observations in prompts and tooling.
- Require explicit evidence before drawing conclusions from a diff, log, or external signal.

### External controls

- Limit outbound network access to approved MCP servers and package registries only.
- Prefer pinned packages and trusted registries.
- Require human review before enabling any new external integration or external tool.

### Operational controls

- Use `interaction-logging` for every significant AI task and include artifacts, tools, and human decisions.
- Require human approval for non-trivial code changes or any change that touches external dependencies.
- Maintain an exception process for when a task must exceed the default read-only posture.

## Residual Risks

The residual risk is not zero, but it is acceptable only under a narrow definition of the environment:

- This is a synthetic demo and evaluation environment, not a live production deployment.
- The repository does not contain real credentials or real customer data.
- External packages and tools are used only in a controlled developer environment.
- Human review remains the final gate for any action that could affect code, infrastructure, or external systems.

The remaining risk is primarily operational and governance-related: a real-world prompt, a real secret, or an unreviewed package could convert a controlled review exercise into a broader security problem.

## Decision Record

### Final decision

Accept the overall risk posture, but only as a controlled, synthetic, read-only-by-default AI environment.

### Rationale

- The repository is designed to teach AI governance, payment safety, data handling, and architecture review in a realistic but fictional scenario.
- The operational design deliberately contains the three unsafe factors — private data, untrusted content, and external communication — because they are the subject of the exercise.
- The risk is manageable because the repository is synthetic, the guidance explicitly prohibits real credentials and customer data, and the workflow relies on human review and recorded evidence.

## Revisit Triggers

The team should revisit this decision immediately if any of the following occurs:

- Real customer data, payment data, or live credentials are introduced into the repo or chat prompts.
- Any model or tool begins making outbound calls beyond the approved local or trusted package flow.
- A new MCP server, package, or integration is added without provenance checks and pinning.
- The agent is asked to act on real production incidents, live infrastructure, or customer-facing messaging without explicit human approval.
- The team starts relying on the AI to directly change code, deploy artifacts, or interact with external systems without a review gate.

The chosen posture is therefore: practical for a training environment, but not a safe default for production AI automation.
