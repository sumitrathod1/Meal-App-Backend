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

            return -1;
        }


        public int FindUserid(string email)
        {
            var user =  context.Users.Where(x => x.Email == email).FirstOrDefault();
            if (user == null)
            {
                return -1;
            }
           
           
            return user.Id;

        }

        public int FindBookingid(int userid, DateTime selecteddate )
        {

          //  DateOnly selectedDateOnly = DateOnly.FromDateTime(selecteddate);

            var user = context.Bookings.Where(x => x.UserId == userid &&
                 x.Date.Year == selecteddate.Year &&
                 x.Date.Month == selecteddate.Month &&
                 x.Date.Day == selecteddate.Day).FirstOrDefault();
          
           

            if (user == null)
            {
                return -1;
            }

            int bookingid = user.Id;
            return bookingid;

        }

    }
}
