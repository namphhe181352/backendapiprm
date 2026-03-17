using BusinessObjects.DTOs.Areas;

namespace Services;

public interface IAreaService
{
    Task<IEnumerable<AreaDto>> GetAllAsync();
    Task<AreaDto?> GetByIdAsync(int id);
    Task<AreaDto> CreateAsync(AreaRequest request);
    Task<bool> UpdateAsync(int id, AreaRequest request);
    Task<bool> ToggleActiveAsync(int id, bool isActive);
}
