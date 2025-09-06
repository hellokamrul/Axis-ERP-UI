using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HaloAxis_UI.Models;
using HaloAxis_UI.Services.Interface;

namespace HaloAxis_UI.Services
{
    public class CompanyApi : ICompanyApi
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions JWrite = new()
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        private static readonly JsonSerializerOptions JRead = new() { PropertyNameCaseInsensitive = true };

        public CompanyApi(HttpClient http) => _http = http;

        public async Task<CompanyCreatedDto?> CreateAsync(CompanyCreateRequest req, string accessToken, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var json = new StringContent(JsonSerializer.Serialize(req, JWrite), Encoding.UTF8, "application/json");
            var res = await _http.PostAsync("/api/v1/Company", json, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"ERP POST /api/v1/Company failed: {(int)res.StatusCode} {res.ReasonPhrase}. Body:\n{body}");

            return JsonSerializer.Deserialize<CompanyCreatedDto>(body, JRead);
        }
    }
}
