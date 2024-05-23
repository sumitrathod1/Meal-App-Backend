using System.ComponentModel.DataAnnotations;

namespace MealApp.Models.Dto
{
    public class AllowedBookingDTO
    {
        [Required]
        public string Email { get; set; }

    }
}
