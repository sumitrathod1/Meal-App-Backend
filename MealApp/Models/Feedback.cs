using System.ComponentModel.DataAnnotations;

namespace MealApp.Models
{
    public class Feedback
    {
        [Key]
        public int id { get; set; }

        public int UserId { get; set; }

        public DateTime FeedbackTimeStamp { get; set; }
        public string FirstName { get; set; }

        public string Message { get; set; }

        public int Rating { get; set; }

        public string MealType { get; set; }
    }
}
