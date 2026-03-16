using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Categories;
using BusinessObjects.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace backendapi.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound(ApiResponse<CategoryDto>.Fail("Category not found"));
        }

        return Ok(ApiResponse<CategoryDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<CategoryDto>.Ok(result, "Category created"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] CategoryRequest request)
    {
        var success = await _categoryService.UpdateAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("Category not found"));
        }

        return Ok(ApiResponse.Ok("Category updated"));
    }

    [HttpPatch("{id:int}/active")]
    public async Task<ActionResult<ApiResponse>> ToggleActive(int id, [FromBody] ToggleActiveRequest request)
    {
        var success = await _categoryService.ToggleActiveAsync(id, request.IsActive);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("Category not found"));
        }

        return Ok(ApiResponse.Ok("Category active status updated"));
    }
}
