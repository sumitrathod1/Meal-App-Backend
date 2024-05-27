
namespace MealApp.Repo
{
    public interface ICouponRepository
    {
        public string EncodeString(string input);

        public string DecodeString(string input);

        Task SaveCouponCodeAsync(int userid, string code);

    }
}
