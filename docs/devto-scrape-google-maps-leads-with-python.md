---
title: How to Scrape Google Maps Leads with Python (Without Building a Crawler)
published: false
description: Turn a keyword like "dentists in Austin" into Name / Phone / Website rows via a hosted Agent API—Python, TypeScript, and MCP included.
tags: python, typescript, api, webdev
# cover_image: https://your-cdn.example/cover.png
canonical_url: https://gmapsleadfinder.com/guides/how-to-scrape-google-maps-data
---

# How to Scrape Google Maps Leads with Python (Without Building a Crawler)

You need a list of local businesses: name, phone, website, address. You do **not** need another Playwright script that breaks every time Google changes the DOM.

This tutorial shows a practical path for developers:

1. Get an API key
2. Call a hosted Google Maps lead API from Python (or TypeScript)
3. Export rows to CSV
4. (Optional) Wire the same API into Cursor / Claude via MCP

We'll use **[GMaps Lead Finder](https://gmapsleadfinder.com)**'s Agent HTTP API and the open SDKs. The service runs the extraction for you; your code only creates jobs and reads results.

> **Disclaimer:** Use public business data responsibly and in line with applicable laws and platform terms. Empty email/social fields mean nothing public was found—don't invent contacts.

---

## What you'll build

A ~20-line script that turns:

```text
dentists in Austin TX
```

into rows like:

| Name | Phone | Website | …
|------|-------|---------|---
| …    | …     | …       | …

Same shape whether you use the web UI, Chrome extension, or Agent API—your export column preferences decide the exact headers.

---

## Prerequisites

