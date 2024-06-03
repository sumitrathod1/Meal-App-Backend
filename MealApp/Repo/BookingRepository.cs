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

        //it check that user have booked todays meal or not
        public async Task<bool> IsBookedAsync(string email, DateTime selecteddate)
        {
           // DateTime today = DateTime.Today; // Using DateTime.Today to get only the date part without the time component
            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
            {
                return false;
            }

            int userId = user.Id;
            var booking = await context.Bookings
                .FirstOrDefaultAsync(y => y.UserId == userId && y.Date.Date == selecteddate.Date && y.Status == Status.Booked); // Compare only the date part

            if (booking != null)
            {
                return true;                      //    Lunch or dinner booked for selected date
            }

            return false;
        }

       
        

        public async Task<bool> GetAquiredStatusLunch(int userid)
        {
           DateTime today = DateTime.Today;
            var booking = await context.Bookings
                .FirstOrDefaultAsync(y => y.UserId == userid && y.Date.Date == today.Date && y.Status == Status.Used); // Compare only the date part

 
            if (booking != null && booking.Type == Models.Type.Lunch)
            {
                return true;           // lunch aquired.
            }
            
            return false;
        }

        public async Task<bool> GetAquiredStatusDinner(int userid)
        {
            DateTime today = DateTime.Today;
            var booking = await context.Bookings
                .FirstOrDefaultAsync(y => y.UserId == userid && y.Date.Date == today.Date && y.Status == Status.Used); // Compare only the date part


            if (booking != null && booking.Type == Models.Type.Dinner)
            {
                return true;           // dinner aquired.
            }

            return false;
        }


        public List<DateTime> FindBookingDates(int userId)
        {
            var bookingDates = context.Bookings
                .Where(booking => booking.UserId == userId && (booking.Status == Status.Booked || booking.Status == Status.Used))
                .Select(booking => booking.Date) // Convert DateTime to DateOnly
                .ToList();

            return bookingDates;
        }




        //find list of existing bookings of specific user and specific Type lunch dinner and gives only booked dates
        public async Task<List<Booking>> ExistingBookingsAsync(int userId, Models.Type bookingType, DateTime startDate, DateTime endDate)
        {
            return await context.Bookings
                .Where(b => b.UserId == userId &&  b.Status == Status.Booked && b.Date >= startDate && b.Date <= endDate)
                .ToListAsync();
        }   // b.Type == bookingType => is not used because this method returns all booked rows


        //gives booked days from today 
        public int CountBookings(int UserId, DateTime StartDate)
        {

            return context.Bookings.Count(booking => booking.UserId == UserId && booking.Date >= StartDate && booking.Status== Status.Booked);
        }

        public int GetDualBooking(int UserId, DateTime StartDate)
        {
                                                                // returns the no. of days where lunch and dinner both booked
            var dualBookingDays = context.Bookings
                .Where(booking => booking.UserId == UserId
                               && booking.Date >= StartDate
                               && booking.Status == Status.Booked)
                .GroupBy(booking => new { booking.UserId, booking.Date })
                .Where(g => g.Select(b => b.Type).Distinct().Count() > 1)
                .Count();

            return dualBookingDays;
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

        public async Task ChangeBookingStatusAsync(int userId ,DateTime today)
        {
            
            var booking = await context.Bookings.FirstOrDefaultAsync(b => b.UserId == userId && (b.Date.Date == today.Date));
            if (booking != null)
            {
                booking.Status = Status.Used;
                await context.SaveChangesAsync();
            }
        }


        public async Task<Booking> GetBookingStatusByUserIdAsync(int userId)
        {
            return await context.Bookings.FirstOrDefaultAsync(x => x.UserId == userId);
        }


    }
}
