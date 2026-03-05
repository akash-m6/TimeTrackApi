using Microsoft.EntityFrameworkCore.Storage;
using TimeTrack.API.Data;
using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Repository;

// REPOSITORY: UnitOfWork
// PURPOSE: Coordinates database operations and transactions across repositories.
public class UnitOfWork : IUnitOfWork
{
    private readonly TimeTrackDbContext _context;
    private IDbContextTransaction? _transaction;

    public IUserRepository Users { get; }
    public ITimeLogRepository TimeLogs { get; }
    public ITaskRepository Tasks { get; }
    // expose context when needed for certain queries
    public TimeTrack.API.Data.TimeTrackDbContext Context => _context;
    public ITaskTimeRepository TaskTimes { get; }
    public INotificationRepository Notifications { get; }
    public IPendingRegistrationRepository PendingRegistrations { get; }
    public IBreakRepository Breaks { get; }

    public UnitOfWork(TimeTrackDbContext context)
    {
        _context = context;
        Users = new UserRepository(_context);
        TimeLogs = new TimeLogRepository(_context);
        Tasks = new TaskRepository(_context);
        TaskTimes = new TaskTimeRepository(_context);
        Notifications = new NotificationRepository(_context);
        PendingRegistrations = new PendingRegistrationRepository(_context);
        Breaks = new BreakRepository(_context);
    }

    // METHOD: SaveChangesAsync
    // PURPOSE: Saves all changes to the database.
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    // METHOD: BeginTransactionAsync
    // PURPOSE: Begins a new database transaction.
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    // METHOD: CommitTransactionAsync
    // PURPOSE: Commits the current database transaction.
    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            await _transaction!.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    // METHOD: RollbackTransactionAsync
    // PURPOSE: Rolls back the current database transaction.
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    // METHOD: Dispose
    // PURPOSE: Disposes the database context and transaction.
    public void Dispose()
    {
        _transaction?.Dispose();
        _context?.Dispose();
    }
}