using System.ComponentModel.DataAnnotations;

namespace MealApp.Models
{
    public class Contect
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string FullName { get; set; }
            
        public string Email { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
            
            


    }
}
