using System.ComponentModel.DataAnnotations;

namespace MealApp.Models.Dto
{
    public class ViewBookingsDTO
    {
       
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
       
        public string Email { get; set; }
    }
}
