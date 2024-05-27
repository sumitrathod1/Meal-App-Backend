using System.ComponentModel.DataAnnotations;

namespace MealApp.Models
{
    
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;


    }
}
