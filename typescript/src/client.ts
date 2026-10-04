import {
  ApiError,
  AuthenticationError,
  BadRequestError,
  InsufficientCreditsError,
  JobConflictError,
  NotFoundError,
  PlanNotAllowedError,
  TimeoutError,
} from "./errors.js";
import {
  TERMINAL_STATUSES,
  type ClientOptions,
  type CreateJobResponse,
  type GetResultsOptions,
  type JobResponse,
  type MeResponse,
  type PlaceRow,
  type ResultsResponse,
  type ScrapeOptions,
} from "./types.js";

const DEFAULT_BASE_URL = "https://gmapsleadfinder.com";
const DEFAULT_POLL_INTERVAL_MS = 2000;
const DEFAULT_TIMEOUT_MS = 600_000;
const DEFAULT_RESULT_LIMIT = 100;
const USER_AGENT = "gmaps-scraper-typescript/0.1.3";

function sleep(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function raiseForStatus(status: number, body: unknown): never {
  let message = `API request failed with status ${status}`;
  if (body && typeof body === "object") {
    const record = body as Record<string, unknown>;
    const candidate =
      record.message ?? record.error ?? record.statusMessage;
    if (typeof candidate === "string" && candidate) {
      message = candidate;
    }
  }

  const opts = { statusCode: status, body };
  if (status === 400) throw new BadRequestError(message, opts);
  if (status === 401) throw new AuthenticationError(message, opts);
  if (status === 402) throw new InsufficientCreditsError(message, opts);
  if (status === 403) throw new PlanNotAllowedError(message, opts);
  if (status === 404) throw new NotFoundError(message, opts);
  if (status === 409) throw new JobConflictError(message, opts);
  throw new ApiError(message, opts);
}

export class Client {
  readonly apiKey: string;
  readonly baseUrl: string;
  readonly timeoutMs: number;

  constructor(options: ClientOptions = {}) {
    const key =
      options.apiKey ??
      (typeof process !== "undefined" ? process.env.GMF_API_KEY : undefined);
    if (!key) {
      throw new AuthenticationError(
        "Missing API key. Set GMF_API_KEY or pass apiKey " +
          "(https://gmapsleadfinder.com/account#api-key)",
      );
    }
    this.apiKey = key.trim();
    this.baseUrl = (
      options.baseUrl ??
      (typeof process !== "undefined" ? process.env.GMF_BASE_URL : undefined) ??
      DEFAULT_BASE_URL
    ).replace(/\/$/, "");
    this.timeoutMs = options.timeoutMs ?? 60_000;
  }

  me(): Promise<MeResponse> {
    return this.request<MeResponse>("GET", "/api/v1/me");
  }

  createJob(keyword: string): Promise<CreateJobResponse> {
    const trimmed = keyword?.trim();
    if (!trimmed) {
      throw new BadRequestError("keyword is required");
    }
    return this.request<CreateJobResponse>("POST", "/api/v1/jobs", {
      jsonBody: { keyword: trimmed },
    });
  }

  getJob(jobId: string): Promise<JobResponse> {
    return this.request<JobResponse>("GET", `/api/v1/jobs/${jobId}`);
  }

  getResults(
    jobId: string,
    options: GetResultsOptions = {},
  ): Promise<ResultsResponse> {
    const query: Record<string, string> = {
      limit: String(options.limit ?? DEFAULT_RESULT_LIMIT),
    };
    if (options.cursor !== undefined && options.cursor !== null) {
      query.cursor = String(options.cursor);
    }
    return this.request<ResultsResponse>(
      "GET",
      `/api/v1/jobs/${jobId}/results`,
      { query },
    );
  }

  createReviewsJob(place: string): Promise<CreateJobResponse> {
    const trimmed = place?.trim();
    if (!trimmed) {
      throw new BadRequestError("place is required");
    }
    return this.request<CreateJobResponse>("POST", "/api/v1/review-jobs", {
      jsonBody: { place: trimmed },
    });
  }

  getReviewsJob(jobId: string): Promise<JobResponse> {
    return this.request<JobResponse>("GET", `/api/v1/review-jobs/${jobId}`);
  }

  getReviewsResults(
    jobId: string,
    options: GetResultsOptions = {},
  ): Promise<ResultsResponse> {
    const query: Record<string, string> = {
      limit: String(options.limit ?? DEFAULT_RESULT_LIMIT),
    };
    if (options.cursor !== undefined && options.cursor !== null) {
      query.cursor = String(options.cursor);
    }
    return this.request<ResultsResponse>(
      "GET",
      `/api/v1/review-jobs/${jobId}/results`,
      { query },
    );
  }

  createPhotosJob(place: string): Promise<CreateJobResponse> {
    const trimmed = place?.trim();
    if (!trimmed) {
      throw new BadRequestError("place is required");
    }
    return this.request<CreateJobResponse>("POST", "/api/v1/photo-jobs", {
      jsonBody: { place: trimmed },
    });
  }

  getPhotosJob(jobId: string): Promise<JobResponse> {
    return this.request<JobResponse>("GET", `/api/v1/photo-jobs/${jobId}`);
  }

  getPhotosResults(
    jobId: string,
    options: GetResultsOptions = {},
  ): Promise<ResultsResponse> {
    const query: Record<string, string> = {
      limit: String(options.limit ?? DEFAULT_RESULT_LIMIT),
    };
    if (options.cursor !== undefined && options.cursor !== null) {
      query.cursor = String(options.cursor);
    }
    return this.request<ResultsResponse>(
      "GET",
      `/api/v1/photo-jobs/${jobId}/results`,
      { query },
    );
  }

  async scrape(keyword: string, options: ScrapeOptions = {}): Promise<PlaceRow[]> {
    const pollIntervalMs = options.pollIntervalMs ?? DEFAULT_POLL_INTERVAL_MS;
    const timeoutMs = options.timeoutMs ?? DEFAULT_TIMEOUT_MS;
    const resultLimit = options.resultLimit ?? DEFAULT_RESULT_LIMIT;

    const created = await this.createJob(keyword);
    const jobId = created.jobId;
    await this.waitForJob(jobId, pollIntervalMs, timeoutMs, "maps");
    return this.fetchAllRows(jobId, resultLimit, "maps");
  }

  async scrapeReviews(place: string, options: ScrapeOptions = {}): Promise<PlaceRow[]> {
    const pollIntervalMs = options.pollIntervalMs ?? DEFAULT_POLL_INTERVAL_MS;
    const timeoutMs = options.timeoutMs ?? DEFAULT_TIMEOUT_MS;
    const resultLimit = options.resultLimit ?? DEFAULT_RESULT_LIMIT;

    const created = await this.createReviewsJob(place);
    const jobId = created.jobId;
    await this.waitForJob(jobId, pollIntervalMs, timeoutMs, "reviews");
    return this.fetchAllRows(jobId, resultLimit, "reviews");
  }

  async scrapePhotos(place: string, options: ScrapeOptions = {}): Promise<PlaceRow[]> {
    const pollIntervalMs = options.pollIntervalMs ?? DEFAULT_POLL_INTERVAL_MS;
    const timeoutMs = options.timeoutMs ?? DEFAULT_TIMEOUT_MS;
    const resultLimit = options.resultLimit ?? DEFAULT_RESULT_LIMIT;

    const created = await this.createPhotosJob(place);
    const jobId = created.jobId;
    await this.waitForJob(jobId, pollIntervalMs, timeoutMs, "photos");
    return this.fetchAllRows(jobId, resultLimit, "photos");
  }

  private async waitForJob(
    jobId: string,
    pollIntervalMs: number,
    timeoutMs: number,
    kind: "maps" | "reviews" | "photos" = "maps",
  ): Promise<void> {
    const deadline = Date.now() + timeoutMs;
    for (;;) {
      const job =
        kind === "reviews"
          ? await this.getReviewsJob(jobId)
          : kind === "photos"
            ? await this.getPhotosJob(jobId)
            : await this.getJob(jobId);
      const status = (job.status ?? "").toLowerCase();
      if (TERMINAL_STATUSES.has(status)) {
        if (status === "failed") {
          throw new ApiError(job.error || `Job ${jobId} failed`, { body: job });
        }
        return;
      }
      if (Date.now() >= deadline) {
        throw new TimeoutError(
          `Timed out after ${timeoutMs}ms waiting for job ${jobId} (last status=${job.status})`,
        );
      }
      await sleep(Math.max(pollIntervalMs, 100));
    }
  }

  private async fetchAllRows(
    jobId: string,
    resultLimit: number,
    kind: "maps" | "reviews" | "photos",
  ): Promise<PlaceRow[]> {
    const rows: PlaceRow[] = [];
    let cursor: string | null = "0";
    while (cursor !== null) {
      const page: ResultsResponse =
        kind === "reviews"
          ? await this.getReviewsResults(jobId, { limit: resultLimit, cursor })
          : kind === "photos"
            ? await this.getPhotosResults(jobId, { limit: resultLimit, cursor })
            : await this.getResults(jobId, { limit: resultLimit, cursor });
      if (page.rows?.length) {
        rows.push(...page.rows);
      }
      cursor =
        page.nextCursor === undefined || page.nextCursor === null
          ? null
          : String(page.nextCursor);
    }
    return rows;
  }

  private async request<T>(
    method: string,
    path: string,
    opts?: {
      jsonBody?: Record<string, unknown>;
      query?: Record<string, string>;
    },
  ): Promise<T> {
    const url = new URL(`${this.baseUrl}${path}`);
    if (opts?.query) {
      for (const [k, v] of Object.entries(opts.query)) {
        url.searchParams.set(k, v);
      }
    }

    const headers: Record<string, string> = {
      Authorization: `Bearer ${this.apiKey}`,
      Accept: "application/json",
      "User-Agent": USER_AGENT,
    };

    const init: RequestInit = { method, headers };
    if (opts?.jsonBody) {
      headers["Content-Type"] = "application/json";
      init.body = JSON.stringify(opts.jsonBody);
    }

    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), this.timeoutMs);
    init.signal = controller.signal;

    try {
      const res = await fetch(url, init);
      const text = await res.text();
      let body: unknown = {};
      if (text) {
        try {
          body = JSON.parse(text);
        } catch {
          body = { message: text };
        }
      }
      if (!res.ok) {
        raiseForStatus(res.status, body);
      }
      return body as T;
    } catch (err) {
      if (err instanceof ApiError) throw err;
      throw new ApiError(
        err instanceof Error ? `Network error: ${err.message}` : "Network error",
      );
    } finally {
      clearTimeout(timer);
    }
  }
}
