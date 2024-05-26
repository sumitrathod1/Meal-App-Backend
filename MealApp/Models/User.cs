using System.ComponentModel.DataAnnotations;

namespace MealApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string? ResetPasswordToken { get; set; }
        public DateTime RestPAsswordExpiry { get; set; }

        public int AllowedAccess { get; set; } = 1;

        public int Credits { get; set; } = 0;

        public int BookingDays { get; set;} = 0;
    }
}
