using HaloAxis_UI.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace HaloAxis_UI.Services.Interface
{
    public interface IAuthApi
    {
        Task<AuthUser?> RegisterAsync(Models.RegisterRequest req, CancellationToken ct = default);
        Task<AuthUser?> LoginAsync(Models.LoginRequest req, CancellationToken ct = default);
        Task<AuthUser?> CurrentUserAsync(string accessToken, CancellationToken ct = default);
    }

}
