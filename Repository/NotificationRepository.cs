using Microsoft.EntityFrameworkCore;
using TimeTrack.API.Data;
using TimeTrack.API.Models;
using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Repository;

// REPOSITORY: NotificationRepository
// PURPOSE: Handles database operations for Notification entities.
public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(TimeTrackDbContext context) : base(context)
    {
    }

    // METHOD: GetNotificationsByUserIdAsync
    // PURPOSE: Retrieves all notifications for a specific user.
    public async System.Threading.Tasks.Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    // METHOD: GetUnreadNotificationsAsync
    // PURPOSE: Retrieves all unread notifications for a specific user.
    public async System.Threading.Tasks.Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId)
    {
        return await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    // METHOD: GetUnreadCountAsync
    // PURPOSE: Returns count of unread notifications for a user.
    public async System.Threading.Tasks.Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _dbSet
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    // METHOD: MarkAsReadAsync
    // PURPOSE: Marks a notification as read.
    public async System.Threading.Tasks.Task MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _dbSet.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }
    }

    // METHOD: MarkAllAsReadAsync
    // PURPOSE: Marks all notifications as read for a user.
    public async System.Threading.Tasks.Task MarkAllAsReadAsync(Guid userId)
    {
        var unreadNotifications = await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }
    }
}