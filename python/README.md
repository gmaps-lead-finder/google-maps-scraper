# Google Maps Scraper

<p align="center">
  <img src="https://raw.githubusercontent.com/gmaps-lead-finder/google-maps-scraper/main/google-maps-scraper.png" alt="Google Maps Lead Scraper" width="100%" />
</p>

**Google Maps Extractor · Google Maps Lead Scraper · Google Maps Lead Extractor**

Python SDK to scrape Google Maps places (leads), **reviews**, and **photos** through a hosted Agent HTTP API. Powered by [GMaps Lead Finder](https://gmapsleadfinder.com). This is **not** a local browser crawler — jobs run in the cloud scrape-and-enrich pipeline.

- **PyPI:** [`google-maps-scraper-sdk`](https://pypi.org/project/google-maps-scraper-sdk/)
- **Import:** `gmaps_scraper`
- **CLI:** `gmaps-scraper` (or `python -m gmaps_scraper`)

## Get an API key

1. Sign in at [gmapsleadfinder.com](https://gmapsleadfinder.com).
2. Use a plan with Agent API (**Growth** or higher) — see [Pricing](https://gmapsleadfinder.com/pricing).
3. Open [Account → API key](https://gmapsleadfinder.com/account#api-key) and copy your `gmf_…` key.

```bash
export GMF_API_KEY=gmf_your_key_here
```

Never commit API keys or embed them in public frontends.

## Install

```bash
pip install google-maps-scraper-sdk
```

Requires **Python 3.10+**. Uses the standard library only (no third-party HTTP dependencies).

## Configuration

| Variable | Required | Description |
|----------|----------|-------------|
| `GMF_API_KEY` | Yes | Bearer key (`gmf_…`) |
| `GMF_BASE_URL` | No | Default `https://gmapsleadfinder.com` |

You can also pass credentials in code:

```python
from gmaps_scraper import Client

client = Client(api_key="gmf_…", base_url="https://gmapsleadfinder.com")
```

## Quickstart

```python
from gmaps_scraper import Client

client = Client()  # reads GMF_API_KEY

me = client.me()
print(me["plan"], me["creditsRemaining"])

# Google Maps scraper helper: create job → poll → fetch all rows
rows = client.scrape("dentists in Austin TX")
for row in rows[:5]:
    print(row.get("Name"), row.get("Phone"), row.get("Website"), row.get("Emails"))

# Single-place reviews / photos (run sequentially — one in-flight job per user)
reviews = client.scrape_reviews("https://maps.google.com/?cid=…")
photos = client.scrape_photos("ChIJ…")  # Place ID, URL, or business_id
```

CLI:

```bash
gmaps-scraper me
gmaps-scraper scrape "dentists in Austin TX" --out leads.json
gmaps-scraper scrape "dentists in Austin TX" --out leads.csv
```

## Client API

### `me()`

Returns plan and credits: `plan`, `creditsLimit`, `creditsUsed`, `creditsRemaining`, plus optional `permanentCredits` / `subscriptionCreditsRemaining` (permanent packs never expire; pack-only accounts get Starter features; Agent API still requires Growth+).

### `create_job(keyword)`

Queues a single-keyword Google Maps lead scrape. Body is exactly one search query (city + category works best).

Returns: `jobId`, `keywordCount`, `creditsRemaining`.

### `get_job(job_id)`

Poll job status until `completed`, `partial`, or `failed` (also `queued` / `running` while in progress).

### `get_results(job_id, *, limit=100, cursor=None)`

Paginated place rows as `{ column_header: string }` objects. Follow `nextCursor` until `null`. `limit` is 1–500.

### `scrape(keyword, *, poll_interval_ms=2000, timeout_ms=600_000, result_limit=100)`

High-level Google Maps extractor: creates a job, polls to a terminal status, then returns **all** result rows.

### `create_reviews_job(place)` / `get_reviews_job(job_id)` / `get_reviews_results(job_id, *, limit=100, cursor=None)`

Single-place reviews job. `place` is a Maps URL, share short link (`maps.app.goo.gl` / `goo.gl/maps/…`), or `business_id`.

### `scrape_reviews(place, *, poll_interval_ms=2000, timeout_ms=600_000, result_limit=100)`

Create reviews job → poll → all review rows.

### `create_photos_job(place)` / `get_photos_job(job_id)` / `get_photos_results(job_id, *, limit=100, cursor=None)`

Single-place photos job. `place` is a Maps URL, share short link, `business_id`, or Place ID.

### `scrape_photos(place, *, poll_interval_ms=2000, timeout_ms=600_000, result_limit=100)`

Create photos job → poll → all photo rows.

## CLI reference

```bash
gmaps-scraper me
gmaps-scraper scrape "<keyword>" [--out path.json|path.csv]
gmaps-scraper scrape "<keyword>" [--poll-interval-ms 2000] [--timeout-ms 600000]

# Prefer module form if the npm CLI is also installed (same binary name):
python -m gmaps_scraper me
python -m gmaps_scraper.cli scrape "coffee shops in Austin TX" --out leads.csv
```

## Errors

| HTTP | Exception | Meaning |
|------|-----------|---------|
| 400 | `BadRequestError` | Invalid request (e.g. not exactly one keyword) |
| 401 | `AuthenticationError` | Missing or invalid API key |
| 402 | `InsufficientCreditsError` | No credits remaining |
| 403 | `PlanNotAllowedError` | Plan cannot use Agent API (need Growth+) |
| 404 | `NotFoundError` | Job not found |
| 409 | `JobConflictError` | Another job is already running for this user |
| — | `TimeoutError` | Local poll timeout in `scrape()` |
| other | `ApiError` | Base / unexpected API failure |

## Limits

- Exactly **one keyword** per leads job; exactly **one place** per reviews or photos job.
- Only **one running job** per user at a time (web UI, HTTP API, and MCP share the lock) → `409` if busy. For multiple keywords or places, scrape **sequentially**.
- **1 credit = 1 place row**; enrich is included.
- Agent API requires **Growth** or higher.
- Empty email/social cells mean nothing public was found — contacts are never invented.

## Links

| Resource | URL |
|----------|-----|
| Website | https://gmapsleadfinder.com |
| Pricing | https://gmapsleadfinder.com/pricing |
| HTTP API docs | https://gmapsleadfinder.com/docs/api |
| Agent & MCP | https://gmapsleadfinder.com/docs/agent |
| OpenAPI | https://gmapsleadfinder.com/openapi-agent.yaml |
| Get API key | https://gmapsleadfinder.com/account#api-key |
| PyPI | https://pypi.org/project/google-maps-scraper-sdk/ |
| Source / examples | https://github.com/gmaps-lead-finder/google-maps-scraper |

Prefer Remote MCP for Claude / Cursor / Codex? See the [Agent docs](https://gmapsleadfinder.com/docs/agent).

## License

MIT
