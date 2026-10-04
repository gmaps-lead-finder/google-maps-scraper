//! HTTP client for the GMaps Lead Finder Agent API.

use crate::errors::{raise_for_status, Error, Result};
use crate::types::{
    is_terminal_status, ClientOptions, CreateJobResponse, GetResultsOptions, JobResponse,
    MeResponse, PlaceRow, ResultsResponse, ScrapeOptions,
};
use reqwest::blocking::Client as HttpClient;
use reqwest::header::{HeaderMap, HeaderValue, AUTHORIZATION, CONTENT_TYPE, USER_AGENT};
use serde_json::json;
use std::env;
use std::thread;
use std::time::{Duration, Instant};

const DEFAULT_BASE_URL: &str = "https://gmapsleadfinder.com";
const DEFAULT_POLL_INTERVAL_MS: u64 = 2000;
const DEFAULT_TIMEOUT_MS: u64 = 600_000;
const DEFAULT_RESULT_LIMIT: u32 = 100;
const DEFAULT_HTTP_TIMEOUT_MS: u64 = 60_000;
const UA: &str = "gmaps-scraper-rust/0.1.3";

pub struct Client {
    base_url: String,
    http: HttpClient,
}

impl Client {
    /// Build a client from options / `GMF_API_KEY` / `GMF_BASE_URL`.
    pub fn new(opts: ClientOptions) -> Result<Self> {
        let api_key = opts
            .api_key
            .filter(|s| !s.trim().is_empty())
            .or_else(|| env::var("GMF_API_KEY").ok())
            .map(|s| s.trim().to_string())
            .filter(|s| !s.is_empty())
            .ok_or_else(|| Error::Authentication {
                message: "Missing API key. Set GMF_API_KEY or pass api_key (https://gmapsleadfinder.com/account#api-key)".into(),
                status_code: None,
                body: None,
            })?;

        let base_url = opts
            .base_url
            .filter(|s| !s.trim().is_empty())
            .or_else(|| env::var("GMF_BASE_URL").ok())
            .unwrap_or_else(|| DEFAULT_BASE_URL.to_string())
            .trim_end_matches('/')
            .to_string();

        let timeout_ms = opts.timeout_ms.unwrap_or(DEFAULT_HTTP_TIMEOUT_MS);
        let mut headers = HeaderMap::new();
        headers.insert(
            AUTHORIZATION,
            HeaderValue::from_str(&format!("Bearer {api_key}")).map_err(|e| Error::Api {
                message: format!("Invalid API key header: {e}"),
                status_code: None,
                body: None,
            })?,
        );
        headers.insert(USER_AGENT, HeaderValue::from_static(UA));

        let http = HttpClient::builder()
            .default_headers(headers)
            .timeout(Duration::from_millis(timeout_ms))
            .build()?;

        Ok(Self { base_url, http })
    }

    pub fn me(&self) -> Result<MeResponse> {
        self.request("GET", "/api/v1/me", None, None)
    }

    pub fn create_job(&self, keyword: &str) -> Result<CreateJobResponse> {
        let trimmed = keyword.trim();
        if trimmed.is_empty() {
            return Err(Error::BadRequest {
                message: "keyword is required".into(),
                status_code: None,
                body: None,
            });
        }
        self.request(
            "POST",
            "/api/v1/jobs",
            Some(json!({ "keyword": trimmed })),
            None,
        )
    }

    pub fn get_job(&self, job_id: &str) -> Result<JobResponse> {
        self.request("GET", &format!("/api/v1/jobs/{job_id}"), None, None)
    }

    pub fn get_results(&self, job_id: &str, opts: GetResultsOptions) -> Result<ResultsResponse> {
        let limit = opts.limit.unwrap_or(DEFAULT_RESULT_LIMIT);
        let mut query = vec![("limit".to_string(), limit.to_string())];
        if let Some(cursor) = opts.cursor {
            query.push(("cursor".to_string(), cursor));
        }
        self.request(
            "GET",
            &format!("/api/v1/jobs/{job_id}/results"),
            None,
            Some(query),
        )
    }

    pub fn create_reviews_job(&self, place: &str) -> Result<CreateJobResponse> {
        let trimmed = place.trim();
        if trimmed.is_empty() {
            return Err(Error::BadRequest {
                message: "place is required".into(),
                status_code: None,
                body: None,
            });
        }
        self.request(
            "POST",
            "/api/v1/review-jobs",
            Some(json!({ "place": trimmed })),
            None,
        )
    }

    pub fn get_reviews_job(&self, job_id: &str) -> Result<JobResponse> {
        self.request("GET", &format!("/api/v1/review-jobs/{job_id}"), None, None)
    }

    pub fn get_reviews_results(
        &self,
        job_id: &str,
        opts: GetResultsOptions,
    ) -> Result<ResultsResponse> {
        let limit = opts.limit.unwrap_or(DEFAULT_RESULT_LIMIT);
        let mut query = vec![("limit".to_string(), limit.to_string())];
        if let Some(cursor) = opts.cursor {
            query.push(("cursor".to_string(), cursor));
        }
        self.request(
            "GET",
            &format!("/api/v1/review-jobs/{job_id}/results"),
            None,
            Some(query),
        )
    }

    pub fn create_photos_job(&self, place: &str) -> Result<CreateJobResponse> {
        let trimmed = place.trim();
        if trimmed.is_empty() {
            return Err(Error::BadRequest {
                message: "place is required".into(),
                status_code: None,
                body: None,
            });
        }
        self.request(
            "POST",
            "/api/v1/photo-jobs",
            Some(json!({ "place": trimmed })),
            None,
        )
    }

