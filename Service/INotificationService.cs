using TimeTrack.API.Models;

namespace TimeTrack.API.Service;

public interface INotificationService
{
    System.Threading.Tasks.Task CreateNotificationAsync(Guid userId, string type, string message);
    
    // ✅ Changed NotificationEntity to Notification
    System.Threading.Tasks.Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);
    System.Threading.Tasks.Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId);
    System.Threading.Tasks.Task<int> GetUnreadCountAsync(Guid userId);
    System.Threading.Tasks.Task MarkAsReadAsync(Guid notificationId);
    System.Threading.Tasks.Task MarkAllAsReadAsync(Guid userId);
    System.Threading.Tasks.Task SendTaskAssignmentNotificationAsync(Guid userId, string taskTitle);
    System.Threading.Tasks.Task SendLogReminderNotificationAsync(Guid userId);
    System.Threading.Tasks.Task SendTaskDeadlineNotificationAsync(Guid userId, string taskTitle, DateTime dueDate);
}