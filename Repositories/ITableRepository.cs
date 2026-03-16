using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Tables;
using BusinessObjects.Models;

namespace Repositories;

public interface ITableRepository : IGenericRepository<DiningTable>
{
    Task<PagedResult<DiningTable>> GetFilteredAsync(TableQueryParams queryParams);
}
