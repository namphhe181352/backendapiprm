using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Admin;
using BusinessObjects.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace backendapi.Controllers;

[ApiController]
[Route("api/admin/staff")]
[Authorize(Roles = "admin")]
public class AdminStaffController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminStaffController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<StaffDto>>>> Get([FromQuery] StaffQueryParams queryParams)
    {
        var result = await _adminService.GetStaffAsync(queryParams);
        return Ok(ApiResponse<PagedResult<StaffDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<StaffDto>>> GetById(int id)
    {
        var result = await _adminService.GetStaffByIdAsync(id);
        if (result is null)
        {
            return NotFound(ApiResponse<StaffDto>.Fail("Staff not found"));
        }

        return Ok(ApiResponse<StaffDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StaffDto>>> Create([FromBody] StaffRequest request)
    {
        var result = await _adminService.CreateStaffAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<StaffDto>.Ok(result, "Staff created"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] StaffUpdateRequest request)
    {
        var success = await _adminService.UpdateStaffAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("Staff not found"));
        }

        return Ok(ApiResponse.Ok("Staff updated"));
    }

    [HttpPatch("{id:int}/active")]
    public async Task<ActionResult<ApiResponse>> ToggleActive(int id, [FromBody] ToggleActiveRequest request)
    {
        var success = await _adminService.ToggleStaffActiveAsync(id, request.IsActive);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("Staff not found"));
        }

        return Ok(ApiResponse.Ok("Staff active status updated"));
    }
}
