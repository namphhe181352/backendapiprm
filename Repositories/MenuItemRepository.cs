using BusinessObjects.DTOs;
using BusinessObjects.DTOs.MenuItems;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class MenuItemRepository : GenericRepository<MenuItem>, IMenuItemRepository
{
    public MenuItemRepository(Prm393RestaurantContext context) : base(context)
    {
    }

    public async Task<PagedResult<MenuItem>> GetFilteredAsync(MenuItemQueryParams queryParams)
    {
        var query = _dbSet
            .Include(x => x.Category)
            .AsQueryable();

        if (queryParams.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == queryParams.CategoryId.Value);
        }

        if (queryParams.IsAvailable.HasValue)
        {
            query = query.Where(x => x.IsAvailable == queryParams.IsAvailable.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
        {
            var keyword = queryParams.Keyword.Trim();
            query = query.Where(x => x.Name.Contains(keyword));
        }

        var page = queryParams.Page <= 0 ? 1 : queryParams.Page;
        var pageSize = queryParams.PageSize <= 0 ? 10 : queryParams.PageSize;

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<MenuItem>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
