using MealApp.Models;

namespace MealApp.Repo
{
    public interface IUserRepository
    {
        int GetAllowedBookings(int UserId);
        void UpdateAllowedBooking(int UserId, int delta);
        User Update(User user);

        int FindAccess(string email);
    }
}
