Use architecture-analyst.

Using the validated as-built architecture and repository code:

Generate one runnable architecture test.

Rule:

Controllers must not directly depend on repositories.

Requirements:

- Prefer NetArchTest.
- Include complete compilable code.
- Include required package references.
- Include execution command.
- Include expected PASS condition.
- Include expected FAIL condition.
- Cite the repository structure and code evidence that justify the rule.
- Explain why the rule protects the architecture.

Save output to:

tests/ArchitectureTests/DependencyRulesTests.cs

Create:

docs/architecture-test.md

Structure:

# Architecture Test

## Architectural Rule

## Rationale

## Code

## Execution

dotnet test

## Expected Result

## Evidence

Then use interaction-logging.

Generate a JSONL audit record referencing:

- tests/ArchitectureTests/DependencyRulesTests.cs
- docs/architecture-test.md
