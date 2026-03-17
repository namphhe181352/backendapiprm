using BusinessObjects.Models;

namespace Repositories;

public class AreaRepository : GenericRepository<Area>, IAreaRepository
{
    public AreaRepository(Prm393RestaurantContext context) : base(context)
    {
    }
}
