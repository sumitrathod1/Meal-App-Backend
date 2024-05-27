using MealApp.Context;
using MealApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MealApp.Repo
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext context;

        public NotificationRepository(AppDbContext context) 
        { 
            
            this.context = context;
        }



        //public void RemoveNotification(int )
        //{
        //    context.notifications.Remove(notification);
        //    context.SaveChanges();

        //}

        public async Task <Notification> AddNotificationAsync(Notification notification)
        {
            context.notifications.Add(notification);
            await context.SaveChangesAsync();
            return notification;
        }

        public async Task<int> CountNotificationsByUserIdAsync(int userId)
        {
            return await context.notifications.CountAsync(n => n.UserId == userId);
        }
        public async Task DeleteNotificationsByUserIdAsync(int userId)
        {
            var notifications = context.notifications.Where(n => n.UserId == userId);
            context.notifications.RemoveRange(notifications);
            await context.SaveChangesAsync();
        }

        public List<Notification> FindNotification(int UserId)
        {
            return context.notifications.Where(x => x.UserId == UserId).ToList();
        }

    }
}
