using System.ComponentModel.DataAnnotations;

namespace MealApp.Models.Dto
{
    public class ChangePasswordDTO
    {

        [Required]
        public string Email { get; set; }

        [Required]
        public string OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
