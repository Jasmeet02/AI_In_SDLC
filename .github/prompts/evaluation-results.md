Use review-evaluator.

Use governance-review.

Objective:
Evaluate the effectiveness of the review process using the evaluation cases in ops/evals.

Create:

1. Two additional evaluation cases based on real defects.
   - One must contain a genuine defect.
   - One must be a clean case with no defect.

2. An evaluation report.

Run the evaluation set three times using the baseline configuration:

- Baseline Run 1
- Baseline Run 2
- Baseline Run 3

Then change exactly ONE of the following:

- Review prompt
- Model
- Context file

Document the change and run the evaluation set again:

- Changed Variable Run

For each case capture:

- Input
- Expected Outcome
- Actual Outcome
- Success/Failure
- Notes

Calculate and report:

- Total Cases
- Issues Expected
- Issues Caught (True Positives)
- Issues Missed (False Negatives)
- False Alarms (False Positives)
- Caught Rate
- Missed Rate
- False Alarm Rate

Assess:

- Whether the change improved performance
- Whether any metrics regressed
- Confidence level given the sample size
- Threats to validity and evaluation limitations

Include:

- A brief summary of the two additional cases
- A comparison of baseline versus changed-variable results
- A section describing where the reviewer or agent was wrong or unhelpful

Save output to:

docs/evaluation-results.md

Structure:

# Evaluation Cases

## Case 1 - Defect Case

## Case 2 - Clean Case

# Baseline Results

## Run 1

## Run 2

## Run 3

# Metrics

- Total Cases
- Issues Caught
- Issues Missed
- False Alarms
- Caught Rate
- Missed Rate
- False Alarm Rate

# Changed Variable

## What Changed

## Reason For Change

## Re-run Results

# Comparison

## Baseline vs Changed Variable

# Assessment

## Improvement Analysis

## Confidence Assessment

## Threats To Validity

## Where The Reviewer Was Wrong Or Unhelpful

# Conclusion

Then use interaction-logging.

Generate a JSONL audit record referencing:

- docs/evaluation-results.md
- governance-review
- review-evaluator
- interaction-logging

Record:
- evaluation stage
- baseline runs
- changed variable
- calculated metrics
- human validation
- generated artifacts