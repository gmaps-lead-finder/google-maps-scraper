# Google Maps Scraper

<p align="center">
  <img src="https://raw.githubusercontent.com/GMaps-Lead-Finder/google-maps-scraper/main/google-maps-scraper.png" alt="Google Maps Lead Scraper" width="100%" />
</p>

**Google Maps Extractor · Google Maps Lead Scraper · Google Maps Lead Extractor**

Ruby SDK to scrape Google Maps places (leads), **reviews**, and **photos** through a hosted Agent HTTP API. Powered by [GMaps Lead Finder](https://gmapsleadfinder.com). This is **not** a local browser crawler — jobs run in the cloud scrape-and-enrich pipeline.

- **RubyGems:** [`google-maps-scraper-sdk`](https://rubygems.org/gems/google-maps-scraper-sdk)
- **Require:** `gmaps_scraper` → `GmapsScraper::Client`
- **CLI:** `gmaps-scraper`

## Get an API key

1. Sign in at [gmapsleadfinder.com](https://gmapsleadfinder.com).
2. Use a plan with Agent API (**Growth** or higher) — see [Pricing](https://gmapsleadfinder.com/pricing).
3. Open [Account → API key](https://gmapsleadfinder.com/account#api-key) and copy your `gmf_…` key.

```bash
export GMF_API_KEY=gmf_your_key_here
```

## Install

```bash
gem install google-maps-scraper-sdk
# or: bundle add google-maps-scraper-sdk
```

Requires **Ruby 3.1+** (stdlib `net/http` + `json` only).

## Quickstart

```ruby
require "gmaps_scraper"

client = GmapsScraper::Client.new # reads GMF_API_KEY
me = client.me
puts "#{me['plan']} #{me['creditsRemaining']}"

rows = client.scrape("dentists in Austin TX")
puts rows.length

# Single-place reviews / photos (run sequentially — one in-flight job per user)
reviews = client.scrape_reviews("https://maps.google.com/?cid=…")
photos = client.scrape_photos("ChIJ…") # Place ID, URL, or business_id
puts reviews.length, photos.length
```

CLI:

```bash
gmaps-scraper me
gmaps-scraper scrape "dentists in Austin TX" --out leads.json
```

From this directory:

```bash
bundle install
bundle exec ruby examples/quickstart.rb
bundle exec gmaps-scraper me
```

## Client API

| Method | Description |
|--------|-------------|
| `Client.new(api_key:, base_url:, timeout_s:)` | From env / kwargs |
| `me` | Plan & credits |
| `create_job(keyword)` | Queue one-keyword leads job |
| `get_job(job_id)` | Poll leads status |
| `get_results(job_id, limit:, cursor:)` | Paginated leads rows |
| `scrape(keyword, poll_interval_ms:, timeout_ms:, result_limit:)` | Create → poll → all leads rows |
| `create_reviews_job(place)` | Queue single-place reviews job |
| `get_reviews_job(job_id)` | Poll reviews status |
| `get_reviews_results(job_id, limit:, cursor:)` | Paginated review rows |
| `scrape_reviews(place, …)` | Create → poll → all review rows |
| `create_photos_job(place)` | Queue single-place photos job |
| `get_photos_job(job_id)` | Poll photos status |
| `get_photos_results(job_id, limit:, cursor:)` | Paginated photo rows |
| `scrape_photos(place, …)` | Create → poll → all photo rows |

`scrape` / `scrape_reviews` / `scrape_photos` defaults: `poll_interval_ms` 2000, `timeout_ms` 600000, `result_limit` 100.

## Errors

| HTTP | Class |
|------|-------|
| 400 | `GmapsScraper::BadRequestError` |
| 401 | `GmapsScraper::AuthenticationError` |
| 402 | `GmapsScraper::InsufficientCreditsError` |
| 403 | `GmapsScraper::PlanNotAllowedError` |
| 404 | `GmapsScraper::NotFoundError` |
| 409 | `GmapsScraper::JobConflictError` |
| — | `GmapsScraper::TimeoutError`, `GmapsScraper::ApiError` |

## Limits

- Exactly one keyword per leads job; exactly one place per reviews or photos job
- One running job per user (`409` if busy); run keywords/places sequentially
- 1 credit = 1 place row; Growth+ required

## Publish to RubyGems

```bash
cd ruby
gem build google-maps-scraper-sdk.gemspec
gem push google-maps-scraper-sdk-0.1.2.gem
```

## Links

| Resource | URL |
|----------|-----|
| Website | https://gmapsleadfinder.com |
| HTTP API | https://gmapsleadfinder.com/docs/api |
| RubyGems | https://rubygems.org/gems/google-maps-scraper-sdk |
| Monorepo | https://github.com/GMaps-Lead-Finder/google-maps-scraper |

## License

MIT
