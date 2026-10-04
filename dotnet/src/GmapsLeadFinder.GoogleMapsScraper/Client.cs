using System.Net.Http.Headers;
using System.Text.Json;
using GmapsLeadFinder.GoogleMapsScraper.Exceptions;

namespace GmapsLeadFinder.GoogleMapsScraper;

public sealed class Client : IDisposable
{
    public const string DefaultBaseUrl = "https://gmapsleadfinder.com";
    public const int DefaultPollIntervalMs = 2000;
    public const int DefaultTimeoutMs = 600_000;
    public const int DefaultResultLimit = 100;
    public const string Version = "0.1.3";
    public const string UserAgent = "gmaps-scraper-dotnet/" + Version;

    private static readonly HashSet<string> TerminalStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "completed", "partial", "failed",
    };

    private readonly HttpClient _http;
    private readonly bool _ownsHttp;

    public Client(ClientOptions? options = null)
        : this(CreateHttpClient(options ?? new ClientOptions()), ownsHttp: true)
    {
    }

    public Client(HttpClient httpClient, ClientOptions? options = null)
        : this(ConfigureHttpClient(httpClient, options ?? new ClientOptions()), ownsHttp: false)
    {
    }

    private Client(HttpClient http, bool ownsHttp)
    {
        _http = http;
        _ownsHttp = ownsHttp;
    }

    private static HttpClient CreateHttpClient(ClientOptions options)
    {
        var http = new HttpClient { Timeout = options.Timeout };
        return ConfigureHttpClient(http, options);
    }

    private static HttpClient ConfigureHttpClient(HttpClient http, ClientOptions options)
    {
        var key = (options.ApiKey ?? Environment.GetEnvironmentVariable("GMF_API_KEY") ?? "").Trim();
        if (string.IsNullOrEmpty(key))
        {
            throw new AuthenticationException(
                "Missing API key. Set GMF_API_KEY or pass ApiKey " +
                "(https://gmapsleadfinder.com/account#api-key)");
        }

        var baseUrl = (options.BaseUrl
            ?? Environment.GetEnvironmentVariable("GMF_BASE_URL")
            ?? DefaultBaseUrl).Trim().TrimEnd('/');

        http.BaseAddress = new Uri(baseUrl + "/");
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
        http.DefaultRequestHeaders.Accept.Clear();
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        http.DefaultRequestHeaders.UserAgent.Clear();
        http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);
        if (http.Timeout == Timeout.InfiniteTimeSpan || http.Timeout == default)
        {
            http.Timeout = options.Timeout;
        }

        return http;
    }

    public Task<JsonElement> MeAsync(CancellationToken cancellationToken = default)
        => RequestAsync(HttpMethod.Get, "api/v1/me", null, null, cancellationToken);

    public Task<JsonElement> CreateJobAsync(string keyword, CancellationToken cancellationToken = default)
    {
        var trimmed = keyword?.Trim() ?? "";
        if (trimmed.Length == 0)
        {
            throw new BadRequestException("keyword is required");
        }

        var body = JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, string> { ["keyword"] = trimmed });
        return RequestAsync(HttpMethod.Post, "api/v1/jobs", body, null, cancellationToken);
    }

    public Task<JsonElement> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
        => RequestAsync(HttpMethod.Get, $"api/v1/jobs/{Uri.EscapeDataString(jobId)}", null, null, cancellationToken);

    public Task<JsonElement> GetResultsAsync(
        string jobId,
        GetResultsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new GetResultsOptions();
        var query = new Dictionary<string, string> { ["limit"] = options.Limit.ToString() };
        if (options.Cursor is not null)
        {
            query["cursor"] = options.Cursor;
        }

        return RequestAsync(
            HttpMethod.Get,
            $"api/v1/jobs/{Uri.EscapeDataString(jobId)}/results",
            null,
            query,
            cancellationToken);
    }

    public Task<JsonElement> CreateReviewsJobAsync(string place, CancellationToken cancellationToken = default)
    {
        var trimmed = place?.Trim() ?? "";
        if (trimmed.Length == 0)
        {
            throw new BadRequestException("place is required");
        }

        var body = JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, string> { ["place"] = trimmed });
        return RequestAsync(HttpMethod.Post, "api/v1/review-jobs", body, null, cancellationToken);
    }

    public Task<JsonElement> GetReviewsJobAsync(string jobId, CancellationToken cancellationToken = default)
        => RequestAsync(HttpMethod.Get, $"api/v1/review-jobs/{Uri.EscapeDataString(jobId)}", null, null, cancellationToken);

    public Task<JsonElement> GetReviewsResultsAsync(
        string jobId,
        GetResultsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new GetResultsOptions();
        var query = new Dictionary<string, string> { ["limit"] = options.Limit.ToString() };
        if (options.Cursor is not null)
        {
            query["cursor"] = options.Cursor;
        }

        return RequestAsync(
            HttpMethod.Get,
            $"api/v1/review-jobs/{Uri.EscapeDataString(jobId)}/results",
            null,
            query,
            cancellationToken);
    }

    public Task<JsonElement> CreatePhotosJobAsync(string place, CancellationToken cancellationToken = default)
    {
        var trimmed = place?.Trim() ?? "";
        if (trimmed.Length == 0)
        {
            throw new BadRequestException("place is required");
        }

        var body = JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, string> { ["place"] = trimmed });
        return RequestAsync(HttpMethod.Post, "api/v1/photo-jobs", body, null, cancellationToken);
    }

    public Task<JsonElement> GetPhotosJobAsync(string jobId, CancellationToken cancellationToken = default)
        => RequestAsync(HttpMethod.Get, $"api/v1/photo-jobs/{Uri.EscapeDataString(jobId)}", null, null, cancellationToken);

    public Task<JsonElement> GetPhotosResultsAsync(
        string jobId,
        GetResultsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new GetResultsOptions();
        var query = new Dictionary<string, string> { ["limit"] = options.Limit.ToString() };
        if (options.Cursor is not null)
        {
            query["cursor"] = options.Cursor;
        }

        return RequestAsync(
            HttpMethod.Get,
            $"api/v1/photo-jobs/{Uri.EscapeDataString(jobId)}/results",
            null,
            query,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Dictionary<string, string>>> ScrapeAsync(
        string keyword,
        ScrapeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new ScrapeOptions();
        var created = await CreateJobAsync(keyword, cancellationToken).ConfigureAwait(false);
        var jobId = created.GetProperty("jobId").GetString()
            ?? throw new ApiException("createJob response missing jobId", body: created.ToString());
        await WaitForJobAsync(jobId, options, "maps", cancellationToken).ConfigureAwait(false);
        return await FetchAllRowsAsync(jobId, options.ResultLimit, "maps", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Dictionary<string, string>>> ScrapeReviewsAsync(
        string place,
        ScrapeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new ScrapeOptions();
        var created = await CreateReviewsJobAsync(place, cancellationToken).ConfigureAwait(false);
        var jobId = created.GetProperty("jobId").GetString()
            ?? throw new ApiException("createReviewsJob response missing jobId", body: created.ToString());
        await WaitForJobAsync(jobId, options, "reviews", cancellationToken).ConfigureAwait(false);
        return await FetchAllRowsAsync(jobId, options.ResultLimit, "reviews", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Dictionary<string, string>>> ScrapePhotosAsync(
        string place,
        ScrapeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new ScrapeOptions();
        var created = await CreatePhotosJobAsync(place, cancellationToken).ConfigureAwait(false);
        var jobId = created.GetProperty("jobId").GetString()
            ?? throw new ApiException("createPhotosJob response missing jobId", body: created.ToString());
        await WaitForJobAsync(jobId, options, "photos", cancellationToken).ConfigureAwait(false);
        return await FetchAllRowsAsync(jobId, options.ResultLimit, "photos", cancellationToken).ConfigureAwait(false);
    }

    private async Task WaitForJobAsync(
        string jobId,
        ScrapeOptions options,
        string kind,
        CancellationToken cancellationToken)
    {
        var pollMs = Math.Max(100, options.PollIntervalMs);
        var deadline = DateTime.UtcNow.AddMilliseconds(options.TimeoutMs);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var job = kind switch
            {
                "reviews" => await GetReviewsJobAsync(jobId, cancellationToken).ConfigureAwait(false),
                "photos" => await GetPhotosJobAsync(jobId, cancellationToken).ConfigureAwait(false),
                _ => await GetJobAsync(jobId, cancellationToken).ConfigureAwait(false),
            };
            var status = job.TryGetProperty("status", out var statusEl)
                ? statusEl.GetString()?.ToLowerInvariant() ?? ""
                : "";

            if (TerminalStatuses.Contains(status))
            {
                if (status == "failed")
                {
                    var err = job.TryGetProperty("error", out var errEl) ? errEl.GetString() : null;
                    throw new ApiException(err ?? $"Job {jobId} failed", body: job.ToString());
                }

                return;
            }

            if (DateTime.UtcNow >= deadline)
            {
                var last = job.TryGetProperty("status", out var s) ? s.GetString() : null;
                throw new Exceptions.TimeoutException(
                    $"Timed out after {options.TimeoutMs}ms waiting for job {jobId} (last status={last})");
            }

            await Task.Delay(pollMs, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<IReadOnlyList<Dictionary<string, string>>> FetchAllRowsAsync(
        string jobId,
        int resultLimit,
        string kind,
        CancellationToken cancellationToken)
    {
        var rows = new List<Dictionary<string, string>>();
        string? cursor = "0";
        while (cursor is not null)
        {
            var page = kind switch
            {
                "reviews" => await GetReviewsResultsAsync(
                    jobId,
                    new GetResultsOptions { Limit = resultLimit, Cursor = cursor },
                    cancellationToken).ConfigureAwait(false),
                "photos" => await GetPhotosResultsAsync(
                    jobId,
                    new GetResultsOptions { Limit = resultLimit, Cursor = cursor },
                    cancellationToken).ConfigureAwait(false),
                _ => await GetResultsAsync(
                    jobId,
                    new GetResultsOptions { Limit = resultLimit, Cursor = cursor },
                    cancellationToken).ConfigureAwait(false),
            };

            if (page.TryGetProperty("rows", out var rowsEl) && rowsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var row in rowsEl.EnumerateArray())
                {
                    var dict = new Dictionary<string, string>(StringComparer.Ordinal);
                    foreach (var prop in row.EnumerateObject())
                    {
                        dict[prop.Name] = prop.Value.ValueKind switch
                        {
                            JsonValueKind.String => prop.Value.GetString() ?? "",
                            JsonValueKind.Null => "",
                            _ => prop.Value.ToString(),
                        };
                    }

                    rows.Add(dict);
                }
            }

            if (!page.TryGetProperty("nextCursor", out var next) || next.ValueKind == JsonValueKind.Null)
            {
                cursor = null;
            }
            else
            {
                var nextStr = next.ValueKind == JsonValueKind.String ? next.GetString() : next.ToString();
                cursor = string.IsNullOrEmpty(nextStr) ? null : nextStr;
            }
        }

        return rows;
    }

    private async Task<JsonElement> RequestAsync(
        HttpMethod method,
        string path,
        byte[]? jsonBody,
        IReadOnlyDictionary<string, string>? query,
        CancellationToken cancellationToken)
    {
        var url = path;
        if (query is { Count: > 0 })
        {
            var qs = string.Join("&", query.Select(kv =>
                $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            url = $"{path}?{qs}";
        }

        using var req = new HttpRequestMessage(method, url);
        if (jsonBody is not null)
        {
            req.Content = new ByteArrayContent(jsonBody);
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        HttpResponseMessage res;
        try
        {
            res = await _http.SendAsync(req, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new ApiException($"Network error: {ex.Message}", inner: ex);
        }

        using (res)
        {
            var raw = await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            JsonElement body;
            if (string.IsNullOrEmpty(raw))
            {
                body = default;
            }
            else
            {
                try
                {
                    using var doc = JsonDocument.Parse(raw);
                    body = doc.RootElement.Clone();
                }
                catch (JsonException)
                {
                    body = JsonSerializer.SerializeToElement(new Dictionary<string, string> { ["message"] = raw });
                }
            }

            var status = (int)res.StatusCode;
            if (status < 200 || status >= 300)
            {
                throw RaiseForStatus(status, body);
            }

            return body.ValueKind == JsonValueKind.Undefined
                ? JsonSerializer.SerializeToElement(new Dictionary<string, object?>())
                : body;
        }
    }

    private static ApiException RaiseForStatus(int status, JsonElement body)
    {
        var message = $"API request failed with status {status}";
        if (body.ValueKind == JsonValueKind.Object)
        {
            foreach (var key in new[] { "message", "error", "statusMessage" })
            {
                if (body.TryGetProperty(key, out var el) && el.ValueKind == JsonValueKind.String)
                {
                    var s = el.GetString();
                    if (!string.IsNullOrEmpty(s))
                    {
                        message = s;
                        break;
                    }
                }
            }
        }

        object? bodyObj = body.ValueKind == JsonValueKind.Undefined ? null : body.ToString();
        return status switch
        {
            400 => new BadRequestException(message, status, bodyObj),
            401 => new AuthenticationException(message, status, bodyObj),
            402 => new InsufficientCreditsException(message, status, bodyObj),
            403 => new PlanNotAllowedException(message, status, bodyObj),
            404 => new NotFoundException(message, status, bodyObj),
            409 => new JobConflictException(message, status, bodyObj),
            _ => new ApiException(message, status, bodyObj),
        };
    }

    public void Dispose()
    {
        if (_ownsHttp)
        {
            _http.Dispose();
        }
    }
}
