# Measurement Plan

## Metric 1 - Lead Time For Changes

### Definition

The elapsed time from the first commit on a change to the point it is merged or released.

### Why It Matters

This measures delivery speed and the ability of the team to move work from idea to production. It is a core flow metric for high-performing engineering teams and a direct indicator of how quickly the team can respond to incidents and feature work.

### Data Source

Repository history for local git metadata; future GitHub pull request history for merged work.

### Query

- `git log --pretty=format:"%h|%ad|%s" --date=iso-strict`
- `git rev-list --count HEAD`
- Future GitHub source: `gh pr list --state merged --limit 100 --json number,createdAt,mergedAt`

### Collection Method

- For today: extract git commit timestamps from local repository history.
- For the rotation: collect merged PR timestamps from GitHub or the repository hosting system and compute elapsed time from change start to merge.
- Store the measurement in a simple CSV or dashboard table keyed by PR or branch.

### Baseline

Not measurable in the current repo today. The repository contains git history but no merged pull request metadata or branch merge timestamps in the checked-in files. Baseline to establish before the rotation starts: `TBD` using GitHub PR metadata or an equivalent tracker.

## Metric 2 - Change Failure Rate

### Definition

The percentage of changes that cause a failed build, test, deploy, or post-deploy validation event.

### Why It Matters

This metric shows whether the team is shipping changes that are stable and reliable. A high rate indicates weak validation or unsafe release practices.

### Data Source

CI/CD history in `ops/pipeline/run-timings.csv` and the pipeline definition in `ops/pipeline/ci-pipeline.yml`.

### Query

- `python -c "import csv; from pathlib import Path; rows=list(csv.DictReader(Path('ops/pipeline/run-timings.csv').open())); success=sum(1 for r in rows if r['conclusion']=='success'); failed=sum(1 for r in rows if r['conclusion']!='success'); total=len(rows); print(success, failed, total, round((failed/total)*100,2))"`

### Collection Method

- Read the CSV exported from CI pipeline step history.
- Count successful and unsuccessful step outcomes.
- Treat this as a CI pipeline failure proxy until deployment or production release data exists.

### Baseline

Measured today from repo evidence: 342 successful step outcomes, 36 failed step outcomes, 378 total recorded step outcomes, or a historical CI failure proxy of 9.52%. This is a proxy metric only; it is not a true production change failure rate because there is no release or deployment outcome record in the repository.

## Metric 3 - Review Turnaround Time

### Definition

The time between a change being opened for review and receiving a human review decision.

### Why It Matters

Slow review turnaround can become a bottleneck and increases cycle time. It is also a risk indicator for delayed feedback, blocked work, and overloaded reviewers.

### Data Source

GitHub PR metadata, review events, or a review tracker. This repository does not currently include a pull request review dataset.

### Query

- Future GitHub source: `gh pr list --state open --json number,createdAt,updatedAt`
- Future GitHub review event source: `gh pr view <number> --comments`

### Collection Method

- Pull PR creation and review timestamps from the repository hosting platform.
- Compute the difference between the initial PR creation time and the first human review decision.

### Baseline

Not measurable in the current repo today. The repo contains no PR review data, no GitHub metadata, and no review-event logs. Baseline to establish before the rotation starts: `TBD` from GitHub API, Azure DevOps work items, or a review tracking system.

## Measurements Captured Today

1. `git rev-list --count HEAD` = 10 commits in the current repository history.
2. `python ... run-timings.csv` = 342 successful CI step outcomes, 36 failed CI step outcomes, 378 total step observations, representing a 9.52% historical CI failure proxy.
3. `python ... run-timings.csv` = average recorded CI step duration = 99.5 seconds; maximum recorded step duration = 647.0 seconds.
4. `git log --pretty=format:"%h|%ad|%s" --date=iso-strict --max-count=10` confirms the repository contains local commit history and timestamps for the current project.

## Assumptions

- The repository is a demo environment and does not include production deployment or release telemetry.
- The available CI timing CSV is a historical operational artifact, not a full release record.
- The team will establish measurement collection from GitHub or the repository host before the rotation begins.
- The current repo is sufficient to establish initial baselines for tooling and workflow health, but not for release-traceable engineering metrics.

## Limitations

- There is no GitHub PR history or merge metadata checked into the repo.
- There is no production deployment data, release log, or review-tracking dataset in the repository.
- The current `ops/pipeline/run-timings.csv` is a step-level pipeline artifact, not a direct change-failure or review-turnaround dataset.
- The available repository history is short and not representative of a mature production organization’s full lifecycle metrics.

## Decision Record

The team should treat this as a bootstrap measurement plan. Use repository evidence where available and set the formal baseline for Lead Time For Changes and Review Turnaround Time from GitHub PR data before the rotation starts. For the current repo, the only immediately measurable items are git history and CI history. The plan is therefore to collect a valid baseline from local repo metadata and CI pipeline logs now, then add the missing PR and review data sources as part of the rotation setup.

### Human validation performed

Yes. The values above were validated by running the repo commands listed in this plan against the actual git history and pipeline CSV in the workspace.

## Summary

This plan establishes the minimum governance baseline for the rotation and clearly separates what can be measured today from what requires additional repository or workflow data. The metrics are intentionally conservative: we measure what is available now, and we define the missing data sources required to make the baseline operationally meaningful before the rotation begins.
