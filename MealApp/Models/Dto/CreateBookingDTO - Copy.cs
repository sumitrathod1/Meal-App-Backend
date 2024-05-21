using System.ComponentModel.DataAnnotations;

namespace MealApp.Models.Dto
{
    public class CreateBookingDTO
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public String Type { get; set; }
    }
}
