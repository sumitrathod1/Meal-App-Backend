using MealApp.Context;
using MealApp.Models;

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
            Booking Booking=Find(BookingId);
            Booking.Status = Status.Cancelled;
            return Update(Booking);
        }

        public Booking Find(int BookingId)
        { return context.Bookings.Find(BookingId); }

        public List<Booking> FindBookings(int UserId, DateTime StartDate, DateTime EndDate)
        {
            return context.Bookings.Where(booking=>booking.UserId == UserId && booking.Date >= StartDate && booking.Date <= EndDate).ToList();
        }

       

        public Booking Update(Booking booking)
        {
            var Booking = context.Bookings.Attach(booking);
            Booking.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return booking;
        }
    }
}