- Python 3.10+ **or** Node.js 18+
- A [GMaps Lead Finder](https://gmapsleadfinder.com) account
- An Agent API key (`gmf_…`) — available on **Growth** and higher plans  
  → [Account → API key](https://gmapsleadfinder.com/account#api-key)

Verify the key:

```bash
export GMF_API_KEY=gmf_your_key_here

curl -sS https://gmapsleadfinder.com/api/v1/me \
  -H "Authorization: Bearer $GMF_API_KEY"
```

You should see JSON with `plan`, `creditsRemaining`, etc.

| Status | Meaning |
|--------|---------|
| `401` | Missing / invalid key |
| `403` | Plan can't use Agent API |
| `402` | No credits left |

Never commit the key. Keep it in env / a secret store—not in frontend code.

---

## Option A: Python SDK (recommended)

### Install

```bash
pip install google-maps-scraper-sdk
```

Import package name: `gmaps_scraper` (stdlib HTTP only—no extra deps).

### One-shot scrape

```python
from gmaps_scraper import Client

client = Client()  # reads GMF_API_KEY

me = client.me()
print("credits remaining:", me["creditsRemaining"])

rows = client.scrape("dentists in Austin TX")
for row in rows[:5]:
    print(row.get("Name"), row.get("Phone"), row.get("Website"))
```

`scrape()` creates the job, polls until `completed` / `partial` / `failed`, then paginates results for you.

### Export CSV with the CLI

```bash
gmaps-scraper scrape "dentists in Austin TX" --out leads.csv

# if the binary clashes with the npm CLI:
python -m gmaps_scraper scrape "dentists in Austin TX" --out leads.csv
```

### Multiple keywords (must be sequential)

The API allows **one in-flight job per user**. Parallel keywords → `409`. Run them one after another:

```python
from gmaps_scraper import Client

KEYWORDS = [
    "dentists in Austin TX",
    "coffee shops in Austin TX",
]

client = Client()
for keyword in KEYWORDS:
    print(f"=== {keyword} ===")
    rows = client.scrape(keyword)
    print(f"got {len(rows)} rows")
```

### Reviews / photos for one place

```python
reviews = client.scrape_reviews("https://maps.google.com/?cid=…")
photos = client.scrape_photos("ChIJ…")  # Place ID, Maps URL, or business_id
```

Same rule: one job at a time—wait for completion before the next.

---

## Option B: TypeScript / Node

```bash
npm install @gmapsleadfinder/google-maps-scraper
```

```ts
import { Client } from "@gmapsleadfinder/google-maps-scraper";

const client = new Client();

const me = await client.me();
console.log(me.creditsRemaining);

const rows = await client.scrape("dentists in Austin TX");
for (const row of rows.slice(0, 5)) {
  console.log(row.Name, row.Phone, row.Website);
}
```

CLI:

```bash
npx gmaps-scraper scrape "dentists in Austin TX" --out leads.csv
```

---

## Option C: Raw HTTP (any language)

Useful when you can't install an SDK—CI, shell, Go, etc.

```bash
# 1) Create job — exactly ONE keyword
JOB=$(curl -sS -X POST https://gmapsleadfinder.com/api/v1/jobs \
  -H "Authorization: Bearer $GMF_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{"keyword":"dentists in Austin TX"}')
echo "$JOB"
# → { "jobId": "…" }

# 2) Poll until completed | partial | failed
curl -sS "https://gmapsleadfinder.com/api/v1/jobs/$JOB_ID" \
  -H "Authorization: Bearer $GMF_API_KEY"

# 3) Page results (follow nextCursor until null)
curl -sS "https://gmapsleadfinder.com/api/v1/jobs/$JOB_ID/results?limit=100" \
  -H "Authorization: Bearer $GMF_API_KEY"
```

OpenAPI: [gmapsleadfinder.com/openapi-agent.yaml](https://gmapsleadfinder.com/openapi-agent.yaml)  
HTTP docs: [gmapsleadfinder.com/docs/api](https://gmapsleadfinder.com/docs/api)

---

## Option D: Remote MCP (Cursor / Claude / Codex)

If you're already in an AI coding client, skip the script and call the same API as MCP tools.

**Endpoint:** `https://gmapsleadfinder.com/mcp`  
**Auth:** `Authorization: Bearer gmf_…`

Cursor (`~/.cursor/mcp.json`):

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

Claude Code:

```bash
claude mcp add --transport http gmaps-finder https://gmapsleadfinder.com/mcp \
  --header "Authorization: Bearer $GMF_API_KEY"
```

Tools map 1:1 to HTTP: `gmaps_me`, `gmaps_create_job`, `gmaps_get_job`, `gmaps_get_results`, plus reviews/photos variants.

Ask the agent: *“Create a GMaps job for ‘plumbers in Denver’, wait until done, then summarize the first 20 rows.”*

Guide: [gmapsleadfinder.com/docs/agent](https://gmapsleadfinder.com/docs/agent)

---

## Hard rules (save yourself some 4xx)

1. **One keyword per leads job** — multi-keyword body → `400`
2. **One running job per user** — parallel jobs → `409`; run sequentially
3. **Credits** — roughly 1 credit ≈ 1 place; check `gmaps_me` / `client.me()` first
4. **Don't fabricate contacts** — missing `Emails` / socials means “not found publicly”

---

## When *not* to use this

| Need | Better fit |
|------|------------|
| Official Places data inside a product under Google Maps Platform terms | [Google Places API](https://developers.google.com/maps/documentation/places/web-service) |
| One-off manual list in the browser | [Online extractor](https://gmapsleadfinder.com/#extractor) (free tier includes lifetime starter credits) |
| Capture while browsing Maps | [Chrome extension](https://gmapsleadfinder.com/extensions/google-maps-extractor) |

DIY browser scrapers are fine for learning—but for recurring lead ops, a hosted job API is usually cheaper than maintaining selectors and proxies.

---

## Wrap-up

```text
keyword → create job → poll → paginate results → CSV / CRM
```

That's the whole loop. Pick Python or TypeScript if you want `scrape()` to handle polling; use MCP if you want the agent to drive it; use raw HTTP if you already have a client stack.

### Links

- Product: [gmapsleadfinder.com](https://gmapsleadfinder.com)
- Python: `pip install google-maps-scraper-sdk`
- TypeScript: `npm i @gmapsleadfinder/google-maps-scraper`
- Client kit (all languages): [github.com/google-maps-lead-scraper/google-maps-scraper](https://github.com/google-maps-lead-scraper/google-maps-scraper)
- Pricing (Agent API = Growth+): [gmapsleadfinder.com/pricing](https://gmapsleadfinder.com/pricing)

If you ship something cool with the SDK (batch cities, CRM sync, n8n node), drop a comment—happy to feature interesting setups.

---

*Questions or broken examples? Open an issue on the client kit repo or ping support@gmapsleadfinder.com.*
