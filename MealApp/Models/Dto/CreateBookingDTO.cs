using System.ComponentModel.DataAnnotations;

namespace MealApp.Models.Dto
{
    public class CreateBookingDTO
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        
        public int ? UserId { get; set; }

        [Required]
        public string Email { get; set; }


        [Required]
        public string Type { get; set; }
    }
}
