Use architecture-analyst.

Use governance-review.

Create a measurement plan.

Requirements:

Establish baseline metrics before the rotation starts.

Include the following metrics:

1. Lead Time For Changes
2. Change Failure Rate
3. Review Turnaround Time

For each metric provide:

- Definition
- Why It Matters
- Data Source
- Exact Query, Command, Report, or Repository Source Used
- Collection Method
- Baseline Value

Then:

Attempt to capture actual values for any metrics available today from:

- Repository history
- Pull request history
- Commit history
- CI/CD history
- Workflow run history

Capture at least two actual measurements.

If a metric cannot be captured:

- Explain why.
- Identify the missing data source.
- Explain how it would be collected in the future.

Record:

- Assumptions
- Limitations
- Human validation performed

Save output to:

docs/measurement-plan.md

Structure:

# Measurement Plan

## Metric 1 - Lead Time For Changes

### Definition

### Why It Matters

### Data Source

### Query

### Collection Method

### Baseline

## Metric 2 - Change Failure Rate

### Definition

### Why It Matters

### Data Source

### Query

### Collection Method

### Baseline

## Metric 3 - Review Turnaround Time

### Definition

### Why It Matters

### Data Source

### Query

### Collection Method

### Baseline

## Measurements Captured Today

## Assumptions

## Limitations

## Decision Record

Requirements:

- Use repository evidence whenever available.
- Record actual measured values for at least two metrics.
- Clearly identify any metrics that could not be measured today.

Then use interaction-logging and generate a JSONL audit record.

The audit record must reference:

- docs/measurement-plan.md
- governance-review