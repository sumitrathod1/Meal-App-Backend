using MealApp.Models;

namespace MealApp.Repo
{
    public interface IUserRepository
    {
        int GetAllowedAccess(int UserId);
        int UpdateAllowedAccess(int UserId, int bookedday , int credits);
        int GetDualBooking(int UserId, DateTime StartDate);

        User UpdateCredits(int UserId, int delta);
        User Update(User user);

        
        int FindUserid(string email);
        int FindBookingid(int userid, DateTime selectedDate, Models.Type BookingType);


        public Task AddContectusDataAsync(Contect contect);


        public Task<bool> IstodaysLunchAquired(int userid, DateTime today);

        public Task<bool> IstodaysDinnerAquired(int userid, DateTime today);

        public Task<bool> IsLunchFeedbackstored(int userid, DateTime today);

        public Task<bool> IsDinnerFeedbackstored(int userid, DateTime today);




    }
}
