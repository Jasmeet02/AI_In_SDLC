---
name: interaction-logging
description: Generate structured JSONL audit records.
---

# Interaction Logging

Generate a JSONL audit record suitable for:

logs/ai-assistance-log.jsonl

## Capture

### AI Tool

- ai_tool
- model

### Agent Details

- agent_name
- agent_type
- mode

### Request

- request
- follow_up_interactions

### Skills

- skills_used

### Subagents

- subagents_used

### MCP Tools

- mcp_tools_used

### Other Tools

- tools_used

### AI Actions

- ai_actions

### Deliverables

- artifacts

### Files

- affected_files

### Human Oversight

- human_decision
- action_verification

### Execution Evidence

- execution_command
- test_results

### Limitations

- limitations

### Outcome

- outcome

## JSONL Template

Return a single valid JSON object suitable for appending to:

../logs/ai-assistance-log.jsonl

{
  "id": "<unique-id>",
  "timestamp": "<ISO-8601 UTC timestamp>",
  "stage": "<architecture-analysis|workflow-analysis|testing|governance|evaluation>",
  "agent_name": "<agent-name>",
  "agent_type": "<custom|sub-agent|built-in>",
  "ai_tool": "<tool-name>",
  "model": "<model-name>",
  "mode": "<ask|plan|agent|edit>",
  "request": "<user-request>",
  "follow_up_interactions": [],
  "skills_used": [
    {
      "name": "<skill-name>",
      "type": "<custom|built-in>",
      "purpose": "<purpose>"
    }
  ],
  "subagents_used": [
    {
      "name": "<subagent-name>",
      "task": "<delegated-task>",
      "status": "<completed|partial|failed>"
    }
  ],
  "mcp_tools_used": [
    {
      "server": "<mcp-server>",
      "tool": "<tool-name>",
      "purpose": "<purpose>"
    }
  ],
  "tools_used": [
    "<tool-name>"
  ],
  "ai_actions": [
    "<action>"
  ],
  "artifacts": [
    {
      "name": "<artifact-name>",
      "path": "<artifact-path>",
      "type": "<document|diagram|code|test|video>"
    }
  ],
  "affected_files": [
    "<file-path>"
  ],
  "human_decision": "<manual decision or correction>",
  "action_verification": "<validation performed>",
  "execution_command": "<command executed>",
  "test_results": {
    "passed": 0,
    "failed": 0,
    "notes": "<notes>"
  },
  "limitations": [
    "<limitation-or-assumption>"
  ],
  "outcome": "<successful|partial|rework>"
}

## Rules

- Return only a valid JSON object.
- Do not wrap the JSON in markdown code fences.
- One interaction equals one JSON object.
- The JSON must be appendable to a JSONL file.
- Capture actual skills, subagents, MCP tools and artifacts used.
- Record human corrections and validation whenever performed.
- Record test execution results when available.
- For Mermaid diagram generation, record Mermaid MCP under `mcp_tools_used`.