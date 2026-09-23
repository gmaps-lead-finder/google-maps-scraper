# Remote MCP

Hosted MCP endpoint for AI clients. Live guide: [gmapsleadfinder.com/docs/agent](https://gmapsleadfinder.com/docs/agent) · Agents hub: [gmapsleadfinder.com/agents](https://gmapsleadfinder.com/agents)

## Endpoint

- URL: `https://gmapsleadfinder.com/mcp`
- Transport: Streamable HTTP (JSON-RPC)
- Auth: `Authorization: Bearer gmf_<key>`
- Server name (suggested): `gmaps-finder`

Same plan and credit rules as the HTTP Agent API (Growth+).

## Tools

| Tool | Purpose |
|------|---------|
| `gmaps_me` | Plan & credits |
| `gmaps_create_job` | Create one-keyword job |
| `gmaps_get_job` | Job status |
| `gmaps_get_results` | Paginated results |
| `gmaps_create_reviews_job` | Create single-place reviews job |
| `gmaps_get_reviews_job` | Reviews job status |
| `gmaps_get_reviews_results` | Paginated review rows |
| `gmaps_create_photos_job` | Create single-place photos job |
| `gmaps_get_photos_job` | Photos job status |
| `gmaps_get_photos_results` | Paginated photo rows |

## Install snippets

### Claude Code

```bash
claude mcp add --transport http gmaps-finder https://gmapsleadfinder.com/mcp \
  --header "Authorization: Bearer $GMF_API_KEY"
```

### Cursor

Add to MCP config (e.g. `~/.cursor/mcp.json`):

```json
{
  "mcpServers": {
    "gmaps-finder": {
      "url": "https://gmapsleadfinder.com/mcp",
      "headers": {
        "Authorization": "Bearer gmf_your_key_here"
      }
    }
  }
}
```

### Codex

```toml
[mcp_servers.gmaps-finder]
url = "https://gmapsleadfinder.com/mcp"
http_headers = { Authorization = "Bearer gmf_your_key_here" }
```

### Generic MCP client

```json
{
  "mcpServers": {
    "gmaps-finder": {
      "url": "https://gmapsleadfinder.com/mcp",
      "headers": {
        "Authorization": "Bearer gmf_your_key_here"
      }
    }
  }
}
```

## Agent tips

- Create one job, wait until complete, then fetch results (or ask the agent to poll).
- For reviews, use `gmaps_create_reviews_job` with a place URL, share short link (`maps.app.goo.gl` / `goo.gl/maps/…`), or `business_id`, then poll and fetch via the reviews tools.
- For photos, use `gmaps_create_photos_job` with a place URL, share short link, `business_id`, or Place ID, then poll and fetch via the photos tools.
- For multiple keywords or places, run jobs **one after another** — concurrent jobs return `409`.
- Never paste API keys into shared transcripts; use env vars or the product’s Agents UI when available.

## Related

- [Getting an API key](getting-api-key.md)
- [HTTP API](http-api.md)
- [AGENTS.md](../AGENTS.md)
