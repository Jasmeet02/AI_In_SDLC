# Evaluation results

Using governance-review, this report evaluates the review benchmark in [ops/evals/results_template.csv](../ops/evals/results_template.csv) across the baseline runs and the single-variable prompt change in [ops/evals/README.md](../ops/evals/README.md).

## Executive summary

The benchmark compares three baseline runs against one prompt-only rerun. The only changed variable was the review prompt: the same reviewer, same cases, same context, and same model were reused. The stricter defect-only wording reduced false alarms and prevented the reviewer from treating harmless refactors or test-only diffs as production defects.

## Results table

| Run | Caught | Missed | False alarm | Clean | Assessment |
| --- | ---: | ---: | ---: | ---: | --- |
| 1 (v1 baseline) | 7 | 1 | 1 | 3 | Serviceable but noisy; one clean case was flagged and one subtle defect was missed. |
| 2 (v1 baseline) | 6 | 2 | 1 | 3 | Mixed quality; the reviewer caught the big issues but missed concurrency and silent-failure defects. |
| 3 (v1 baseline) | 5 | 3 | 1 | 3 | Weakest run; several actual defects were missed and a test-only patch was misread as a defect. |
| 4 (v2 tightened prompt) | 8 | 0 | 0 | 4 | Best run; all real defects were caught and no clean case was incorrectly flagged. |

### Aggregate summary

| Prompt version | Total scored rows | Caught | Missed | False alarm | Clean | Notes |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| v1 baseline | 36 | 18 | 6 | 3 | 9 | Good on obvious defects, weak on subtle correctness issues and non-defects. |
| v2 tightened prompt | 12 | 8 | 0 | 0 | 4 | Clear improvement in precision and recall, with no false positives. |

## Metrics

These counts are based on the reviewed worksheet in [ops/evals/results_template.csv](../ops/evals/results_template.csv) and the run structure documented in [ops/evals/README.md](../ops/evals/README.md).

- Defect-detection rate for baseline v1: 18/24 = 75% on defect-bearing cases.
- Missed-defect rate for baseline v1: 6/24 = 25%.
- False-alarm rate on clean cases for baseline v1: 3/9 = 33.3%.
- Defect-detection rate for v2: 8/8 = 100% on defect-bearing cases.
- Missed-defect rate for v2: 0/8 = 0%.
- False-alarm rate on clean cases for v2: 0/4 = 0%.

The most important operational signal is that the tighter prompt improved both precision and recall without any model or context change. Put differently, the reviewer became more reliable by being stricter about what counts as a real defect.

## Improvement analysis

The single-variable change was the prompt wording itself. The stronger prompt said, in effect, that the reviewer should only report real behavioral defects and ignore style, tests, and harmless refactors. That narrowed the signal and changed the reviewer behavior in exactly the way the benchmark suggested it should.

The improvements are visible in the cases that previously caused instability:

- Case 04 was consistently mislabeled as a defect in the baseline because it was only a behavior-preserving rename. Under v2 it was correctly treated as clean.
- Case 06 was misclassified as a defect in one baseline run even though it only added tests. Under v2 it stayed clean.
- Case 03, a subtle discount-rounding defect, was missed in multiple baseline runs and was caught under v2.
- Case 08, a concurrency regression from replacing a thread-safe collection with a plain dictionary, was missed once in v1 and caught under v2.
- Case 10, a broad catch block returning success on failure, was missed in baseline and caught under v2.

This pattern suggests the reviewer was not simply inconsistent; it was systematically over-including low-value observations and under-prioritizing deeper correctness defects when the prompt allowed too much latitude.

## Confidence assessment

The confidence labels in the worksheet are a useful check on reviewer calibration:

- In the baseline runs, the reviewer was often highly confident even when it was wrong. The false alarms on clean refactors and test-only diffs were sometimes reported with medium or high confidence, which is a calibration problem.
- The misses were often recorded as low confidence, which is closer to the truth but still unhelpful when the review is meant to catch real defects.
- Under v2, the results were consistently reported as high confidence for both true positives and clean cases, which is better aligned with the observed quality improvement.

This means the stricter prompt did not just reduce noise; it also improved confidence calibration by keeping the reviewer focused on actual defects rather than on speculative or style-focused concerns.

## Threats to validity

1. Small benchmark size: only 12 cases, repeated three times in the baseline and once for the changed-variable run. This is enough for directional evidence, not for a statistically robust generalization.
2. Learning effect: the same reviewer saw the benchmark multiple times, so the improved v2 run may have benefited from familiarity with the case set and the scoring rule.
3. Single-variable change only: this tells us a lot about prompt quality, but it does not show how the same pattern would behave under different models, other contexts, or production code review conditions.
4. Worksheet formatting issue: the final baseline row and the first v2 row in [ops/evals/results_template.csv](../ops/evals/results_template.csv) are concatenated on one physical line, which can trip naive CSV parsers and is a data-quality risk in any re-analysis.
5. Benchmark realism: these diffs are curated examples, so they are good for comparing reviewer behavior but not a substitute for live production review data.

## Where the reviewer was wrong or unhelpful

The reviewer was wrong or unhelpful in specific, repeatable ways:

- It treated harmless refactors as defects: case 04 in early baseline runs.
- It treated safe test-only changes as production issues: case 06 in one baseline run.
- It missed subtle arithmetic and concurrency regressions: case 03 and case 08.
- It missed silent-failure logic and broad exception handling bugs: case 10.
- It sometimes gave low-confidence or vague conclusions when the read should have been decisive, which reduces the actionability of the review.

The best evidence of improvement is that the v2 prompt eliminated the pattern of false positives, and the reviewer no longer confused refactors or tests with real defects.

## Governance review conclusion

Under governance-review, the evidence supports a straightforward conclusion: the prompt change was a meaningful improvement. The v2 version offered better defect detection without increasing false alarms, which is the operationally important tradeoff in a review benchmark. The remaining limitation is that the benchmark remains narrow and synthetic; for broader claims, the same measurement approach should be repeated on a larger, more realistic review corpus.
