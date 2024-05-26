using MealApp.Models;

namespace MealApp.Repo
{
    public interface IUserRepository
    {
        int GetAllowedAccess(int UserId);
        int UpdateAllowedAccess(int UserId, int bookedday , int credits);

        User UpdateCredits(int UserId, int delta);
        User Update(User user);

        
        int FindUserid(string email);
        int FindBookingid(int userid, DateTime selectedDate);
    }
}
