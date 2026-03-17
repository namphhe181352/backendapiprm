using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Tables;

namespace Services;

public interface ITableService
{
    Task<PagedResult<TableDto>> GetFilteredAsync(TableQueryParams queryParams);
    Task<TableDto?> GetByIdAsync(int id);
    Task<TableDto> CreateAsync(TableRequest request);
    Task<bool> UpdateAsync(int id, TableRequest request);
    Task<bool> ToggleActiveAsync(int id, bool isActive);
    Task<bool> ChangeStatusAsync(int id, string status);
}
