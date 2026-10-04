# AGENTS.md — GMaps Lead Finder client

Instructions for AI coding agents (Cursor, Claude Code, Codex, etc.) working in this repo or integrating the hosted API.

## What this project is

Official **client kit** for [GMaps Lead Finder](https://gmapsleadfinder.com). It talks to the **hosted** Agent HTTP API / Remote MCP. It is **not** a local Google Maps crawler.

- Base URL: `https://gmapsleadfinder.com`
- Auth: `Authorization: Bearer gmf_<key>`
- Env: `GMF_API_KEY` (optional `GMF_BASE_URL`)

## Preferred install (for end users / agents)

```bash
pip install google-maps-scraper-sdk
# import: from gmaps_scraper import Client

npm install @gmapsleadfinder/google-maps-scraper
# import: import { Client } from "@gmapsleadfinder/google-maps-scraper"

go get github.com/gmaps-lead-finder/google-maps-scraper/go@v0.1.4
# first: go mod init … ; import gmaps "github.com/…/go"

cargo add google-maps-scraper-sdk
# use google_maps_scraper_sdk::{Client, ClientOptions};

composer require gmapsleadfinder/google-maps-scraper
# use GmapsLeadFinder\GoogleMapsScraper\Client;
# repo: https://github.com/gmaps-lead-finder/google-maps-scraper-php

gem install google-maps-scraper-sdk
# require "gmaps_scraper" → GmapsScraper::Client

dotnet add package GmapsLeadFinder.GoogleMapsScraper
# using GmapsLeadFinder.GoogleMapsScraper;
# CLI: dotnet tool install -g GmapsLeadFinder.GoogleMapsScraper.Cli
```

## Prefer MCP vs HTTP

| Situation | Use |
|-----------|-----|
| User is in Claude / Cursor / Codex with MCP support | **Remote MCP** at `https://gmapsleadfinder.com/mcp` |
| Scripts, CI, custom apps | **HTTP** via Python/TS/Go/Rust/PHP/Ruby/.NET SDK or raw `fetch`/`curl` |

MCP tools map 1:1 to HTTP: `gmaps_me`, `gmaps_create_job`, `gmaps_get_job`, `gmaps_get_results`, `gmaps_create_reviews_job`, `gmaps_get_reviews_job`, `gmaps_get_reviews_results`, `gmaps_create_photos_job`, `gmaps_get_photos_job`, `gmaps_get_photos_results`.

## API key rules

1. Tell the user to get a key at https://gmapsleadfinder.com/account#api-key (Growth+ plan).
2. Store only in env / secret store — **never** hard-code, commit, or put in public frontends.
3. Keys look like `gmf_` + hex.

## HTTP workflow

1. `GET /api/v1/me` — confirm `creditsRemaining > 0` (includes permanent packs) and plan allows API (Growth+; pack-only accounts are Starter-tier features only).
2. `POST /api/v1/jobs` with `{ "keyword": "<one query>" }` → `jobId`.
3. Poll `GET /api/v1/jobs/{id}` until `completed`, `partial`, or `failed`.
4. `GET /api/v1/jobs/{id}/results?limit=100` and follow `nextCursor` until `null`.

Or use SDK helpers: `scrape` / `Scrape` / `ScrapeAsync` (Python / TypeScript / Go / Rust / PHP / Ruby / .NET) which polls and paginates.

## Reviews HTTP workflow

1. `GET /api/v1/me` — confirm credits.
2. `POST /api/v1/review-jobs` with `{ "place": "<url, short link, or business_id>" }` → `jobId`.
3. Poll `GET /api/v1/review-jobs/{id}` until `completed`, `partial`, or `failed`.
4. `GET /api/v1/review-jobs/{id}/results?limit=100` and follow `nextCursor` until `null`.

Or use SDK helpers: `scrapeReviews` / `scrape_reviews` / `ScrapeReviews` / `ScrapeReviewsAsync`.

## Photos HTTP workflow

1. `GET /api/v1/me` — confirm credits.
2. `POST /api/v1/photo-jobs` with `{ "place": "<url, short link, business_id, or Place ID>" }` → `jobId`.
3. Poll `GET /api/v1/photo-jobs/{id}` until `completed`, `partial`, or `failed`.
4. `GET /api/v1/photo-jobs/{id}/results?limit=100` and follow `nextCursor` until `null`.

Or use SDK helpers: `scrapePhotos` / `scrape_photos` / `ScrapePhotos` / `ScrapePhotosAsync`.

## Hard constraints

- Exactly **one** keyword per leads job (multi-keyword → `400`); exactly **one** place per reviews or photos job.
- Only **one** in-flight job per user (`409` if busy). For multiple keywords or places, run **sequentially** and wait for completion.
- `402` = no credits; `403` = plan cannot use Agent API; `401` = bad/missing key.
- Empty email/social fields mean nothing public was found — do not invent contacts.

## Result shape

Rows are objects keyed by export column headers, e.g. `Name`, `Phone`, `Website`, `Emails` for leads (exact set follows the user’s Leads export preferences). Review rows follow the user’s Reviews export preferences.

## Canonical links

- HTTP docs: https://gmapsleadfinder.com/docs/api
- Agent & MCP: https://gmapsleadfinder.com/docs/agent
- OpenAPI: https://gmapsleadfinder.com/openapi-agent.yaml (repo copy: `openapi/agent.yaml`)
- Pricing: https://gmapsleadfinder.com/pricing
- Installable Agent Skill (skills.sh): https://github.com/gmaps-lead-finder/google-maps-scraper-skill — `npx skills add gmaps-lead-finder/google-maps-scraper-skill`
- Language SDK READMEs in this repo: `python/`, `typescript/`, `go/`, `rust/`, `ruby/`, `dotnet/`
- PHP SDK (separate repo): https://github.com/gmaps-lead-finder/google-maps-scraper-php — Packagist `gmapsleadfinder/google-maps-scraper`

## When editing this repo

- Keep Python, TypeScript, Go, Rust, Ruby, .NET, and PHP public APIs aligned (PHP changes land in the separate PHP repo).
- Do not add Playwright/Selenium scrapers or auth bypasses.
- Go releases use Git tags `go/vX.Y.Z`.
- Rust releases use `cargo publish` from `rust/`.
- Ruby releases use `gem build` / `gem push` from `ruby/`.
- .NET releases use `dotnet pack` / `dotnet nuget push` from `dotnet/`.
- PHP releases: tag `vX.Y.Z` in the PHP repo and Packagist auto-update (no subtree).
