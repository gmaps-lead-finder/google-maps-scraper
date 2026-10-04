# Google Maps Scraper

<p align="center">
  <img src="https://raw.githubusercontent.com/gmaps-lead-finder/google-maps-scraper/main/google-maps-scraper.png" alt="Google Maps Lead Scraper" width="100%" />
</p>

**Google Maps Extractor · Google Maps Lead Scraper · Google Maps Lead Extractor**

TypeScript / Node.js SDK to scrape Google Maps places (leads), **reviews**, and **photos** through a hosted Agent HTTP API. Powered by [GMaps Lead Finder](https://gmapsleadfinder.com). This is **not** a local browser crawler — jobs run in the cloud scrape-and-enrich pipeline.

- **npm:** [`@gmapsleadfinder/google-maps-scraper`](https://www.npmjs.com/package/@gmapsleadfinder/google-maps-scraper)
- **Import:** `@gmapsleadfinder/google-maps-scraper`
- **CLI:** `npx gmaps-scraper` (or global `gmaps-scraper`)

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
npm install @gmapsleadfinder/google-maps-scraper
```

Requires **Node.js 18+** (uses global `fetch`).

## Configuration

| Variable | Required | Description |
|----------|----------|-------------|
| `GMF_API_KEY` | Yes | Bearer key (`gmf_…`) |
| `GMF_BASE_URL` | No | Default `https://gmapsleadfinder.com` |

You can also pass credentials in code:

```ts
import { Client } from "@gmapsleadfinder/google-maps-scraper";

const client = new Client({
  apiKey: "gmf_…",
  baseUrl: "https://gmapsleadfinder.com",
});
```

## Quickstart

```ts
import { Client } from "@gmapsleadfinder/google-maps-scraper";

const client = new Client(); // reads GMF_API_KEY

const me = await client.me();
console.log(me.plan, me.creditsRemaining);

// Google Maps scraper helper: create job → poll → fetch all rows
const rows = await client.scrape("dentists in Austin TX");
for (const row of rows.slice(0, 5)) {
  console.log(row.Name, row.Phone, row.Website, row.Emails);
}

// Single-place reviews / photos (run sequentially — one in-flight job per user)
const reviews = await client.scrapeReviews("https://maps.google.com/?cid=…");
const photos = await client.scrapePhotos("ChIJ…"); // Place ID, URL, or business_id
```

CLI:

```bash
npx gmaps-scraper me
npx gmaps-scraper scrape "dentists in Austin TX" --out leads.json
npx gmaps-scraper scrape "dentists in Austin TX" --out leads.csv
```

## Client API

### `me()`

Returns plan and credits: `plan`, `creditsLimit`, `creditsUsed`, `creditsRemaining`, plus optional `permanentCredits` / `subscriptionCreditsRemaining` (permanent packs never expire; pack-only accounts get Starter features; Agent API still requires Growth+).

### `createJob(keyword)`

Queues a single-keyword Google Maps lead scrape. Body is exactly one search query (city + category works best).

Returns: `jobId`, `keywordCount`, `creditsRemaining`.

### `getJob(jobId)`

Poll job status until `completed`, `partial`, or `failed` (also `queued` / `running` while in progress).

### `getResults(jobId, { limit = 100, cursor })`

Paginated place rows as `{ [columnHeader]: string }` objects. Follow `nextCursor` until `null`. `limit` is 1–500.

### `scrape(keyword, { pollIntervalMs = 2000, timeoutMs = 600_000, resultLimit = 100 })`

High-level Google Maps extractor: creates a job, polls to a terminal status, then returns **all** result rows.

### `createReviewsJob(place)` / `getReviewsJob(jobId)` / `getReviewsResults(jobId, options)`

Single-place reviews job. `place` is a Maps URL, share short link (`maps.app.goo.gl` / `goo.gl/maps/…`), or `business_id`.

### `scrapeReviews(place, options?)`

Create reviews job → poll → all review rows. Same options as `scrape()`.

### `createPhotosJob(place)` / `getPhotosJob(jobId)` / `getPhotosResults(jobId, options)`

Single-place photos job. `place` is a Maps URL, share short link, `business_id`, or Place ID.

### `scrapePhotos(place, options?)`

Create photos job → poll → all photo rows. Same options as `scrape()`.

## CLI reference

```bash
npx gmaps-scraper me
npx gmaps-scraper scrape "<keyword>" [--out path.json|path.csv]
npx gmaps-scraper scrape "<keyword>" [--poll-interval-ms 2000] [--timeout-ms 600000]

# After: npm install -g @gmapsleadfinder/google-maps-scraper
gmaps-scraper me
```

If you also installed the Python package globally, both expose a `gmaps-scraper` binary — prefer `npx gmaps-scraper` for the Node Google Maps scraper CLI.

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
| npm | https://www.npmjs.com/package/@gmapsleadfinder/google-maps-scraper |
| Source / examples | https://github.com/gmaps-lead-finder/google-maps-scraper |

Prefer Remote MCP for Claude / Cursor / Codex? See the [Agent docs](https://gmapsleadfinder.com/docs/agent).

## License

MIT
