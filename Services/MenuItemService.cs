using BusinessObjects.DTOs;
using BusinessObjects.DTOs.MenuItems;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace Services;

public class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ICategoryRepository _categoryRepository;

    public MenuItemService(IMenuItemRepository menuItemRepository, ICategoryRepository categoryRepository)
    {
        _menuItemRepository = menuItemRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<PagedResult<MenuItemDto>> GetFilteredAsync(MenuItemQueryParams queryParams)
    {
        var result = await _menuItemRepository.GetFilteredAsync(queryParams);

        return new PagedResult<MenuItemDto>
        {
            Items = result.Items.Select(MapMenuItem).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<MenuItemDto?> GetByIdAsync(int id)
    {
        var entity = await _menuItemRepository.Query()
            .Include(x => x.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return entity is null ? null : MapMenuItem(entity);
    }

    public async Task<MenuItemDto> CreateAsync(MenuItemRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId)
            ?? throw new KeyNotFoundException("Category not found.");

        var entity = new MenuItem
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            ImageUrl = request.ImageUrl,
            IsAvailable = request.IsAvailable
        };

        await _menuItemRepository.AddAsync(entity);
        await _menuItemRepository.SaveChangesAsync();

        entity.Category = category;
        return MapMenuItem(entity);
    }

    public async Task<bool> UpdateAsync(int id, MenuItemRequest request)
    {
        var entity = await _menuItemRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null)
        {
            throw new KeyNotFoundException("Category not found.");
        }

        entity.CategoryId = request.CategoryId;
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Price = request.Price;
        entity.ImageUrl = request.ImageUrl;
        entity.IsAvailable = request.IsAvailable;

        _menuItemRepository.Update(entity);
        await _menuItemRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleAvailabilityAsync(int id, bool isAvailable)
    {
        var entity = await _menuItemRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        entity.IsAvailable = isAvailable;
        _menuItemRepository.Update(entity);
        await _menuItemRepository.SaveChangesAsync();
        return true;
    }

    private static MenuItemDto MapMenuItem(MenuItem item) => new()
    {
        Id = item.Id,
        CategoryId = item.CategoryId,
        CategoryName = item.Category?.Name ?? string.Empty,
        Name = item.Name,
        Description = item.Description,
        Price = item.Price,
        ImageUrl = item.ImageUrl,
        IsAvailable = item.IsAvailable
    };
}
