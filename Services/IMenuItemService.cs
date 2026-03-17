using BusinessObjects.DTOs;
using BusinessObjects.DTOs.MenuItems;

namespace Services;

public interface IMenuItemService
{
    Task<PagedResult<MenuItemDto>> GetFilteredAsync(MenuItemQueryParams queryParams);
    Task<MenuItemDto?> GetByIdAsync(int id);
    Task<MenuItemDto> CreateAsync(MenuItemRequest request);
    Task<bool> UpdateAsync(int id, MenuItemRequest request);
    Task<bool> ToggleAvailabilityAsync(int id, bool isAvailable);
}
