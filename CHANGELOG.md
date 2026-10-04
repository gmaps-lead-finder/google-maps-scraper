# Changelog

All notable changes to this project will be documented in this file.

## [0.1.3] - 2026-10-04

### Changed

- GitHub org / repository metadata is now [gmaps-lead-finder](https://github.com/gmaps-lead-finder) across npm, PyPI, crates.io, RubyGems, and NuGet package manifests
- Bumped SDK package versions to `0.1.3` (npm, PyPI, crates.io, RubyGems, NuGet)

## [0.1.2] - 2026-09-19

### Added

- Documented reviews / photos Client API across all SDK READMEs, `docs/python.md`, `docs/typescript.md`, root README, and `llms.txt`

### Fixed

- TypeScript `fetchAllRows` TS7022 (`page` needed explicit `ResultsResponse` annotation) blocking `npm publish`

### Changed

- Aligned all SDK package versions to `0.1.2` (npm, PyPI, Go tag `go/v0.1.2`, crates.io, RubyGems, NuGet, PHP `v0.1.2`)

## [0.1.1] - 2026-09-16

### Added

- Go SDK module `github.com/gmaps-lead-finder/google-maps-scraper/go` (tag `go/v0.1.1` for pkg.go.dev)
- Rust crate `google-maps-scraper-sdk` for crates.io (`rust/`)
- PHP SDK Packagist `gmapsleadfinder/google-maps-scraper` in separate repo [google-maps-scraper-php](https://github.com/gmaps-lead-finder/google-maps-scraper-php) (`v0.1.1`)
- Ruby gem `google-maps-scraper-sdk` for RubyGems (`ruby/`, require `gmaps_scraper`)
- .NET NuGet `GmapsLeadFinder.GoogleMapsScraper` + CLI tool `GmapsLeadFinder.GoogleMapsScraper.Cli` (`dotnet/`)

### Changed

- Expanded SEO package READMEs and added README hero image
- Bumped npm `@gmapsleadfinder/google-maps-scraper` and PyPI `google-maps-scraper-sdk` to `0.1.1`
- Root README / publishing / AGENTS / llms link PHP + Packagist + Ruby + .NET

## [0.1.0] - 2026-09-16

### Added

- Python SDK published as PyPI `google-maps-scraper-sdk` (import `gmaps_scraper`) + `gmaps-scraper` CLI
- TypeScript SDK published as npm `@gmapsleadfinder/google-maps-scraper` + CLI
- Examples for quickstart, CSV export, and sequential keywords
- Docs: API key setup, HTTP API, MCP, Python, TypeScript, publishing
- `AGENTS.md` and `llms.txt` for AI clients
- Bundled OpenAPI snapshot at `openapi/agent.yaml`
