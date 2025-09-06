// Services/AuthApi.cs
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HaloAxis_UI.Models;
using HaloAxis_UI.Services.Interface;

namespace HaloAxis_UI.Services
{
    public class AuthApi : IAuthApi
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions J = new() { PropertyNameCaseInsensitive = true };

        public AuthApi(HttpClient http) => _http = http;

        public async Task<AuthUser?> RegisterAsync(Models.RegisterRequest req, CancellationToken ct = default)
        {
            var res = await _http.PostAsync("/api/Auth/register", Json(req), ct);
            if (!res.IsSuccessStatusCode) return null;
            return await Read<AuthUser>(res);
        }

        public async Task<AuthUser?> LoginAsync(Models.LoginRequest req, CancellationToken ct = default)
        {
            var res = await _http.PostAsync("/api/Auth/login", Json(req), ct);
            if (!res.IsSuccessStatusCode) return null;
            return await Read<AuthUser>(res);
        }

        public async Task<AuthUser?> CurrentUserAsync(string accessToken, CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var res = await _http.GetAsync("/api/Auth/current-user", ct);
            if (!res.IsSuccessStatusCode) return null;
            return await Read<AuthUser>(res);
        }

        private static StringContent Json<T>(T obj) =>
            new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        private static async Task<T?> Read<T>(HttpResponseMessage r) =>
            JsonSerializer.Deserialize<T>(await r.Content.ReadAsStringAsync(), J);
    }
}
