# TypeScript SDK

npm package: [`@gmapsleadfinder/google-maps-scraper`](https://www.npmjs.com/package/@gmapsleadfinder/google-maps-scraper)  
Source: [`typescript/`](../typescript/)

## Install

```bash
npm install @gmapsleadfinder/google-maps-scraper
```

Requires Node.js 18+ (uses global `fetch`).

### Develop from this repo

```bash
cd typescript
npm install
npm run build
```

## Configuration

```bash
export GMF_API_KEY=gmf_your_key_here
# optional:
# export GMF_BASE_URL=https://gmapsleadfinder.com
```

Or pass `apiKey` / `baseUrl` into `new Client({ ... })`.

## Usage

```ts
import { Client } from "@gmapsleadfinder/google-maps-scraper";

const client = new Client();

const me = await client.me();
console.log(me.creditsRemaining);

const rows = await client.scrape("dentists in Austin TX");
for (const row of rows.slice(0, 5)) {
  console.log(row.Name, row.Phone, row.Website);
}

// Single-place reviews / photos (run sequentially)
const reviews = await client.scrapeReviews("https://maps.app.goo.gl/…"); // URL, short link, or business_id
const photos = await client.scrapePhotos("ChIJ…"); // Place ID, URL, short link, or business_id
```

### Low-level methods

```ts
const created = await client.createJob("dentists in Austin TX");
const job = await client.getJob(created.jobId);
const page = await client.getResults(created.jobId, { limit: 100, cursor: "0" });

// Reviews / photos
const r = await client.createReviewsJob("https://maps.google.com/?cid=…");
const p = await client.createPhotosJob("ChIJ…");
```

`scrape()` / `scrapeReviews()` / `scrapePhotos()` options: `pollIntervalMs` (default `2000`), `timeoutMs` (default `600_000`), `resultLimit` (default `100`).

## CLI

```bash
npx gmaps-scraper me
npx gmaps-scraper scrape "dentists in Austin TX" --out leads.json
npx gmaps-scraper scrape "dentists in Austin TX" --out leads.csv
```

After a global install (`npm i -g @gmapsleadfinder/google-maps-scraper`), the binary is also `gmaps-scraper`. Prefer `npx` if you also installed the Python CLI globally (same command name).

## Examples

- [`typescript/examples/quickstart.ts`](../typescript/examples/quickstart.ts)
- [`typescript/examples/poll-and-export-csv.ts`](../typescript/examples/poll-and-export-csv.ts)
- [`typescript/examples/sequential-keywords.ts`](../typescript/examples/sequential-keywords.ts)

## Errors

Classes exported from the package: `AuthenticationError` (401), `PlanNotAllowedError` (403), `InsufficientCreditsError` (402), `JobConflictError` (409), `NotFoundError` (404), `ApiError` (other).

## Related

- [HTTP API](http-api.md)
- [Publishing (maintainers)](publishing.md)
- Live docs: https://gmapsleadfinder.com/docs/api
