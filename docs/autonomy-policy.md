# Autonomy Policy

## Workflow

Dependency bumps are routine but operationally significant changes. They include patch and minor version updates to runtime libraries, frameworks, build tools, security packages, and platform dependencies. Because dependency changes can silently alter behavior, security posture, build output, and runtime compatibility, they require explicit guardrails even when they appear low-risk.

This policy is intended for production software delivery and assumes a team with six engineers, two of whom are new to the codebase. The goal is to increase autonomy without increasing release risk.

## Autonomy Levels

Level 0: Human-led
Level 1: Guided autonomy
Level 2: Trusted autonomy
Level 3: Release authority

## Tasks By Level

### Level 0: Human-led
- Dependency bump proposals and planning.
- Risk classification and dependency impact review.
- Security and compatibility triage.
- Release approval decisions.

### Level 1: Guided autonomy
- Patch-level dependency updates that are already known-safe.
- Security advisories with no API surface change.
- Small dependency-only updates with existing test coverage.
- Open a pull request with a summary of dependency impact and validation evidence.

### Level 2: Trusted autonomy
- Minor version bumps for dependencies already approved by the team.
- Package updates backed by passing CI and targeted regression checks.
- Low-risk dependency maintenance in active repos with known release patterns.
- Merge after peer review and automated checks pass.

### Level 3: Release authority
- Broad dependency refreshes across multiple services.
- Coordinated upgrades that touch build tooling, runtime versions, or release pipelines.
- Decisions to bypass default controls when justified by a documented risk assessment.
- Final sign-off for production deployment windows.

## Safety Controls

### Level 0
- No automatic execution.
- Human review required for every decision.
- Explicit approval before merge or release.

### Level 1
- Only patch-level or low-risk dependency changes.
- No transitive major-version upgrades without review.
- Requires changelog inspection and dependency diff review.
- Requires CI, build, and targeted tests.

### Level 2
- Only dependencies with existing production usage and clear upgrade history.
- Requires peer review and a clean risk note.
- No release if known blocker remains unresolved.
- Deploy only to a controlled environment or release window.

### Level 3
- Requires a written change brief, rollback plan, and release checklist.
- Must include dependency blast radius, compatibility impacts, and risk owners.
- Requires final approval by engineering leadership or release owner.

## Validation Requirements

All dependency bump work must include:
- Current dependency list and version diff.
- Security review for known issues or advisories.
- Build and test execution for the affected project or solution.
- Regression checks for runtime and integration behavior.
- Clear evidence that the changed dependency does not alter expected contracts.

For production delivery, the minimum bar is:
- unit tests pass,
- relevant integration or smoke tests pass,
- dependency changelog reviewed,
- release note or risk note captured.

## Escalation Conditions

Escalate from the current level to human review or a lower level when any of the following occur:
- transitive major-version change,
- runtime or framework version jump,
- security issue with a non-trivial exploit path,
- failing or flaky tests after the bump,
- compatibility issue with platform assumptions,
- undocumented breakage in build or deployment pipelines,
- high blast radius across multiple services or shared libraries.

## Evidence Required To Move Up A Level

To move from Level 0 to 1, 1 to 2, or 2 to 3, the engineer must provide:
- dependency diff and rationale,
- test and build evidence,
- security or compatibility review summary,
- prior examples or patterns from similar upgrades,
- a human review record documenting approval.

Promotion is not based on confidence alone; it requires evidence.

## Approval Authority

- Level 0: individual engineer or human approver with team sign-off.
- Level 1: senior engineer or designated reviewer.
- Level 2: engineering lead or release owner.
- Level 3: engineering leadership with release approval authority.

The approval authority must be recorded on the pull request or release record.

## Day-One Autonomy Level

Day-one autonomy level: Level 1.

## Day-One Rationale

The team has two new engineers and is delivering production software. That means the day-one policy should maximize safe, bounded autonomy without assuming deep familiarity with the codebase or dependency behavior. Level 1 allows bounded dependency bump work under explicit review but prevents broad or high-risk updates from being performed without senior oversight.

This is the right starting level because it balances production safety, onboarding needs, and learning velocity. It avoids the failure mode where engineers with limited system context approve broad dependency changes without effective validation.
