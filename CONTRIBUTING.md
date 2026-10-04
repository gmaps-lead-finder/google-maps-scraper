# Contributing

Thanks for helping improve the GMaps Lead Finder client kit.

## Scope

This repository is an **official HTTP/MCP client** for [gmapsleadfinder.com](https://gmapsleadfinder.com).

Please keep PRs focused on:

- Python / TypeScript SDK and CLI
- Examples and documentation accuracy
- OpenAPI alignment with the hosted API

Out of scope:

- Self-hosted browser scrapers or Playwright/Selenium crawlers
- Bypassing authentication, credits, or plan gates
- ExtensionsFox Chrome-extension backends

## Development notes

- Keep Python and TypeScript public APIs semantically aligned (`me`, `createJob`, `getJob`, `getResults`, `scrape`).
- Document env var: `GMF_API_KEY`.
- Do not commit `.env` or real keys.
- Prefer linking to live docs when behavior is owned by the SaaS:

  - [HTTP API](https://gmapsleadfinder.com/docs/api)
  - [Agent & MCP](https://gmapsleadfinder.com/docs/agent)

## Pull requests

1. Describe the change and why.
2. Keep commits focused; avoid unrelated refactors.
