// Services/ApiClient.cs
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HaloAxis_UI.Services
{
    public sealed class ApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;

        private static readonly JsonSerializerOptions J = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null // keep whatever casing you send
        };

        public ApiClient(IHttpClientFactory factory, IHttpContextAccessor ctx)
        {
            _http = factory.CreateClient("ErpApi");
            _ctx = ctx;

            // Ensure we always ask for JSON
            if (!_http.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
                _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void AddBearerIfPresent()
        {
            var token = _ctx.HttpContext?.Session.GetString("jwtToken");
            if (!string.IsNullOrWhiteSpace(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // ---------- GET (single) ----------
        public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default)
        {
            AddBearerIfPresent();
            using var res = await _http.GetAsync(path, ct);
            var body = await res.Content.ReadAsStringAsync(ct);
            EnsureSuccess(res, path, body);
            return JsonSerializer.Deserialize<T>(Unwrap(body), J);
        }

        // ---------- GET (list; unwrap {data/items/value/...}) ----------
        public async Task<List<T>> GetListAsync<T>(string path, CancellationToken ct = default)
        {
            AddBearerIfPresent();
            using var res = await _http.GetAsync(path, ct);
            var body = await res.Content.ReadAsStringAsync(ct);
            EnsureSuccess(res, path, body);

            var t = Unwrap(body).Trim();
            if (string.IsNullOrWhiteSpace(t)) return new();

            if (t.StartsWith("[")) return JsonSerializer.Deserialize<List<T>>(t, J) ?? new();

            using var doc = JsonDocument.Parse(t);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
                return JsonSerializer.Deserialize<List<T>>(root.GetRawText(), J) ?? new();

            if (root.ValueKind == JsonValueKind.Object)
            {
                string[] keys = { "data", "items", "results", "value", "values", "$values", "list" };
                foreach (var p in root.EnumerateObject())
                {
                    if (keys.Contains(p.Name, StringComparer.OrdinalIgnoreCase) &&
                        p.Value.ValueKind == JsonValueKind.Array)
                        return JsonSerializer.Deserialize<List<T>>(p.Value.GetRawText(), J) ?? new();
                }
                foreach (var p in root.EnumerateObject())
                {
                    if (keys.Contains(p.Name, StringComparer.OrdinalIgnoreCase) &&
                        p.Value.ValueKind == JsonValueKind.Object)
                    {
                        var single = JsonSerializer.Deserialize<T>(p.Value.GetRawText(), J);
                        return single is null ? new() : new() { single };
                    }
                }
                var one = JsonSerializer.Deserialize<T>(t, J);
                return one is null ? new() : new() { one };
            }

            return new();
        }

        // ---------- POST (typed response) ----------
        public async Task<TRes?> PostJsonAsync<TReq, TRes>(string path, TReq payload, CancellationToken ct = default)
        {
            AddBearerIfPresent();
            using var res = await _http.PostAsync(path, AsJson(payload), ct);
            var body = await res.Content.ReadAsStringAsync(ct);
            EnsureSuccess(res, path, body);
            return typeof(TRes) == typeof(string)
                ? (TRes?)(object?)Unwrap(body)
                : JsonSerializer.Deserialize<TRes>(Unwrap(body), J);
        }

        // ---------- POST (no typed result, returns HttpResponseMessage for rare cases) ----------
        public async Task<HttpResponseMessage> PostAsync<TReq>(string path, TReq payload, CancellationToken ct = default)
        {
            AddBearerIfPresent();
            return await _http.PostAsync(path, AsJson(payload), ct);
        }

        // ---------- PUT (typed response) ----------
        public async Task<TRes?> PutJsonAsync<TReq, TRes>(string path, TReq payload, CancellationToken ct = default)
        {
            AddBearerIfPresent();
            using var res = await _http.PutAsync(path, AsJson(payload), ct);
            var body = await res.Content.ReadAsStringAsync(ct);
            EnsureSuccess(res, path, body);
            return typeof(TRes) == typeof(string)
                ? (TRes?)(object?)Unwrap(body)
                : JsonSerializer.Deserialize<TRes>(Unwrap(body), J);
        }

        public async Task<HttpResponseMessage> PutAsync<TReq>(string path, TReq payload, CancellationToken ct = default)
        {
            AddBearerIfPresent();
            return await _http.PutAsync(path, AsJson(payload), ct);
        }

        // ---------- DELETE ----------
        public async Task DeleteAsync(string path, CancellationToken ct = default)
        {
            AddBearerIfPresent();
            using var res = await _http.DeleteAsync(path, ct);
            var body = await res.Content.ReadAsStringAsync(ct);
            EnsureSuccess(res, path, body);
        }

        // ---------- helpers ----------
        private static StringContent AsJson<T>(T payload) =>
            new StringContent(JsonSerializer.Serialize(payload, J), Encoding.UTF8, "application/json");

        private static string Unwrap(string body)
        {
            var t = (body ?? "").Trim();
            if (t.StartsWith("\""))
            {
                try { t = JsonSerializer.Deserialize<string>(t) ?? t; } catch { }
            }
            return t;
        }

        private static void EnsureSuccess(HttpResponseMessage res, string path, string body)
        {
            if (res.IsSuccessStatusCode) return;

            // Try to pretty-print ProblemDetails/validation errors if present
            string detail = TryExtractProblemDetail(body);
            var msg = $"HTTP {(int)res.StatusCode} {res.StatusCode} for {path}"
                      + (string.IsNullOrWhiteSpace(detail) ? $": {body}" : $": {detail}");
            throw new HttpRequestException(msg);
        }

        private static string TryExtractProblemDetail(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return "";

                string? title = root.TryGetProperty("title", out var t) ? t.GetString() : null;
                string? type = root.TryGetProperty("type", out var ty) ? ty.GetString() : null;
                string? status = root.TryGetProperty("status", out var s) ? s.ToString() : null;

                // Validation errors dictionary
                string validation = "";
                if (root.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Object)
                {
                    var lines = new List<string>();
                    foreach (var p in errs.EnumerateObject())
                    {
                        var arr = p.Value;
                        if (arr.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var v in arr.EnumerateArray())
                                lines.Add($"{p.Name}: {v.GetString()}");
                        }
                    }
                    validation = string.Join("; ", lines);
                }

                var parts = new[] { title, $"type={type}", $"status={status}", validation }
                    .Where(x => !string.IsNullOrWhiteSpace(x));
                return string.Join(" | ", parts);
            }
            catch { return ""; }
        }
    }
}