    pub fn get_photos_job(&self, job_id: &str) -> Result<JobResponse> {
        self.request("GET", &format!("/api/v1/photo-jobs/{job_id}"), None, None)
    }

    pub fn get_photos_results(
        &self,
        job_id: &str,
        opts: GetResultsOptions,
    ) -> Result<ResultsResponse> {
        let limit = opts.limit.unwrap_or(DEFAULT_RESULT_LIMIT);
        let mut query = vec![("limit".to_string(), limit.to_string())];
        if let Some(cursor) = opts.cursor {
            query.push(("cursor".to_string(), cursor));
        }
        self.request(
            "GET",
            &format!("/api/v1/photo-jobs/{job_id}/results"),
            None,
            Some(query),
        )
    }

    /// Create a job, poll until terminal, return all rows.
    pub fn scrape(&self, keyword: &str, opts: ScrapeOptions) -> Result<Vec<PlaceRow>> {
        let result_limit = opts.result_limit.unwrap_or(DEFAULT_RESULT_LIMIT);
        let created = self.create_job(keyword)?;
        self.wait_for_job(&created.job_id, opts, "maps")?;
        self.fetch_all_rows(&created.job_id, result_limit, "maps")
    }

    /// Create a reviews job, poll until terminal, return all review rows.
    pub fn scrape_reviews(&self, place: &str, opts: ScrapeOptions) -> Result<Vec<PlaceRow>> {
        let result_limit = opts.result_limit.unwrap_or(DEFAULT_RESULT_LIMIT);
        let created = self.create_reviews_job(place)?;
        self.wait_for_job(&created.job_id, opts, "reviews")?;
        self.fetch_all_rows(&created.job_id, result_limit, "reviews")
    }

    /// Create a photos job, poll until terminal, return all photo rows.
    pub fn scrape_photos(&self, place: &str, opts: ScrapeOptions) -> Result<Vec<PlaceRow>> {
        let result_limit = opts.result_limit.unwrap_or(DEFAULT_RESULT_LIMIT);
        let created = self.create_photos_job(place)?;
        self.wait_for_job(&created.job_id, opts, "photos")?;
        self.fetch_all_rows(&created.job_id, result_limit, "photos")
    }

    fn wait_for_job(&self, job_id: &str, opts: ScrapeOptions, kind: &str) -> Result<()> {
        let poll_ms = opts
            .poll_interval_ms
            .unwrap_or(DEFAULT_POLL_INTERVAL_MS)
            .max(100);
        let timeout_ms = opts.timeout_ms.unwrap_or(DEFAULT_TIMEOUT_MS);
        let deadline = Instant::now() + Duration::from_millis(timeout_ms);

        loop {
            let job = match kind {
                "reviews" => self.get_reviews_job(job_id)?,
                "photos" => self.get_photos_job(job_id)?,
                _ => self.get_job(job_id)?,
            };
            let status = job.status.clone().unwrap_or_default();
            if is_terminal_status(&status) {
                if status.eq_ignore_ascii_case("failed") {
                    let message = job
                        .error
                        .unwrap_or_else(|| format!("Job {job_id} failed"));
                    return Err(Error::Api {
                        message,
                        status_code: None,
                        body: None,
                    });
                }
                return Ok(());
            }
            if Instant::now() >= deadline {
                return Err(Error::Timeout {
                    message: format!(
                        "Timed out after {timeout_ms}ms waiting for job {job_id} (last status={status:?})"
                    ),
                });
            }
            thread::sleep(Duration::from_millis(poll_ms));
        }
    }

    fn fetch_all_rows(
        &self,
        job_id: &str,
        result_limit: u32,
        kind: &str,
    ) -> Result<Vec<PlaceRow>> {
        let mut rows = Vec::new();
        let mut cursor = Some("0".to_string());
        while let Some(c) = cursor {
            let opts = GetResultsOptions {
                limit: Some(result_limit),
                cursor: Some(c),
            };
            let page = match kind {
                "reviews" => self.get_reviews_results(job_id, opts)?,
                "photos" => self.get_photos_results(job_id, opts)?,
                _ => self.get_results(job_id, opts)?,
            };
            if let Some(batch) = page.rows {
                rows.extend(batch);
            }
            cursor = page.next_cursor.filter(|s| !s.is_empty());
        }
        Ok(rows)
    }

    fn request<T: serde::de::DeserializeOwned>(
        &self,
        method: &str,
        path: &str,
        json_body: Option<serde_json::Value>,
        query: Option<Vec<(String, String)>>,
    ) -> Result<T> {
        let url = format!("{}{path}", self.base_url);
        let mut builder = match method {
            "GET" => self.http.get(&url),
            "POST" => self.http.post(&url),
            other => {
                return Err(Error::Api {
                    message: format!("Unsupported method {other}"),
                    status_code: None,
                    body: None,
                })
            }
        };
        if let Some(q) = query {
            builder = builder.query(&q);
        }
        if let Some(body) = json_body {
            builder = builder
                .header(CONTENT_TYPE, "application/json")
                .json(&body);
        }

        let res = builder.send()?;
        let status = res.status().as_u16();
        let text = res.text().unwrap_or_default();
        let parsed: Option<serde_json::Value> = if text.is_empty() {
            None
        } else {
            serde_json::from_str(&text).ok()
        };

        if !(200..300).contains(&status) {
            return Err(raise_for_status(status, parsed));
        }

        if text.is_empty() {
            return serde_json::from_str("{}").map_err(Error::from);
        }
        serde_json::from_str(&text).map_err(Error::from)
    }
}
