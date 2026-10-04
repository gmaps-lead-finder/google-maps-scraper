#!/usr/bin/env node
import { mkdir, writeFile } from "node:fs/promises";
import path from "node:path";
import { Client } from "./client.js";
import { ApiError } from "./errors.js";
import type { PlaceRow } from "./types.js";

const VERSION = "0.1.3";

function printHelp(): void {
  console.log(`gmaps-scraper ${VERSION}

Usage:
  gmaps-scraper me
  gmaps-scraper scrape "<keyword>" [--out leads.json|leads.csv]
  gmaps-scraper scrape "<keyword>" [--poll-interval-ms 2000] [--timeout-ms 600000]

Env:
  GMF_API_KEY     required (https://gmapsleadfinder.com/account#api-key)
  GMF_BASE_URL    optional (default https://gmapsleadfinder.com)
`);
}

function rowsToCsv(rows: PlaceRow[]): string {
  const fieldnames: string[] = [];
  const seen = new Set<string>();
  for (const row of rows) {
    for (const key of Object.keys(row)) {
      if (!seen.has(key)) {
        seen.add(key);
        fieldnames.push(key);
      }
    }
  }
  const escape = (value: string) => {
    if (/[",\n\r]/.test(value)) {
      return `"${value.replace(/"/g, '""')}"`;
    }
    return value;
  };
  const lines = [fieldnames.join(",")];
  for (const row of rows) {
    lines.push(fieldnames.map((k) => escape(String(row[k] ?? ""))).join(","));
  }
  return lines.join("\n") + "\n";
}

async function writeOutput(rows: PlaceRow[], out?: string): Promise<void> {
  if (!out) {
    process.stdout.write(JSON.stringify(rows, null, 2) + "\n");
    return;
  }
  const abs = path.resolve(out);
  await mkdir(path.dirname(abs), { recursive: true });
  if (abs.toLowerCase().endsWith(".csv")) {
    await writeFile(abs, rowsToCsv(rows), "utf8");
  } else {
    await writeFile(abs, JSON.stringify(rows, null, 2) + "\n", "utf8");
  }
  console.error(`Wrote ${rows.length} rows to ${abs}`);
}

async function main(argv: string[]): Promise<number> {
  const args = argv.slice(2);
  if (args.length === 0 || args.includes("-h") || args.includes("--help")) {
    printHelp();
    return args.length === 0 ? 1 : 0;
  }
  if (args.includes("--version")) {
    console.log(VERSION);
    return 0;
  }

  const command = args[0];
  const client = new Client();

  if (command === "me") {
    const me = await client.me();
    process.stdout.write(JSON.stringify(me, null, 2) + "\n");
    return 0;
  }

  if (command === "scrape") {
    const keyword = args[1];
    if (!keyword || keyword.startsWith("--")) {
      console.error('error: scrape requires a keyword, e.g. scrape "dentists in Austin TX"');
      return 1;
    }
    let out: string | undefined;
    let pollIntervalMs = 2000;
    let timeoutMs = 600_000;
    for (let i = 2; i < args.length; i++) {
      const a = args[i];
      if (a === "--out") {
        out = args[++i];
      } else if (a === "--poll-interval-ms") {
        pollIntervalMs = Number(args[++i]);
      } else if (a === "--timeout-ms") {
        timeoutMs = Number(args[++i]);
      } else {
        console.error(`error: unknown argument ${a}`);
        return 1;
      }
    }
    const rows = await client.scrape(keyword, { pollIntervalMs, timeoutMs });
    await writeOutput(rows, out);
    return 0;
  }

  console.error(`error: unknown command ${command}`);
  printHelp();
  return 1;
}

main(process.argv).catch((err) => {
  if (err instanceof ApiError) {
    console.error(`error: ${err.message}`);
    if (err.statusCode) console.error(`status: ${err.statusCode}`);
  } else {
    console.error(err);
  }
  process.exit(1);
});
