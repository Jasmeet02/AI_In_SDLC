Use architecture-analyst.

Use governance-review.

Create a one-page autonomy policy for the following workflow:

Dependency Bumps

Requirements:

Define multiple autonomy levels and identify which tasks can be performed at each level.

Include:

- Workflow Description
- Autonomy Levels
- Tasks Allowed at Each Level
- Safety Controls for Each Level
- Validation Requirements
- Escalation Conditions
- Evidence Required To Move To A Higher Level
- Approval Authority For Level Promotion
- Day-One Autonomy Level
- Rationale For Day-One Level

Assumptions:

- Two of six engineers are new to the codebase.
- The policy must be suitable for production software delivery.

Save output to:

docs/autonomy-policy.md

Structure:

# Autonomy Policy

## Workflow

## Autonomy Levels

## Tasks By Level

## Safety Controls

## Validation Requirements

## Escalation Conditions

## Evidence Required To Move Up A Level

## Approval Authority

## Day-One Autonomy Level

## Day-One Rationale

Generate a JSONL audit record using interaction-logging.

The audit record must reference:

- docs/autonomy-policy.md
- governance-review
- interaction-logging