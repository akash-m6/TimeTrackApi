using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TimeTrack.API.Data;
using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly TimeTrackDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(TimeTrackDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
    {
        if (predicate == null)
            return await _dbSet.CountAsync();
        
        return await _dbSet.CountAsync(predicate);
    }
}