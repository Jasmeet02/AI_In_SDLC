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

- agent
- agent_name
- agent_type
- mode

### Request

- context
- request
- related_ids
- parent_task

### Skills

- skills_used

### Subagents

- subagents_used

### MCP Tools

- mcp_tools_used

### Other Tools

- tools_used

### Suggestions and Human Oversight

- suggestion
- human_decision
- action_verification

### Limitations

- limitation

### Status and Deliverables

- status
- artifacts

## JSONL Template

Return a single valid JSON object suitable for appending to:

../logs/ai-assistance-log.jsonl

{
  "id": "<unique-id>",
  "timestamp": "<ISO-8601 UTC timestamp>",
  "stage": "<setup | architecture-analysis | workflow-analysis | testing | governance | evaluation>",
  "agent": "<agent_name | agent>",
  "agent_name": "<agent_name>",
  "agent_type": "<built-in | custom | sub-agent>",
  "ai_tool": "<ai_tool_name>",
  "model": "<model_name>",
  "mode": "<ask | plan | agent | edit>",
  "related_ids": [],
  "parent_task": null,
  "context": "<task_context>",
  "request": "<user_request>",
  "skills_used": [
    {
      "name": "<skill_name>",
      "type": "<built-in | custom>",
      "purpose": "<skill_purpose>",
      "invocation": "<automatic | explicit>"
    }
  ],
  "subagents_used": [
    {
      "name": "<subagent_name>",
      "type": "<built-in | custom>",
      "task": "<delegated_task>",
      "status": "<completed | failed | partial>"
    }
  ],
  "mcp_tools_used": [
    {
      "server": "<mcp_server_name>",
      "tool": "<mcp_tool_name>",
      "purpose": "<purpose>"
    }
  ],
  "tools_used": [
    "<tool_name>"
  ],
  "suggestion": "<suggestion_or_null>",
  "human_decision": null,
  "action_verification": "<validation_summary>",
  "limitation": null,
  "status": "<applied | partial | rework | failed>",
  "artifacts": [
    "<artifact_path_or_name>"
  ]
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