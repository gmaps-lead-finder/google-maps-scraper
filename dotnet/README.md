# Google Maps Scraper

<p align="center">
  <img src="https://raw.githubusercontent.com/GMaps-Lead-Finder/google-maps-scraper/main/google-maps-scraper.png" alt="Google Maps Lead Scraper" width="100%" />
</p>

**Google Maps Extractor · Google Maps Lead Scraper · Google Maps Lead Extractor**

.NET SDK to scrape Google Maps places (leads), **reviews**, and **photos** through a hosted Agent HTTP API. Powered by [GMaps Lead Finder](https://gmapsleadfinder.com). This is **not** a local browser crawler — jobs run in the cloud scrape-and-enrich pipeline.

- **NuGet:** [`GmapsLeadFinder.GoogleMapsScraper`](https://www.nuget.org/packages/GmapsLeadFinder.GoogleMapsScraper)
- **CLI tool:** [`GmapsLeadFinder.GoogleMapsScraper.Cli`](https://www.nuget.org/packages/GmapsLeadFinder.GoogleMapsScraper.Cli) (`gmaps-scraper`)
- **Namespace:** `GmapsLeadFinder.GoogleMapsScraper`

## Get an API key

1. Sign in at [gmapsleadfinder.com](https://gmapsleadfinder.com).
2. Use a plan with Agent API (**Growth** or higher) — see [Pricing](https://gmapsleadfinder.com/pricing).
3. Open [Account → API key](https://gmapsleadfinder.com/account#api-key) and copy your `gmf_…` key.

```bash
export GMF_API_KEY=gmf_your_key_here
```

## Install

```bash
dotnet add package GmapsLeadFinder.GoogleMapsScraper
dotnet tool install -g GmapsLeadFinder.GoogleMapsScraper.Cli
```

Requires **.NET 8.0+**. No third-party HTTP dependencies (`HttpClient` + `System.Text.Json`).

## Quickstart

```csharp
using GmapsLeadFinder.GoogleMapsScraper;

using var client = new Client(); // reads GMF_API_KEY
var me = await client.MeAsync();
Console.WriteLine($"{me.GetProperty("plan")} {me.GetProperty("creditsRemaining")}");

var rows = await client.ScrapeAsync("dentists in Austin TX");
Console.WriteLine(rows.Count);

// Single-place reviews / photos (run sequentially — one in-flight job per user)
var reviews = await client.ScrapeReviewsAsync("https://maps.google.com/?cid=…");
var photos = await client.ScrapePhotosAsync("ChIJ…"); // Place ID, URL, or business_id
Console.WriteLine($"{reviews.Count} {photos.Count}");
```

CLI:

```bash
gmaps-scraper me
gmaps-scraper scrape "dentists in Austin TX" --out leads.json
```

From this directory:

```bash
dotnet run --project examples/Quickstart
dotnet run --project src/GmapsLeadFinder.GoogleMapsScraper.Cli -- me
```

## Client API

| Method | Description |
|--------|-------------|
| `new Client(options?)` | From env / `ApiKey` / `BaseUrl` / `Timeout` |
| `MeAsync()` | Plan & credits (`JsonElement`) |
| `CreateJobAsync(keyword)` | Queue one-keyword leads job |
| `GetJobAsync(jobId)` | Poll leads status |
| `GetResultsAsync(jobId, options?)` | Paginated leads rows (`Limit`, `Cursor`) |
| `ScrapeAsync(keyword, options?)` | Create → poll → all leads rows |
| `CreateReviewsJobAsync(place)` | Queue single-place reviews job |
| `GetReviewsJobAsync(jobId)` | Poll reviews status |
| `GetReviewsResultsAsync(jobId, options?)` | Paginated review rows |
| `ScrapeReviewsAsync(place, options?)` | Create → poll → all review rows |
| `CreatePhotosJobAsync(place)` | Queue single-place photos job |
| `GetPhotosJobAsync(jobId)` | Poll photos status |
| `GetPhotosResultsAsync(jobId, options?)` | Paginated photo rows |
| `ScrapePhotosAsync(place, options?)` | Create → poll → all photo rows |

`ScrapeOptions`: `PollIntervalMs` (2000), `TimeoutMs` (600000), `ResultLimit` (100).

## Errors

| HTTP | Class |
|------|-------|
| 400 | `BadRequestException` |
| 401 | `AuthenticationException` |
| 402 | `InsufficientCreditsException` |
| 403 | `PlanNotAllowedException` |
| 404 | `NotFoundException` |
| 409 | `JobConflictException` |
| — | `TimeoutException`, `ApiException` |

All under `GmapsLeadFinder.GoogleMapsScraper.Exceptions`.

## Limits

- Exactly one keyword per leads job; exactly one place per reviews or photos job
- One running job per user (`409` if busy); run keywords/places sequentially
- 1 credit = 1 place row; Growth+ required

## Publish to NuGet

```bash
cd dotnet
dotnet pack src/GmapsLeadFinder.GoogleMapsScraper -c Release
dotnet pack src/GmapsLeadFinder.GoogleMapsScraper.Cli -c Release
dotnet nuget push src/GmapsLeadFinder.GoogleMapsScraper/bin/Release/*.nupkg \
  --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push src/GmapsLeadFinder.GoogleMapsScraper.Cli/bin/Release/*.nupkg \
  --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

## Links

| Resource | URL |
|----------|-----|
| Website | https://gmapsleadfinder.com |
| HTTP API | https://gmapsleadfinder.com/docs/api |
| NuGet | https://www.nuget.org/packages/GmapsLeadFinder.GoogleMapsScraper |
| Monorepo | https://github.com/GMaps-Lead-Finder/google-maps-scraper |

## License

MIT
