using BusinessObjects.DTOs;
using BusinessObjects.DTOs.MenuItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace backendapi.Controllers;

[ApiController]
[Route("api/menu-items")]
[Authorize]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuItemService _menuItemService;

    public MenuItemsController(IMenuItemService menuItemService)
    {
        _menuItemService = menuItemService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<MenuItemDto>>>> Get([FromQuery] MenuItemQueryParams queryParams)
    {
        var result = await _menuItemService.GetFilteredAsync(queryParams);
        return Ok(ApiResponse<PagedResult<MenuItemDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> GetById(int id)
    {
        var result = await _menuItemService.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound(ApiResponse<MenuItemDto>.Fail("Menu item not found"));
        }

        return Ok(ApiResponse<MenuItemDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> Create([FromBody] MenuItemRequest request)
    {
        var result = await _menuItemService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<MenuItemDto>.Ok(result, "Menu item created"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] MenuItemRequest request)
    {
        var success = await _menuItemService.UpdateAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("Menu item not found"));
        }

        return Ok(ApiResponse.Ok("Menu item updated"));
    }

    [HttpPatch("{id:int}/availability")]
    public async Task<ActionResult<ApiResponse>> ToggleAvailability(int id, [FromBody] ToggleAvailabilityRequest request)
    {
        var success = await _menuItemService.ToggleAvailabilityAsync(id, request.IsAvailable);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("Menu item not found"));
        }

        return Ok(ApiResponse.Ok("Menu item availability updated"));
    }
}
