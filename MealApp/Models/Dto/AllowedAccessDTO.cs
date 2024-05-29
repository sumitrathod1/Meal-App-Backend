using System.ComponentModel.DataAnnotations;

namespace MealApp.Models.Dto
{
    public class AllowedAccessDTO
    {
        [Required]
        public string Email { get; set; }

        public DateTime SelectedDate { get; set; }

    }
}
