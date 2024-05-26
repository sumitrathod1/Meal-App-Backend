using MealApp.Context;
using MealApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MealApp.Repo
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext context;
        public BookingRepository(AppDbContext context)
        {
            this.context = context;
        }

        public Booking Add(Booking booking)
        {
            context.Bookings.Add(booking);
            context.SaveChanges();
            return booking;
        }

        public Booking CancelBooking(int BookingId)
        {
            Booking Booking= context.Bookings.Find(BookingId);
            Booking.Status =Status.Cancelled;
            Update(Booking);
            return Booking;
        }

        //public User UpdateBookingdaya(int BookingId)
        //{
        //    Booking Booking = context.Bookings.Find(BookingId);
        //    Booking.Status = Status.Cancelled;
        //    Update(Booking);
        //    return Booking;
        //}



        //this finds booking id based on user id and date /// and userid find by emailid
        //public Booking Find(int BookingId)
        //{ 
        //    return context.Bookings.Find(BookingId);
        //}


        public List<Booking> FindBookings(int UserId, DateTime StartDate, DateTime EndDate)
        {
            return context.Bookings.Where(booking=>booking.UserId == UserId && booking.Date >= StartDate && booking.Date <= EndDate).ToList();
        }

        //find list of existing bookings of specific user and specific Type lunch dinner and gives only booked dates
        public async Task<List<Booking>> ExistingBookingsAsync(int userId, Models.Type bookingType, DateTime startDate, DateTime endDate)
        {
            return await context.Bookings
                .Where(b => b.UserId == userId && b.Type == bookingType && b.Status == Status.Booked && b.Date >= startDate && b.Date <= endDate)
                .ToListAsync();
        }


        //gives booked days from today to allowedaccess
        public int CountBookings(int UserId, DateTime StartDate, DateTime EndDate)
        {

            return context.Bookings.Count(booking => booking.UserId == UserId && booking.Date >= StartDate && booking.Date <= EndDate && booking.Status== Status.Booked);
        }



        public Booking Update(Booking booking)
        {
            var Booking = context.Bookings.Attach(booking);
            Booking.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return booking;
        }

        public User UpdateUser(User usermodel)
        {
            var user = context.Users.Attach(usermodel);
            user.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
           
            context.SaveChanges();
            return usermodel;
        }

        public int FindDAccess(string email)
        {
            var user = context.Users.Where(u => u.Email == email).FirstOrDefault();
            int BookingDays = user.BookingDays;

            if(BookingDays > 0)
            {
                return BookingDays + user.Credits;
            }
            if (BookingDays == 0)
            {
                return user.Credits;
            }

            return -1;
        }

        //public int GetBookedDays(int userid, int access)//////////////////
        //{
            
        //    var Booking = context.Bookings.Where(x => x.UserId == userid && ).FirstOrDefault();

        //    int RequestedBookings = 0;

        //    for (DateTime CurrDate = Today; CurrDate <= EndDate; CurrDate = CurrDate.AddDays(1))
        //    {
        //        if (CurrDate.DayOfWeek != DayOfWeek.Saturday && CurrDate.DayOfWeek != DayOfWeek.Sunday)
        //        {
        //            RequestedBookings++;
        //        }
        //    }
            
        //    var user = context.Users.Where(u => u.Email == email).FirstOrDefault();
        //    int BookingDays = user.BookingDays;

        //    if (BookingDays > 0)
        //    {
        //        return BookingDays + user.Credits;
        //    }
        //    if (BookingDays == 0)
        //    {
        //        return user.Credits;
        //    }

        //    return -1;
        //}

        public int FindDCreddits(string email)
        {
            var user = context.Users.Where(u => u.Email == email).FirstOrDefault();

            if (user != null)
            {
                return user.Credits;
            }

            return -1;
        }
    }
}
