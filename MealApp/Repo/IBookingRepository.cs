using MealApp.Models;

namespace MealApp.Repo
{
    public interface IBookingRepository
    {
        Booking Add(Booking booking);
       // Booking Find(int BookingId);
        Booking Update(Booking Booking);

        Task<bool> GetAquiredStatusDinner(int userid);
        Task<bool> GetAquiredStatusLunch(int userid);
        int GetDualBooking(int UserId, DateTime StartDate);
        User UpdateUser(User User);
        Booking CancelBooking(int BookingId);

        int FindDAccess(string email);

        int FindDCreddits(string email);
        List<DateTime> FindBookingDates(int UserId);

        Task<bool> IsBookedAsync(string email,DateTime selecteddate);
       


        int CountBookings(int id, DateTime today);
        Task<List<Booking>> ExistingBookingsAsync(int userId, Models.Type bookingType, DateTime startDate, DateTime endDate);

        Task<Booking> GetBookingStatusByUserIdAsync(int userId);
        Task ChangeBookingStatusAsync(int userId, DateTime today);
    }
}
