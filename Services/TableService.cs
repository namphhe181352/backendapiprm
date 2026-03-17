using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Tables;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace Services;

public class TableService : ITableService
{
    private readonly ITableRepository _tableRepository;
    private readonly IAreaRepository _areaRepository;

    public TableService(ITableRepository tableRepository, IAreaRepository areaRepository)
    {
        _tableRepository = tableRepository;
        _areaRepository = areaRepository;
    }

    public async Task<PagedResult<TableDto>> GetFilteredAsync(TableQueryParams queryParams)
    {
        var result = await _tableRepository.GetFilteredAsync(queryParams);

        return new PagedResult<TableDto>
        {
            Items = result.Items.Select(MapTable).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<TableDto?> GetByIdAsync(int id)
    {
        var entity = await _tableRepository.Query()
            .Include(x => x.Area)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return entity is null ? null : MapTable(entity);
    }

    public async Task<TableDto> CreateAsync(TableRequest request)
    {
        var area = await _areaRepository.GetByIdAsync(request.AreaId)
            ?? throw new KeyNotFoundException("Area not found.");

        var entity = new DiningTable
        {
            AreaId = request.AreaId,
            Name = request.Name,
            Capacity = request.Capacity,
            Status = request.Status,
            IsActive = true
        };

        await _tableRepository.AddAsync(entity);
        await _tableRepository.SaveChangesAsync();

        entity.Area = area;
        return MapTable(entity);
    }

    public async Task<bool> UpdateAsync(int id, TableRequest request)
    {
        var entity = await _tableRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        var area = await _areaRepository.GetByIdAsync(request.AreaId);
        if (area is null)
        {
            throw new KeyNotFoundException("Area not found.");
        }

        entity.AreaId = request.AreaId;
        entity.Name = request.Name;
        entity.Capacity = request.Capacity;
        entity.Status = request.Status;

        _tableRepository.Update(entity);
        await _tableRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleActiveAsync(int id, bool isActive)
    {
        var entity = await _tableRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        entity.IsActive = isActive;
        _tableRepository.Update(entity);
        await _tableRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangeStatusAsync(int id, string status)
    {
        var entity = await _tableRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        entity.Status = status;
        _tableRepository.Update(entity);
        await _tableRepository.SaveChangesAsync();
        return true;
    }

    private static TableDto MapTable(DiningTable table) => new()
    {
        Id = table.Id,
        AreaId = table.AreaId,
        AreaName = table.Area?.Name ?? string.Empty,
        Name = table.Name,
        Capacity = table.Capacity,
        Status = table.Status,
        IsActive = table.IsActive
    };
}
