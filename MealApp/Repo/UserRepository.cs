using MealApp.Context;
using MealApp.Migrations;
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

        


        public int GetAllowedAccess(int UserId)
        {
            User user=context.Users.Find(UserId);
            return user.AllowedAccess;
        }

        public User Update(User user)
        {
            var User = context.Users.Attach(user);
            User.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return user;
        }
        public int UpdateAllowedAccess(int UserId,int bookeddays, int credits)
        {

            int allowedAccess;
            User user = context.Users.Find(UserId);

            if (bookeddays > 0)
            {
               allowedAccess = bookeddays + credits;
                user.AllowedAccess = allowedAccess;
            }

           else
            {
                allowedAccess = credits;
                user.AllowedAccess = allowedAccess;

            }




            Update(user);
            return allowedAccess;
        }

        public User UpdateCredits(int UserId, int delta)
        {
            User user = context.Users.Find(UserId);
            user.BookingDays += delta;
            user.Credits -= delta;
            User user1 = Update(user);
            return user1;
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
                 x.Date.Day == selecteddate.Day && x.Status== Status.Booked).FirstOrDefault();
          
           

            if (user == null)
            {
                return -1;
            }

            int bookingid = user.Id;
            return bookingid;

        }

        

    }
}
