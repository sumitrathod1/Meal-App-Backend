using MealApp.Models;

namespace MealApp.Repo
{
    public interface IBookingRepository
    {
        Booking Add(Booking booking);
       // Booking Find(int BookingId);
        Booking Update(Booking Booking);

        User UpdateUser(User User);
        Booking CancelBooking(int BookingId);

        int FindDAccess(string email);

        int FindDCreddits(string email);
        List<Booking> FindBookings(int UserId, DateTime StartDate, DateTime EndDate);
        int CountBookings(int id, DateTime today, DateTime enddate);
    }
}
