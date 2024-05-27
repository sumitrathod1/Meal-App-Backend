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
       
    }
}
