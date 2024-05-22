using MealApp.Context;
using MealApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MealApp.Repo
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext context;
        public UserRepository(AppDbContext context)
        {
            this.context = context;
        }

        


        public int GetAllowedBookings(int UserId)
        {
            User user=context.Users.Find(UserId);
            return user.AllowedBookings;
        }

        public User Update(User user)
        {
            var User = context.Users.Attach(user);
            User.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return user;
        }
        public void UpdateAllowedBooking(int UserId,int delta)
        {
            User user = context.Users.Find(UserId);
            user.AllowedBookings += delta;
            Update(user);
            return;
        }


        public int FindAccess(string email)
        {
            var user = context.Users.Where(u => u.Email == email).FirstOrDefault();

            if (user != null)
            {
                return user.AllowedBookings;
            }

            return 0;
        }



    }
}
