using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Tables;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class TableRepository : GenericRepository<DiningTable>, ITableRepository
{
    public TableRepository(Prm393RestaurantContext context) : base(context)
    {
    }

    public async Task<PagedResult<DiningTable>> GetFilteredAsync(TableQueryParams queryParams)
    {
        var query = _dbSet
            .Include(x => x.Area)
            .AsQueryable();

        if (queryParams.AreaId.HasValue)
        {
            query = query.Where(x => x.AreaId == queryParams.AreaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Status))
        {
            query = query.Where(x => x.Status == queryParams.Status);
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

        return new PagedResult<DiningTable>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
