using HaloAxis_UI.Models;

namespace HaloAxis_UI.Services.Interface
{
    public interface IDepartmentApi
    {
        Task<List<DepartmentDto>> ListByCompanyAsync(string comId, string accessToken, CancellationToken ct = default);
        Task<DepartmentDto?> GetAsync(string id, string accessToken, CancellationToken ct = default);
        Task CreateAsync(DepartmentDto dto, string accessToken, CancellationToken ct = default);
        Task UpdateAsync(DepartmentDto dto, string accessToken, CancellationToken ct = default);
        Task DeleteAsync(string id, string accessToken, CancellationToken ct = default);
    }
}
