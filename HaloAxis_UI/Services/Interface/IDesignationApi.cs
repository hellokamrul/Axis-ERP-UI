using HaloAxis_UI.Models;

namespace HaloAxis_UI.Services.Interface
{
    public interface IDesignationApi
    {
        Task<List<DesignationDto>> ListByCompanyAsync(string comId, string accessToken, CancellationToken ct = default);
        Task<DesignationDto?> GetAsync(string id, string accessToken, CancellationToken ct = default);
        Task CreateAsync(DesignationDto dto, string accessToken, CancellationToken ct = default);
        Task UpdateAsync(DesignationDto dto, string accessToken, CancellationToken ct = default);
        Task DeleteAsync(string id, string accessToken, CancellationToken ct = default);
    }
}
