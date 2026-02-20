using TimeTrack.API.Models;
using System.Threading.Tasks;

namespace TimeTrack.API.Repository.IRepository;

public interface INotificationRepository : IGenericRepository<Notification>
{
    System.Threading.Tasks.Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(Guid userId);
    System.Threading.Tasks.Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId);
    System.Threading.Tasks.Task<int> GetUnreadCountAsync(Guid userId);
    System.Threading.Tasks.Task MarkAsReadAsync(Guid notificationId);
    System.Threading.Tasks.Task MarkAllAsReadAsync(Guid userId);
}