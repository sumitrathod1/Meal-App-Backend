using System.ComponentModel.DataAnnotations;

namespace MealApp.Models
{
    public enum Status
    {
        Booked,
        Cancelled,
        Used
    }

    public enum Type
    {
        Lunch,
        Dinner
    }
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }  
        public Status Status { get; set; }

        public Type Type { get; set; }

        public static implicit operator Booking(User v)
        {
            throw new NotImplementedException();
        }
    }
}
