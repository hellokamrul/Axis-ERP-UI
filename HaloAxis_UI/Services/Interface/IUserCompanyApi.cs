using HaloAxis_UI.Models;

namespace HaloAxis_UI.Services.Interface
{
    public interface IUserCompanyApi
    {
        Task<List<CompanyLite>> GetByUserAsync(string userId, string accessToken, CancellationToken ct = default);

        // Matches your Swagger: POST /api/v1/UserCompany with { userId, comId, companyName }
        Task<bool> AttachAsync(string userId, string comId, string companyName,
                               string accessToken, CancellationToken ct = default);
    }
}
