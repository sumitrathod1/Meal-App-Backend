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




        // POST: pageload/getallowedbookings
        [HttpPost("Allowedbookings")]
        public async Task<IActionResult> Allowedbooking([FromBody] AllowedBookingDTO allowedBookingDTO)
        {
            string email = allowedBookingDTO.Email;

            int allowedBookings = UserRepository.FindAccess(email);  //method used from Userrepo 

            if (allowedBookings >= 0)
            {
                return Ok(new { AllowedBookings = allowedBookings });
            }

            return NotFound(new { Message = "User not found or no allowed bookings" });
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



        // put Booking/CancelBooking/email,date
        // here id is booking id and it will come from userid and date 
       
        [HttpPut("CancelBooking")]
        public async Task<IActionResult> CancelBooking([FromBody] CancelBookingDTO cancelDTO)
        {
            DateTime SelectedDate = cancelDTO.selecteddate;
            string Email = cancelDTO.Email;

            //this will find user id based on emailid

            int userid = UserRepository.FindUserid(Email);

            if (userid == -1)
            {
                NotFound(new { message = "User Not Found!" });
            }

            int bookingid = UserRepository.FindBookingid(userid, SelectedDate);

            if (bookingid == -1)
            {
                NotFound(new { message = "booking is not for this day" });
            }

            Booking CancelledBooking=BookingRepository.CancelBooking(bookingid);

            UserRepository.UpdateAllowedBooking(userid, +1);
            return Ok(CancelledBooking);
        }

       


            // DELETE api/<Booking>/5
            [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
