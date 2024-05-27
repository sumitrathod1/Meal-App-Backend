using MealApp.Models;

namespace MealApp.Repo
{
    public interface IEmailRepository
    {
        void SendEmail(EmailModel emailModel);
    }
}
