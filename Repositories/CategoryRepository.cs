using BusinessObjects.Models;

namespace Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(Prm393RestaurantContext context) : base(context)
    {
    }
}
