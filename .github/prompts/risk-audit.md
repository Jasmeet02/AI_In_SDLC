Use architecture-analyst.

Use governance-review.

Audit the agent setup described in:

ops/README.md

Requirements:

Evaluate the setup against the AI safety Lethal Trifecta:

1. Private Data
2. Untrusted Content
3. External Communication

For each leg:

- Determine whether it is present.
- Provide evidence from the repository.
- Explain the associated risk.

Then:

- Decide whether to redesign the setup to remove one of the three legs
  OR
- Explicitly accept the risk.

If accepting the risk:

- Record the decision.
- Record the trigger that would cause the team to revisit the decision.

Also evaluate the following risks:

- Prompt Injection
- Data Leakage
- Insecure or Unreviewed Code
- Supply Chain Risk
- Excessive Agency

For each risk include:

- Risk Description
- Existing Controls
- Recommended Controls
- Residual Risk

Reference:

- Agent
- Subagents
- Skills
- MCP Servers
- External Integrations

Save output to:

docs/risk-audit.md

Structure:

# Risk Audit

## Agent Setup Reviewed

## Lethal Trifecta Assessment

### Private Data

### Untrusted Content

### External Communication

## Redesign Or Risk Acceptance

## Risk Review

### Prompt Injection

### Data Leakage

### Insecure Or Unreviewed Code

### Supply Chain Risk

### Excessive Agency

## Controls

## Residual Risks

## Decision Record

## Revisit Triggers

Requirements:

- Clearly distinguish repository evidence from assumptions.
- Record a final decision.
- Record the rationale for the decision.

Then use interaction-logging and generate a JSONL audit record.

The audit record must reference:

- ops/README.md
- docs/risk-audit.md
- governance-review