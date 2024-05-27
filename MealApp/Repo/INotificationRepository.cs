using MealApp.Models;

namespace MealApp.Repo
{
    public interface INotificationRepository
    {

      Task<Notification> AddNotificationAsync(Notification notification);
    }
}
