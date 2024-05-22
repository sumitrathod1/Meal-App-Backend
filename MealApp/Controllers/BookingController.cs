using MealApp.Context;
using MealApp.Models;
using MealApp.Models.Dto;
using MealApp.Repo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Type = MealApp.Models.Type;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MealApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _authContext;
        private readonly IUserRepository UserRepository;
        private readonly IBookingRepository BookingRepository;
        private readonly int id;

        public BookingController(IUserRepository userRepository, IBookingRepository bookingRepository, AppDbContext appDbContext)
        {
            _authContext = appDbContext;
            UserRepository = userRepository;
            BookingRepository = bookingRepository;
        }

        // GET: Bookings/ViewBookings
        [HttpGet("ViewBookings")]
        public async Task<IActionResult> ViewBookings(ViewBookingsDTO bookingsDTO)
        {
            DateTime StartDate = bookingsDTO.StartDate;
            DateTime EndDate = bookingsDTO.EndDate;
            int UserId = bookingsDTO.UserId;
            return Ok(BookingRepository.FindBookings(UserId, StartDate, EndDate));
        }




        // GET: pageload/getallowedbookings
        [HttpGet("Allowedbookings")]
        public async Task<IActionResult> FindAccess([FromQuery] string email)
        {
            return Ok(UserRepository.FindAccess(email));
        }

        // GET api/<Booking>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<Booking>
        [HttpPost("CreateBooking")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDTO bookingDTO)
        {
            DateTime StartDate = bookingDTO.StartDate;
            DateTime EndDate= bookingDTO.EndDate;
            DateTime Today= DateTime.Now;
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == bookingDTO.Email);
            if (user == null)
            {
                if (user == null)
                    return NotFound(new { message = "User Not Found!" });
            }
            Type BookingType = Type.Lunch;
            if(bookingDTO.Type == "Lunch")
            {
                BookingType= Type.Lunch;
            }else if(bookingDTO.Type == "Dinner")
            {
                BookingType = Type.Dinner;
            }
            int UserId = user.Id; 
            int AllowedBookings = UserRepository.GetAllowedBookings(UserId);
            int RequestedBookings = 0;
            for (DateTime CurrDate = StartDate; CurrDate <= EndDate; CurrDate= CurrDate.AddDays(1))
            {
                if (CurrDate.DayOfWeek != DayOfWeek.Saturday && CurrDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    RequestedBookings++;
                }
            }
            if (RequestedBookings > AllowedBookings) {
                return BadRequest(new { Message = "RequestedBooking > AllowedBookings" });
            }
            if(Today.Subtract(StartDate).Days > 0){
                return BadRequest(new { Message = "StartDate must not be a past date" });
            }
            List<Booking> SavedBookings=new List<Booking>();
            for(DateTime CurrDate = StartDate; CurrDate <= EndDate; CurrDate = CurrDate.AddDays(1))
            {
                if(CurrDate.DayOfWeek != DayOfWeek.Saturday && CurrDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    Booking CurrBooking = new Booking
                    {
                        UserId=UserId,
                        Date=CurrDate,
                        Type=BookingType,
                        Status=Status.Booked
                    };
                    Booking SavedBooking=BookingRepository.Add(CurrBooking);
                    SavedBookings.Add(SavedBooking);
                }
                
            }
            UserRepository.UpdateAllowedBooking(UserId, -RequestedBookings);
            return Ok(SavedBookings);
        }



        // PUT Booking/CancelBooking/5
        // here id is booking id and it will come from userid and date 
        [HttpPut("CancelBooking/{id}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            Booking Booking = BookingRepository.Find(id);
           int UserId=Booking.UserId;
            Booking CancelledBooking=BookingRepository.CancelBooking(id);
            UserRepository.UpdateAllowedBooking(UserId, +1);
            return Ok(CancelledBooking);
        }

       


            // DELETE api/<Booking>/5
            [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
