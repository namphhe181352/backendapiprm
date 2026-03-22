using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace backendapi.Controllers;

[ApiController]
[Route("api/admin/statistics")]
[Authorize(Roles = "admin")]
public class AdminStatisticsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminStatisticsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<RevenueStatisticsDto>>> Revenue([FromQuery] string period = "today")
    {
        var result = await _adminService.GetRevenueStatisticsAsync(period);
        return Ok(ApiResponse<RevenueStatisticsDto>.Ok(result));
    }

    [HttpGet("top-items")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TopItemDto>>>> TopItems([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int limit = 10)
    {
        var result = await _adminService.GetTopItemsAsync(from, to, limit);
        return Ok(ApiResponse<IEnumerable<TopItemDto>>.Ok(result));
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<StatisticsOverviewDto>>> Overview([FromQuery] string period = "today", [FromQuery] int topLimit = 5)
    {
        var result = await _adminService.GetStatisticsOverviewAsync(period, topLimit);
        return Ok(ApiResponse<StatisticsOverviewDto>.Ok(result));
    }
}
