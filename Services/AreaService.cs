using BusinessObjects.DTOs.Areas;
using BusinessObjects.Models;
using Repositories;

namespace Services;

public class AreaService : IAreaService
{
    private readonly IAreaRepository _areaRepository;

    public AreaService(IAreaRepository areaRepository)
    {
        _areaRepository = areaRepository;
    }

    public async Task<IEnumerable<AreaDto>> GetAllAsync()
    {
        var areas = await _areaRepository.GetAllAsync();
        return areas.Select(MapArea);
    }

    public async Task<AreaDto?> GetByIdAsync(int id)
    {
        var area = await _areaRepository.GetByIdAsync(id);
        return area is null ? null : MapArea(area);
    }

    public async Task<AreaDto> CreateAsync(AreaRequest request)
    {
        var entity = new Area
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true
        };

        await _areaRepository.AddAsync(entity);
        await _areaRepository.SaveChangesAsync();
        return MapArea(entity);
    }

    public async Task<bool> UpdateAsync(int id, AreaRequest request)
    {
        var entity = await _areaRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        entity.Name = request.Name;
        entity.Description = request.Description;

        _areaRepository.Update(entity);
        await _areaRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleActiveAsync(int id, bool isActive)
    {
        var entity = await _areaRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        entity.IsActive = isActive;
        _areaRepository.Update(entity);
        await _areaRepository.SaveChangesAsync();
        return true;
    }

    private static AreaDto MapArea(Area area) => new()
    {
        Id = area.Id,
        Name = area.Name,
        Description = area.Description,
        IsActive = area.IsActive
    };
}
