Use architecture-analyst.

Generate one runnable architecture test.

Rule:

Controllers must not directly depend on repositories.

Requirements:

- Prefer NetArchTest.
- Include complete code.
- Include execution command.
- Include expected result.

Save output to:

tests/ArchitectureTests/DependencyRulesTests.cs

Structure:

# Architecture Test

# Execution

dotnet test

Then use interaction-logger and generate a JSONL audit record.