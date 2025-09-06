using HaloAxis_UI.Models;

namespace HaloAxis_UI.Services.Interface
{
    public interface ICompanyApi
    {
        Task<CompanyCreatedDto?> CreateAsync(CompanyCreateRequest req, string accessToken, CancellationToken ct = default);
    }
}
