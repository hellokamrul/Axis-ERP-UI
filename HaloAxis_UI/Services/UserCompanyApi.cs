using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HaloAxis_UI.Models;
using HaloAxis_UI.Services.Interface;

namespace HaloAxis_UI.Services
{
    // Typed HttpClient must be registered to the ERP base URL in Program.cs
    public class UserCompanyApi : IUserCompanyApi
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

        public UserCompanyApi(HttpClient http) => _http = http;

        public async Task<List<CompanyLite>> GetByUserAsync(string userId, string accessToken, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var route = $"/api/v1/UserCompany/user/{Uri.EscapeDataString(userId)}";
            using var res = await _http.GetAsync(route, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"ERP GET {route} failed: {(int)res.StatusCode} {res.ReasonPhrase}. Body:\n{body}");

            var wires = ParseUserCompanies(body);

            return wires
                .Select(x =>
                {
                    var id = !string.IsNullOrEmpty(x.ComId) ? x.ComId
                           : !string.IsNullOrEmpty(x.CompanyId) ? x.CompanyId
                           : x.Company?.Id ?? "";
                    var name = !string.IsNullOrEmpty(x.CompanyName) ? x.CompanyName
                             : x.Company?.Name ?? id;
                    return new CompanyLite(id, name);
                })
                .Where(c => !string.IsNullOrWhiteSpace(c.CompanyId))
                .ToList();
        }

        // Robust parser: unwrap JSON-as-string, support $values, wrappers, arrays, single object
        private static List<UserCompanyWire> ParseUserCompanies(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return new();
            var t = body.Trim();

            // unwrap "\"{ ... }\"" -> "{ ... }" (do a couple passes just in case)
            for (int i = 0; i < 3 && t.Length > 0 && t[0] == '\"'; i++)
            {
                try
                {
                    var inner = JsonSerializer.Deserialize<string>(t, _json);
                    if (string.IsNullOrWhiteSpace(inner)) break;
                    t = inner.Trim();
                }
                catch { break; }
            }

            // plain array
            if (t.StartsWith("["))
                return JsonSerializer.Deserialize<List<UserCompanyWire>>(t, _json) ?? new();

            // object: try to find an array property ($values/data/items/...)
            using var doc = JsonDocument.Parse(t);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Value.ValueKind != JsonValueKind.Array) continue;

                    if (IsMatch(prop.Name, "$values", "values", "data", "items", "result", "results", "value", "payload", "companies"))
                    {
                        return JsonSerializer.Deserialize<List<UserCompanyWire>>(prop.Value.GetRawText(), _json) ?? new();
                    }
                }

                // treat the whole object as a single record
                var one = JsonSerializer.Deserialize<UserCompanyWire>(t, _json);
                return one is null ? new() : new() { one };
            }

            // anything else -> empty
            return new();
        }

        private static bool IsMatch(string actual, params string[] candidates)
        {
            foreach (var expected in candidates)
            {
                if (actual.Equals(expected, StringComparison.OrdinalIgnoreCase)) return true;
                if (actual.Length > 1 && actual[0] == '$' &&
                    actual.Substring(1).Equals(expected, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public async Task<bool> AttachAsync(string userId, string comId, string companyName,
                                            string accessToken, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var payload = new { userId, comId, companyName }; // EXACTLY matches Swagger
            var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var res = await _http.PostAsync("/api/v1/UserCompany", json, ct);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"ERP attach failed: {(int)res.StatusCode} {res.ReasonPhrase}. Body: {body}");
            }
            return true;
        }
    }

    // Single, canonical wire model (remove any duplicates elsewhere)
    public class UserCompanyWire
    {
        [JsonPropertyName("userId")] public string UserId { get; set; } = "";
        [JsonPropertyName("comId")] public string? ComId { get; set; }
        [JsonPropertyName("companyId")] public string? CompanyId { get; set; }
        [JsonPropertyName("companyName")] public string? CompanyName { get; set; }
        public CompanyDto? Company { get; set; } // for nested shapes if ERP ever returns them
    }
}
