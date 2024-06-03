using MealApp.Context;
using MealApp.Migrations;
using MealApp.Models;
using Microsoft.EntityFrameworkCore;
using System;

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

        public int GetDualBooking(int UserId, DateTime StartDate)
        {
            // returns the no. of days where lunch and dinner both booked
            var dualBookingDays = context.Bookings
                .Where(booking => booking.UserId == UserId
                               && booking.Date.Date >= StartDate.Date
                               && booking.Status == Status.Booked)
                .GroupBy(booking => new { booking.Date.Date })
                .Where(g => g.Select(b => b.Type).Distinct().Count() > 1)
                .Count();

            return dualBookingDays;
        }

        public int UpdateAllowedAccess(int UserId,int bookeddays, int credits)
        {
            DateTime Today = DateTime.Now;
            int dualbookingdays = GetDualBooking(UserId, Today);   // no. of days where both booked

            int AllowedAccess;
            User user = context.Users.Find(UserId);
            AllowedAccess = user.AllowedAccess;         // fetch old allowedaccess

            if(dualbookingdays > 0)
            {
                if (bookeddays > 0)
                {
                    int allowedAccess = bookeddays + credits - dualbookingdays;
                    AllowedAccess = allowedAccess;
                }

             
            }
            else
            {
                if (bookeddays > 0)
                {
                   int allowedAccess = bookeddays + credits;
                    AllowedAccess = allowedAccess;
                }

                else
                {
                    int allowedAccess = credits;
                    AllowedAccess = allowedAccess;

                }

            }


            user.AllowedAccess = AllowedAccess;
            Update(user);
            return AllowedAccess;
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

        public int FindBookingid(int userid, DateTime selecteddate, Models.Type BookingType)
        {

          //  DateOnly selectedDateOnly = DateOnly.FromDateTime(selecteddate);

            var user = context.Bookings.Where(x => x.UserId == userid &&
                 x.Date.Year == selecteddate.Year &&
                 x.Date.Month == selecteddate.Month &&
                 x.Date.Day == selecteddate.Day && x.Status== Status.Booked && x.Type == BookingType).FirstOrDefault();
          
           
            if (user == null)
            {
                return -1;
            }

            int bookingid = user.Id;
            return bookingid;

        }

        public async Task AddContectusDataAsync(Contect contect)
        {
            context.contects.Add(contect);
            await context.SaveChangesAsync();
           
        }


        public async Task<bool> IstodaysLunchAquired(int userid, DateTime today)
        {
            var user = context.Bookings.Where(x => x.UserId == userid &&
                x.Date.Year == today.Year &&
                x.Date.Month == today.Month &&
                x.Date.Day == today.Day && x.Status == Status.Used && x.Type == Models.Type.Lunch).FirstOrDefault();
           
            if(user == null)
            { 
                return false;
            }

            return true;
        }
        

        public async Task<bool> IstodaysDinnerAquired(int userid, DateTime today)
        {
            var user = context.Bookings.Where(x => x.UserId == userid &&
                x.Date.Year == today.Year &&
                x.Date.Month == today.Month &&
                x.Date.Day == today.Day && x.Status == Status.Used && x.Type == Models.Type.Dinner).FirstOrDefault();

            if (user == null)
            {
                return false;
            }

            return true;
        }

        

       public async Task<bool> IsLunchFeedbackstored(int userid, DateTime today)
        {
            var feedback = context.feedbacks.Where(x => x.UserId == userid &&
                x.FeedbackTimeStamp.Year == today.Year &&
                x.FeedbackTimeStamp.Month == today.Month &&
                x.FeedbackTimeStamp.Day == today.Day && x.MealType == "Lunch").FirstOrDefault();

            if (feedback == null)
            {
                return false;
            }

            return true;
        }

        
        public async Task<bool> IsDinnerFeedbackstored(int userid, DateTime today)
        {
            var feedback = context.feedbacks.Where(x => x.UserId == userid &&
                x.FeedbackTimeStamp.Year == today.Year &&
                x.FeedbackTimeStamp.Month == today.Month &&
                x.FeedbackTimeStamp.Day == today.Day && x.MealType == "Lunch").FirstOrDefault();

            if (feedback == null)
            {
                return false;
            }

            return true;
        }

    }
}
