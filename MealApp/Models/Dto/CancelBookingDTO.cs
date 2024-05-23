using System.ComponentModel.DataAnnotations;

namespace MealApp.Models.Dto
{
    public class CancelBookingDTO
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public DateTime selecteddate { get; set; }

    }
}
