---
title: Turn a Google Maps Keyword into CSV with Python (Hosted API + MCP)
published: false
description: A short tutorial on creating a Maps lead job, polling status, and exporting Name / Phone / Website rows—plus optional MCP wiring.
tags: python, api, mcp, automation, webdev
---

# Turn a Google Maps Keyword into CSV with Python (Hosted API + MCP)

You need local business rows: name, phone, website, address. Maintaining your own Maps crawler usually means chasing DOM changes and proxy issues.

A cleaner pattern for scripts and agents is:

1. Create a job with one keyword  
2. Poll until it finishes  
3. Page through results  
4. Write CSV  

This tutorial uses a hosted Agent HTTP API (and optional Remote MCP) so your code only orchestrates jobs. The examples below use the open Python package `google-maps-scraper-sdk` (`import gmaps_scraper`) against `gmapsleadfinder.com`.

> Use public listing data responsibly and follow applicable laws / platform terms. If an email or social field is empty, nothing public was found—don't invent contacts.

---

## What you'll build

Input:

```text
dentists in Austin TX
```

Output: a list of dicts (or a CSV) with columns such as `Name`, `Phone`, `Website` (exact headers depend on your export preferences).

---

## Setup

Requirements:

- Python 3.10+
- An API key in the form `gmf_…` (Growth plan or higher on the hosted service)
- Env var: `GMF_API_KEY`

Check auth:

```bash
export GMF_API_KEY=gmf_your_key_here

curl -sS https://gmapsleadfinder.com/api/v1/me \
  -H "Authorization: Bearer $GMF_API_KEY"
```

Useful status codes:

| Code | Meaning |
|------|---------|
| `401` | Bad or missing key |
| `403` | Plan cannot use the Agent API |
| `402` | Out of credits |
| `409` | Another job is already running |

Keep keys in env or a secret store—never in frontend bundles or git.

---

## Install the SDK

```bash
pip install google-maps-scraper-sdk
```

### One keyword

```python
from gmaps_scraper import Client

client = Client()  # reads GMF_API_KEY

me = client.me()
print("credits remaining:", me["creditsRemaining"])

rows = client.scrape("dentists in Austin TX")
for row in rows[:5]:
    print(row.get("Name"), row.get("Phone"), row.get("Website"))
```

`scrape()` creates the job, polls until `completed` / `partial` / `failed`, then follows cursors for you.

### CSV via CLI

```bash
python -m gmaps_scraper scrape "dentists in Austin TX" --out leads.csv
```

### Several keywords

Only **one job** may run at a time per account. Loop sequentially:

```python
from gmaps_scraper import Client

KEYWORDS = [
    "dentists in Austin TX",
    "coffee shops in Austin TX",
]

client = Client()
for kw in KEYWORDS:
    rows = client.scrape(kw)
    print(kw, "→", len(rows), "places")
```

On this API, lead usage is typically **one credit per place**. Reviews/photos jobs use their own credit rules.

---

## Optional: Remote MCP

If you work in Cursor or Claude with MCP support, you can skip the script:

1. Add remote MCP URL: `https://gmapsleadfinder.com/mcp`
2. Authenticate with `Authorization: Bearer gmf_…`
3. Call tools that map 1:1 to HTTP (`gmaps_me`, `gmaps_create_job`, `gmaps_get_results`, …)

Same credit pool as the HTTP path.

---

## Design notes (why this shape)

- **One keyword per job** keeps runs predictable and avoids messy multi-query payloads.  
- **Poll + cursor** is boring but reliable for long extractions.  
- **Hosted extraction** moves DOM / anti-bot churn out of your repo.  

If you need official Places data inside a product under Google Maps Platform terms, use Google's Places API instead. If you need arbitrary-site scraping, use a general scraper platform. This path is for Maps **lead lists**.

---

## Wrap-up

Orchestrate jobs; don't maintain Maps selectors. Full HTTP/OpenAPI docs live under `/docs` on `gmapsleadfinder.com` if you want lower-level `fetch` instead of the SDK.
