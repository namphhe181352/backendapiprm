using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Areas;
using BusinessObjects.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace backendapi.Controllers;

[ApiController]
[Route("api/areas")]
[Authorize]
public class AreasController : ControllerBase
{
    private readonly IAreaService _areaService;

    public AreasController(IAreaService areaService)
    {
        _areaService = areaService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<AreaDto>>>> GetAll()
    {
        var result = await _areaService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<AreaDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<AreaDto>>> GetById(int id)
    {
        var result = await _areaService.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound(ApiResponse<AreaDto>.Fail("Area not found"));
        }

        return Ok(ApiResponse<AreaDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AreaDto>>> Create([FromBody] AreaRequest request)
    {
        var result = await _areaService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<AreaDto>.Ok(result, "Area created"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] AreaRequest request)
    {
        var success = await _areaService.UpdateAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("Area not found"));
        }

        return Ok(ApiResponse.Ok("Area updated"));
    }

    [HttpPatch("{id:int}/active")]
    public async Task<ActionResult<ApiResponse>> ToggleActive(int id, [FromBody] ToggleActiveRequest request)
    {
        var success = await _areaService.ToggleActiveAsync(id, request.IsActive);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("Area not found"));
        }

        return Ok(ApiResponse.Ok("Area active status updated"));
    }
}
