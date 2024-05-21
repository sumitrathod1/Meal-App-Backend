using MealApp.Models;

namespace MealApp.Repo
{
    public interface IBookingRepository
    {
        Booking Add(Booking booking);
        Booking Find(int BookingId);
        Booking Update(Booking Booking);
        Booking CancelBooking(int BookingId);
        List<Booking> FindBookings(int UserId, DateTime StartDate, DateTime EndDate);
        

    }
}
