# Google Maps Scraper (GMaps Lead Finder Client)

<p align="center">
  <img src="./google-maps-scraper.png" alt="Google Maps Lead Scraper" width="100%" />
</p>

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![npm](https://img.shields.io/npm/v/@gmapsleadfinder/google-maps-scraper.svg)](https://www.npmjs.com/package/@gmapsleadfinder/google-maps-scraper)
[![PyPI](https://img.shields.io/pypi/v/google-maps-scraper-sdk.svg)](https://pypi.org/project/google-maps-scraper-sdk/)
[![Crates.io](https://img.shields.io/crates/v/google-maps-scraper-sdk.svg)](https://crates.io/crates/google-maps-scraper-sdk)
[![Go Reference](https://pkg.go.dev/badge/github.com/GMaps-Lead-Finder/google-maps-scraper/go.svg)](https://pkg.go.dev/github.com/GMaps-Lead-Finder/google-maps-scraper/go)
[![Packagist](https://img.shields.io/packagist/v/gmapsleadfinder/google-maps-scraper.svg)](https://packagist.org/packages/gmapsleadfinder/google-maps-scraper)
[![Gem Version](https://img.shields.io/gem/v/google-maps-scraper-sdk.svg)](https://rubygems.org/gems/google-maps-scraper-sdk)
[![NuGet](https://img.shields.io/nuget/v/GmapsLeadFinder.GoogleMapsScraper.svg)](https://www.nuget.org/packages/GmapsLeadFinder.GoogleMapsScraper)
[![Python 3.10+](https://img.shields.io/badge/python-3.10+-3776AB.svg)](python/)
[![Node.js 18+](https://img.shields.io/badge/node-18+-339933.svg)](typescript/)
[![Go 1.22+](https://img.shields.io/badge/go-1.22+-00ADD8.svg)](go/)
[![Rust](https://img.shields.io/badge/rust-1.70+-dea584.svg)](rust/)
[![PHP 8.1+](https://img.shields.io/badge/php-8.1+-777BB4.svg)](https://github.com/GMaps-Lead-Finder/google-maps-scraper-php)
[![Ruby 3.1+](https://img.shields.io/badge/ruby-3.1+-CC342D.svg)](ruby/)
[![.NET 8+](https://img.shields.io/badge/.NET-8+-512BD4.svg)](dotnet/)

Official open-source **Python + TypeScript + Go + Rust + PHP + Ruby + .NET** client kit for [GMaps Lead Finder](https://gmapsleadfinder.com) — scrape Google Maps **leads**, **reviews**, and **photos** through the hosted Agent HTTP API / Remote MCP.

This repo does **not** run a local browser crawler. It calls the same cloud scrape-and-enrich pipeline as the Online Lead Extractor. Reviews/photos helpers take a single place (Maps URL, share short link, `business_id`, or Place ID for photos); see the [HTTP API docs](https://gmapsleadfinder.com/docs/api).

## Get an API key

1. Sign in at [gmapsleadfinder.com](https://gmapsleadfinder.com) (Google OAuth).
2. Use a plan that includes Agent API (**Growth** or higher) — see [Pricing](https://gmapsleadfinder.com/pricing).
3. Open [Account → API key](https://gmapsleadfinder.com/account#api-key), copy your `gmf_…` key.

```bash
export GMF_API_KEY=gmf_your_key_here
```

## 60-second Quickstart

### Python

```bash
pip install google-maps-scraper-sdk
gmaps-scraper me
gmaps-scraper scrape "dentists in Austin TX" --out leads.json
# or: python -m gmaps_scraper me
```

```python
from gmaps_scraper import Client

client = Client()  # reads GMF_API_KEY
rows = client.scrape("dentists in Austin TX")
print(len(rows), rows[0] if rows else None)
# reviews = client.scrape_reviews("<place url, short link, or business_id>")
# photos = client.scrape_photos("<place url, short link, business_id, or Place ID>")
```

### TypeScript / Node

```bash
npm install @gmapsleadfinder/google-maps-scraper
npx gmaps-scraper me
npx gmaps-scraper scrape "dentists in Austin TX" --out leads.json
```

```ts
import { Client } from "@gmapsleadfinder/google-maps-scraper";

const client = new Client(); // reads GMF_API_KEY
const rows = await client.scrape("dentists in Austin TX");
console.log(rows.length, rows[0]);
// const reviews = await client.scrapeReviews("<place url, short link, or business_id>");
// const photos = await client.scrapePhotos("<place url, short link, business_id, or Place ID>");
```

### Go

```bash
go get github.com/GMaps-Lead-Finder/google-maps-scraper/go@v0.1.3
```

```go
import gmaps "github.com/GMaps-Lead-Finder/google-maps-scraper/go"

client, _ := gmaps.NewClient(nil) // reads GMF_API_KEY
rows, _ := client.Scrape("dentists in Austin TX", nil)
fmt.Println(len(rows), rows[0])
// reviews, _ := client.ScrapeReviews("<place>", nil)
// photos, _ := client.ScrapePhotos("<place>", nil)
```

```bash
cd go && go run ./cli scrape "dentists in Austin TX" --out leads.json
```

### Rust

```bash
cargo add google-maps-scraper-sdk
# CLI: cargo install google-maps-scraper-sdk --bin gmaps-scraper
```

```rust
use google_maps_scraper_sdk::{Client, ClientOptions};

let client = Client::new(ClientOptions::default())?; // reads GMF_API_KEY
let rows = client.scrape("dentists in Austin TX", Default::default())?;
println!("{} {:?}", rows.len(), rows.first());
// let reviews = client.scrape_reviews("<place>", Default::default())?;
// let photos = client.scrape_photos("<place>", Default::default())?;
```

### PHP

```bash
composer require gmapsleadfinder/google-maps-scraper
vendor/bin/gmaps-scraper me
vendor/bin/gmaps-scraper scrape "dentists in Austin TX" --out leads.json
```

```php
use GmapsLeadFinder\GoogleMapsScraper\Client;

$client = new Client(); // reads GMF_API_KEY
$rows = $client->scrape("dentists in Austin TX");
echo count($rows), "\n";
// $reviews = $client->scrapeReviews("<place>");
// $photos = $client->scrapePhotos("<place>");
```

Source: [google-maps-scraper-php](https://github.com/GMaps-Lead-Finder/google-maps-scraper-php) (Packagist: `gmapsleadfinder/google-maps-scraper`).

### Ruby

```bash
gem install google-maps-scraper-sdk
# or: bundle add google-maps-scraper-sdk
gmaps-scraper me
gmaps-scraper scrape "dentists in Austin TX" --out leads.json
```

```ruby
require "gmaps_scraper"

client = GmapsScraper::Client.new # reads GMF_API_KEY
rows = client.scrape("dentists in Austin TX")
puts rows.length
# reviews = client.scrape_reviews("<place>")
# photos = client.scrape_photos("<place>")
```

### .NET

```bash
dotnet add package GmapsLeadFinder.GoogleMapsScraper
dotnet tool install -g GmapsLeadFinder.GoogleMapsScraper.Cli
gmaps-scraper me
gmaps-scraper scrape "dentists in Austin TX" --out leads.json
```

```csharp
using GmapsLeadFinder.GoogleMapsScraper;

using var client = new Client(); // reads GMF_API_KEY
var rows = await client.ScrapeAsync("dentists in Austin TX");
Console.WriteLine(rows.Count);
// var reviews = await client.ScrapeReviewsAsync("<place>");
// var photos = await client.ScrapePhotosAsync("<place>");
```

> Python, npm, Rust, PHP, Ruby, and .NET CLIs may all be named `gmaps-scraper`. Prefer `npx` / `python -m gmaps_scraper` / `go run ./cli` / `cargo run --bin gmaps-scraper` / `vendor/bin/gmaps-scraper` / `bundle exec gmaps-scraper` / `dotnet tool` if you install more than one.

## Product & docs links

| Resource | URL |
|----------|-----|
| Website | https://gmapsleadfinder.com |
| Pricing | https://gmapsleadfinder.com/pricing |
| HTTP API docs | https://gmapsleadfinder.com/docs/api |
| Agent & MCP docs | https://gmapsleadfinder.com/docs/agent |
| Agents hub | https://gmapsleadfinder.com/agents |
| Agent Skill (skills.sh) | https://github.com/GMaps-Lead-Finder/google-maps-scraper-skill — `npx skills add GMaps-Lead-Finder/google-maps-scraper-skill` |
| OpenAPI | https://gmapsleadfinder.com/openapi-agent.yaml |
| Account / API key | https://gmapsleadfinder.com/account#api-key |
| npm | https://www.npmjs.com/package/@gmapsleadfinder/google-maps-scraper |
| PyPI | https://pypi.org/project/google-maps-scraper-sdk/ |
| Go (pkg.go.dev) | https://pkg.go.dev/github.com/GMaps-Lead-Finder/google-maps-scraper/go |
| crates.io | https://crates.io/crates/google-maps-scraper-sdk |
| Packagist (PHP) | https://packagist.org/packages/gmapsleadfinder/google-maps-scraper |
| PHP repo | https://github.com/GMaps-Lead-Finder/google-maps-scraper-php |
| RubyGems | https://rubygems.org/gems/google-maps-scraper-sdk |
| NuGet | https://www.nuget.org/packages/GmapsLeadFinder.GoogleMapsScraper |
| Support | support@gmapsleadfinder.com |

Repo docs: [Python](python/README.md) · [TypeScript](typescript/README.md) · [Go](go/README.md) · [Rust](rust/README.md) · [Ruby](ruby/README.md) · [.NET](dotnet/README.md) · [AGENTS.md](AGENTS.md) · [llms.txt](llms.txt)

## Remote MCP (Claude / Cursor / Codex)

Prefer MCP when using an AI coding agent. Endpoint: `https://gmapsleadfinder.com/mcp`.

**Claude Code**

```bash
claude mcp add --transport http gmaps-finder https://gmapsleadfinder.com/mcp \
  --header "Authorization: Bearer $GMF_API_KEY"
```

**Cursor** (`~/.cursor/mcp.json` fragment)

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

MCP tools: `gmaps_me`, `gmaps_create_job`, `gmaps_get_job`, `gmaps_get_results`, `gmaps_create_reviews_job`, `gmaps_get_reviews_job`, `gmaps_get_reviews_results`, `gmaps_create_photos_job`, `gmaps_get_photos_job`, `gmaps_get_photos_results`. Details: [Agent & MCP docs](https://gmapsleadfinder.com/docs/agent).

## Limits (read before batching)

- **Exactly one keyword per leads job**; **exactly one place per reviews or photos job**
- **One running job per user** at a time (web + API + MCP share the lock) → HTTP `409` if busy
- **1 credit = 1 place row**; enrich is included
- Agent HTTP/MCP requires **Growth+**
- Results page size: `limit` 1–500 (default 100); follow `nextCursor`

## Repository layout

```text
python/          # PyPI: google-maps-scraper-sdk (import gmaps_scraper)
typescript/      # npm: @gmapsleadfinder/google-maps-scraper
go/              # Go module (pkg.go.dev); tag go/vX.Y.Z
rust/            # crates.io: google-maps-scraper-sdk
ruby/            # RubyGems: google-maps-scraper-sdk (require gmaps_scraper)
dotnet/          # NuGet: GmapsLeadFinder.GoogleMapsScraper (+ Cli tool)
openapi/         # OpenAPI snapshot
AGENTS.md        # Instructions for AI agents
llms.txt         # Machine-readable summary

# PHP lives in a separate repo (Packagist root composer.json):
# https://github.com/GMaps-Lead-Finder/google-maps-scraper-php
```

## License

MIT — see [LICENSE](LICENSE).
