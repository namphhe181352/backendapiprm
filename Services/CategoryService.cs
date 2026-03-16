using BusinessObjects.DTOs.Categories;
using BusinessObjects.Models;
using Repositories;

namespace Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(MapCategory);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category is null ? null : MapCategory(category);
    }

    public async Task<CategoryDto> CreateAsync(CategoryRequest request)
    {
        var entity = new Category
        {
            Name = request.Name,
            ImageUrl = request.ImageUrl,
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        await _categoryRepository.AddAsync(entity);
        await _categoryRepository.SaveChangesAsync();
        return MapCategory(entity);
    }

    public async Task<bool> UpdateAsync(int id, CategoryRequest request)
    {
        var entity = await _categoryRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        entity.Name = request.Name;
        entity.ImageUrl = request.ImageUrl;
        entity.DisplayOrder = request.DisplayOrder;

        _categoryRepository.Update(entity);
        await _categoryRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleActiveAsync(int id, bool isActive)
    {
        var entity = await _categoryRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        entity.IsActive = isActive;
        _categoryRepository.Update(entity);
        await _categoryRepository.SaveChangesAsync();
        return true;
    }

    private static CategoryDto MapCategory(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        ImageUrl = category.ImageUrl,
        DisplayOrder = category.DisplayOrder,
        IsActive = category.IsActive
    };
}
