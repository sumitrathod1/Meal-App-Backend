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

        public async Task<int> FindandDeleteNotificationByindex(int userid, int index)
        {
            var notification = context.notifications.FirstOrDefault();

            // Retrieve the notifications for the user, ordered by CreatedTime in ascending order
            var allnotificationsobj = await context.notifications.Where(n => n.UserId == userid)
                                              .OrderByDescending(n => n.CreatedAt).ToListAsync();




            // Check if the index is within the bounds of the list
            if (index < 0 || index >= allnotificationsobj.Count)
            {
                return -1;
            }

            // Get the notification at the specified index
            var notificationToDelete = allnotificationsobj[index];

            // Delete the notification
            context.notifications.Remove(notificationToDelete);

            // Save changes to the database
            await context.SaveChangesAsync();
            return 1;
        }


        public List<string> FindNotification(int UserId)
        {
            return context.notifications.Where(x => x.UserId == UserId).OrderByDescending(x => x.CreatedAt).Select(x=>x.Description).ToList();
        }

       
    }
}
