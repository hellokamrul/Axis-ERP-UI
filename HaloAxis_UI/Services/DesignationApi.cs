using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HaloAxis_UI.Models;
using HaloAxis_UI.Services.Interface;
using static System.Net.WebRequestMethods;

namespace HaloAxis_UI.Services
{
    public class DesignationApi : IDesignationApi
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions J = new() { PropertyNameCaseInsensitive = true };

        public DesignationApi(HttpClient http) => _http = http;

        public async Task<List<DesignationDto>> ListByCompanyAsync(string comId, string token, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

             var url = $"/api/v1/Desgination/GetListByComid?id={Uri.EscapeDataString(comId)}";
            //var url = $https://localhost:7276/api/v1/Desgination/GetListByComid?id=af9310ee-8242-460a-890c-3e1274fdfff0";
            using var res = await _http.GetAsync(url, ct);
            var body = await res.Content.ReadAsStringAsync(ct);
            res.EnsureSuccessStatusCode();
            return JsonList<DesignationDto>(body);
        }

        public async Task<DesignationDto?> GetAsync(string id, string token, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await _http.GetAsync($"/api/v1/Designation/{Uri.EscapeDataString(id)}", ct);
            var body = await res.Content.ReadAsStringAsync(ct);
            res.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<DesignationDto>(Unwrap(body), J);
        }

        public async Task CreateAsync(DesignationDto dto, string token, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var json = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var res = await _http.PostAsync("/api/v1/Desgination", json);
            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"Designation create failed: {res.StatusCode} - {await res.Content.ReadAsStringAsync(ct)}");
        }

        public async Task UpdateAsync(DesignationDto dto, string token, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var json = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var res = await _http.PutAsync("/api/v1/Designation/Update", json, ct);
            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"Designation update failed: {res.StatusCode} - {await res.Content.ReadAsStringAsync(ct)}");
        }

        public async Task DeleteAsync(string id, string token, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await _http.DeleteAsync($"/api/v1/Designation/{Uri.EscapeDataString(id)}", ct);
            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"Designation delete failed: {res.StatusCode} - {await res.Content.ReadAsStringAsync(ct)}");
        }

        // reuse helpers from DepartmentApi
        private static string Unwrap(string body)
        {
            var t = body?.Trim() ?? "";
            if (t.StartsWith("\""))
            {
                try { t = JsonSerializer.Deserialize<string>(t) ?? t; } catch { }
            }
            return t;
        }
        private static List<T> JsonList<T>(string body)
        {
            var t = Unwrap(body);
            if (t.StartsWith("[")) return JsonSerializer.Deserialize<List<T>>(t, J) ?? new();
            using var doc = JsonDocument.Parse(t);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var p in root.EnumerateObject())
                {
                    if (p.Value.ValueKind == JsonValueKind.Array &&
                        (p.Name.Equals("data", StringComparison.OrdinalIgnoreCase) ||
                         p.Name.Equals("items", StringComparison.OrdinalIgnoreCase) ||
                         p.Name.Equals("results", StringComparison.OrdinalIgnoreCase) ||
                         p.Name.Equals("value", StringComparison.OrdinalIgnoreCase) ||
                         p.Name.Equals("values", StringComparison.OrdinalIgnoreCase) ||
                         (p.Name.Length > 1 && p.Name[0] == '$' && p.Name.Substring(1).Equals("values", StringComparison.OrdinalIgnoreCase))))
                    {
                        return JsonSerializer.Deserialize<List<T>>(p.Value.GetRawText(), J) ?? new();
                    }
                }
                var single = JsonSerializer.Deserialize<T>(t, J);
                return single is null ? new() : new() { single };
            }
            return new();
        }
    }
}
