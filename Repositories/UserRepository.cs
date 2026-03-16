using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(Prm393RestaurantContext context) : base(context)
    {
    }

    public Task<User?> GetByUsernameAsync(string username) =>
        _dbSet.FirstOrDefaultAsync(x => x.Username == username);

    public Task<User?> GetByEmailAsync(string email) =>
        _dbSet.FirstOrDefaultAsync(x => x.Email != null && x.Email == email);
}
