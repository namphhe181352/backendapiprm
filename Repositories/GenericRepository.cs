using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly Prm393RestaurantContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(Prm393RestaurantContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public virtual async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public virtual void Update(T entity) => _dbSet.Update(entity);

    public virtual void Delete(T entity) => _dbSet.Remove(entity);

    public virtual async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public IQueryable<T> Query() => _dbSet.AsQueryable();
}
