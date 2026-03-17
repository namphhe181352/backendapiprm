using BusinessObjects.DTOs;
using BusinessObjects.DTOs.MenuItems;
using BusinessObjects.Models;

namespace Repositories;

public interface IMenuItemRepository : IGenericRepository<MenuItem>
{
    Task<PagedResult<MenuItem>> GetFilteredAsync(MenuItemQueryParams queryParams);
}
