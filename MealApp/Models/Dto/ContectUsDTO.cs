using System.ComponentModel.DataAnnotations;

namespace MealApp.Models.Dto
{
    public class ContectUsDTO
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }
        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

    }
}
