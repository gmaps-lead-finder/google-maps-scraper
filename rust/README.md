# Google Maps Scraper

<p align="center">
  <img src="https://raw.githubusercontent.com/gmaps-lead-finder/google-maps-scraper/main/google-maps-scraper.png" alt="Google Maps Lead Scraper" width="100%" />
</p>

**Google Maps Extractor · Google Maps Lead Scraper · Google Maps Lead Extractor**

Rust SDK to scrape Google Maps places (leads), **reviews**, and **photos** through a hosted Agent HTTP API. Powered by [GMaps Lead Finder](https://gmapsleadfinder.com). This is **not** a local browser crawler — jobs run in the cloud scrape-and-enrich pipeline.

- **crates.io:** [`google-maps-scraper-sdk`](https://crates.io/crates/google-maps-scraper-sdk)
- **docs.rs:** https://docs.rs/google-maps-scraper-sdk
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
cargo add google-maps-scraper-sdk
# or: cargo install google-maps-scraper-sdk --bin gmaps-scraper
```

Requires **Rust 1.70+**.

## Quickstart

```rust
use google_maps_scraper_sdk::{Client, ClientOptions};

fn main() -> Result<(), Box<dyn std::error::Error>> {
    let client = Client::new(ClientOptions::default())?; // reads GMF_API_KEY
    let me = client.me()?;
    println!("{} {}", me.plan, me.credits_remaining);

    let rows = client.scrape("dentists in Austin TX", Default::default())?;
    println!("{} {:?}", rows.len(), rows.first());

    // Single-place reviews / photos (run sequentially — one in-flight job per user)
    let reviews = client.scrape_reviews("https://maps.google.com/?cid=…", Default::default())?;
    let photos = client.scrape_photos("ChIJ…", Default::default())?; // Place ID, URL, or business_id
    println!("{} {}", reviews.len(), photos.len());
    Ok(())
}
```

CLI:

```bash
cargo install google-maps-scraper-sdk --bin gmaps-scraper
gmaps-scraper me
gmaps-scraper scrape "dentists in Austin TX" --out leads.json
```

From this repo:

```bash
cd rust
cargo run --example quickstart
cargo run --bin gmaps-scraper -- me
```

## Client API

| Method | Description |
|--------|-------------|
| `Client::new(opts)` | From env / options |
| `me()` | Plan & credits |
| `create_job(keyword)` | Queue one-keyword leads job |
| `get_job(job_id)` | Poll leads status |
| `get_results(job_id, opts)` | Paginated leads rows |
| `scrape(keyword, opts)` | Create → poll → all leads rows |
| `create_reviews_job(place)` | Queue single-place reviews job |
| `get_reviews_job(job_id)` | Poll reviews status |
| `get_reviews_results(job_id, opts)` | Paginated review rows |
| `scrape_reviews(place, opts)` | Create → poll → all review rows |
| `create_photos_job(place)` | Queue single-place photos job |
| `get_photos_job(job_id)` | Poll photos status |
| `get_photos_results(job_id, opts)` | Paginated photo rows |
| `scrape_photos(place, opts)` | Create → poll → all photo rows |

## Errors

`Error` variants: `Authentication` (401), `PlanNotAllowed` (403), `InsufficientCredits` (402), `JobConflict` (409), `NotFound` (404), `BadRequest` (400), `Timeout`, `Api`, `Http`, `Json`.

## Limits

- Exactly one keyword per leads job; exactly one place per reviews or photos job
- One running job per user (`409` if busy); run keywords/places sequentially
- 1 credit = 1 place row; Growth+ required

## Publish to crates.io

```bash
cargo login
cd rust
cargo publish --dry-run
cargo publish
```

## Links

| Resource | URL |
|----------|-----|
| Website | https://gmapsleadfinder.com |
| HTTP API | https://gmapsleadfinder.com/docs/api |
| crates.io | https://crates.io/crates/google-maps-scraper-sdk |
| Source | https://github.com/gmaps-lead-finder/google-maps-scraper |

## License

MIT
