using TimeTrack.API.Models;
using TimeTrack.API.Repository.IRepository;
using TaskAsync = System.Threading.Tasks.Task;

namespace TimeTrack.API.Service;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async TaskAsync CreateNotificationAsync(Guid userId, string type, string message)
    {
        // ✅ Changed to use new Notification model
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();
    }

    // ✅ Changed int to Guid
    public async System.Threading.Tasks.Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
    {
        return await _unitOfWork.Notifications.GetNotificationsByUserIdAsync(userId);
    }

    // ✅ Changed int to Guid
    public async System.Threading.Tasks.Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId)
    {
        return await _unitOfWork.Notifications.GetUnreadNotificationsAsync(userId);
    }

    // ✅ Changed int to Guid
    public async System.Threading.Tasks.Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
    }

    // ✅ Changed int to Guid
    public async TaskAsync MarkAsReadAsync(Guid notificationId)
    {
        await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
        await _unitOfWork.SaveChangesAsync();
    }

    // ✅ Changed int to Guid
    public async TaskAsync MarkAllAsReadAsync(Guid userId)
    {
        await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    // ✅ Changed int to Guid
    public async TaskAsync SendTaskAssignmentNotificationAsync(Guid userId, string taskTitle)
    {
        var message = $"New task assigned: '{taskTitle}'. Please review and start working on it.";
        await CreateNotificationAsync(userId, "TaskAssigned", message);
    }

    // ✅ Changed int to Guid
    public async TaskAsync SendLogReminderNotificationAsync(Guid userId)
    {
        var message = "Reminder: Please log your work hours for today.";
        await CreateNotificationAsync(userId, "LogReminder", message);
    }

    // ✅ Changed int to Guid
    public async TaskAsync SendTaskDeadlineNotificationAsync(Guid userId, string taskTitle, DateTime dueDate)
    {
        var daysRemaining = (dueDate.Date - DateTime.UtcNow.Date).Days;
        var urgency = daysRemaining <= 1 ? "urgent" : $"due in {daysRemaining} days";
        var message = $"Task '{taskTitle}' is {urgency}. Please complete it by {dueDate:MMM dd, yyyy}.";

        await CreateNotificationAsync(userId, "TaskDeadline", message);
    }
}
