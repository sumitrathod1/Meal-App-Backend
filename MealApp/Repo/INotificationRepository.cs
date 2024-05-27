using MealApp.Models;

namespace MealApp.Repo
{
    public interface INotificationRepository
    {

      Task<Notification> AddNotificationAsync(Notification notification);
        public List<Notification> FindNotification(int UserId);

        public Task DeleteNotificationsByUserIdAsync(int userId);

        Task<int> CountNotificationsByUserIdAsync(int userId);
    }
}
