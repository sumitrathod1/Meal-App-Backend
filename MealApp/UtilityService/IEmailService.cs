using MealApp.Models;
using System.Security.Cryptography.X509Certificates;

namespace MealApp.UtilityService
{
    public interface IEmailService
    {
        void SendEmail(EmailModel emailModel);
    }
}
