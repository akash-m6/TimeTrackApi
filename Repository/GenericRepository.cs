using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TimeTrack.API.Data;
using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Repository;

// REPOSITORY: GenericRepository
// PURPOSE: Provides generic CRUD operations for entities.
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly TimeTrackDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(TimeTrackDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // METHOD: GetByIdAsync
    // PURPOSE: Retrieves an entity by its ID.
    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    // METHOD: GetAllAsync
    // PURPOSE: Retrieves all entities.
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    // METHOD: FindAsync
    // PURPOSE: Finds entities matching a predicate.
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    // METHOD: FirstOrDefaultAsync
    // PURPOSE: Retrieves the first entity matching a predicate or null.
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    // METHOD: AddAsync
    // PURPOSE: Adds a new entity to the database.
    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    // METHOD: Update
    // PURPOSE: Updates an existing entity.
    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    // METHOD: Delete
    // PURPOSE: Deletes an entity from the database.
    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    // METHOD: ExistsAsync
    // PURPOSE: Checks if any entity matches a predicate.
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    // METHOD: CountAsync
    // PURPOSE: Returns count of entities matching a predicate.
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
    {
        if (predicate == null)
            return await _dbSet.CountAsync();
        
        return await _dbSet.CountAsync(predicate);
    }
}