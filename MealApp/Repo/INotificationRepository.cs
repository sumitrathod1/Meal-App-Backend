using MealApp.Models;

namespace MealApp.Repo
{
    public interface INotificationRepository
    {

      Task<Notification> AddNotificationAsync(Notification notification);
        public List<string> FindNotification(int UserId);

        public Task DeleteNotificationsByUserIdAsync(int userId);

        public Task<int> FindandDeleteNotificationByindex(int userid, int index);

        Task<int> CountNotificationsByUserIdAsync(int userId);
    }
}
