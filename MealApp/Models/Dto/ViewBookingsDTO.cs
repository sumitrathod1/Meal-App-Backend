using System.ComponentModel.DataAnnotations;

namespace MealApp.Models.Dto
{
    public class ViewBookingsDTO
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public int UserId { get; set; }
    }
}
