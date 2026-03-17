using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Common;
using BusinessObjects.DTOs.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace backendapi.Controllers;

[ApiController]
[Route("api/tables")]
[Authorize]
public class TablesController : ControllerBase
{
    private readonly ITableService _tableService;

    public TablesController(ITableService tableService)
    {
        _tableService = tableService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TableDto>>>> Get([FromQuery] TableQueryParams queryParams)
    {
        var result = await _tableService.GetFilteredAsync(queryParams);
        return Ok(ApiResponse<PagedResult<TableDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TableDto>>> GetById(int id)
    {
        var result = await _tableService.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound(ApiResponse<TableDto>.Fail("Table not found"));
        }

        return Ok(ApiResponse<TableDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TableDto>>> Create([FromBody] TableRequest request)
    {
        var result = await _tableService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<TableDto>.Ok(result, "Table created"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] TableRequest request)
    {
        var success = await _tableService.UpdateAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("Table not found"));
        }

        return Ok(ApiResponse.Ok("Table updated"));
    }

    [HttpPatch("{id:int}/active")]
    public async Task<ActionResult<ApiResponse>> ToggleActive(int id, [FromBody] ToggleActiveRequest request)
    {
        var success = await _tableService.ToggleActiveAsync(id, request.IsActive);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("Table not found"));
        }

        return Ok(ApiResponse.Ok("Table active status updated"));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ApiResponse>> ChangeStatus(int id, [FromBody] ChangeTableStatusRequest request)
    {
        var success = await _tableService.ChangeStatusAsync(id, request.Status);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("Table not found"));
        }

        return Ok(ApiResponse.Ok("Table status updated"));
    }
}
