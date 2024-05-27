using System.ComponentModel.DataAnnotations;

namespace MealApp.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime expiredTime { get; set; } 

        public string CouponCode { get; set; }

    }
}
