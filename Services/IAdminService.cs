using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Admin;

namespace Services;

public interface IAdminService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    Task<RevenueStatisticsDto> GetRevenueStatisticsAsync(string period);
    Task<IEnumerable<TopItemDto>> GetTopItemsAsync(DateTime? from, DateTime? to, int limit);
    Task<PagedResult<StaffDto>> GetStaffAsync(StaffQueryParams queryParams);
    Task<StaffDto?> GetStaffByIdAsync(int id);
    Task<StaffDto> CreateStaffAsync(StaffRequest request);
    Task<bool> UpdateStaffAsync(int id, StaffUpdateRequest request);
    Task<bool> ToggleStaffActiveAsync(int id, bool isActive);
}
