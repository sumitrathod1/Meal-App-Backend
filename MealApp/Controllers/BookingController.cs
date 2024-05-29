using MealApp.Context;
using MealApp.Migrations;
using MealApp.Models;
using MealApp.Models.Dto;
using MealApp.Repo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Type = MealApp.Models.Type;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MealApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _authContext;
        private readonly IConfiguration _configration;
        private readonly IUserRepository UserRepository;
        private readonly IBookingRepository BookingRepository;
        private readonly IEmailRepository _emailRepository;
        private readonly INotificationRepository _NotificationRepository;
        private readonly NotificationRepository notificationRepository;
        private readonly int id;


        public BookingController(IUserRepository userRepository,INotificationRepository notificationRepository, IConfiguration configuration, IEmailRepository EmailRepository, IBookingRepository bookingRepository, AppDbContext appDbContext)
        {
            _authContext = appDbContext;
            _configration = configuration;
            _emailRepository = EmailRepository;
            UserRepository = userRepository;
            BookingRepository = bookingRepository;
            _NotificationRepository = notificationRepository;
        }

        // Post: Bookings/ViewBookings / load on view booking button
        [HttpPost("ViewBookings")]
        public async Task<IActionResult> ViewBookings(ViewBookingsDTO bookingsDTO)
        {
            string email = bookingsDTO.Email;
             var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            int userid = user.Id;
  

          
            return Ok(BookingRepository.FindBookingDates(userid));
        }




        // POST: pageload/getallowedaccess
        [HttpPost("Allowedaccess")]
        public async Task<IActionResult> Allowedaccess([FromBody] AllowedAccessDTO allowedAccessDTO)
        {
            string email = allowedAccessDTO.Email;
            //DateTime tomorrow = DateTime.Now.AddDays(1);
            DateTime Today = DateTime.Now;
           // DateTime Today = DateTime.Now.AddDays(6);

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            int allowedAccess = user.AllowedAccess;
                                                             // used to find no. of weekend days in between today and till access days
                                                               // which will be add to calculate final end date
                                                               // to provide proper access duration
           
            int i = allowedAccess;
            DateTime CurrDate = Today;
            while (0 < i)
            {
                if (CurrDate.DayOfWeek == DayOfWeek.Saturday || CurrDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    i++;
                    CurrDate = CurrDate.AddDays(1);
                }


                CurrDate = CurrDate.AddDays(1);
                i--;
            }


            DateTime enddate = CurrDate;// add weekends in end_date

            int credits = BookingRepository.FindDCreddits(email);  //method used from bookingrepo which return credits
            int bookeddays = BookingRepository.CountBookings(user.Id, Today, enddate);  //count booked days from today to access day
                                                                                       
            user.BookingDays = bookeddays;
            UserRepository.Update(user);              // strored booked days

            int allowedAccessnew = UserRepository.UpdateAllowedAccess(user.Id, bookeddays, credits);
            // this will return and update in database allowedaccess
         
            if (allowedAccessnew < 3 && allowedAccessnew >0)
            {               
                    //send notification to reminde about low access
                    var notification = new Notification
                    {
                        UserId = user.Id,
                        Description = "Reminder: " + $"Only {allowedAccessnew} coupons are left.",
                        CreatedAt = DateTime.Now
                    };
                    await _NotificationRepository.AddNotificationAsync(notification);  
                
                // send email


            }

            if (allowedAccessnew > 0)
            {
                return Ok(new { allowAccess = allowedAccessnew });         // from here only allowedaccess is passed
                                                                           // and frontend will enable that no. of days from 
                                                                           // today from frontend
            }

            if(allowedAccess == 0)
            {
                var notification = new Notification
                {
                    UserId = user.Id,
                    Description = "Expired: " + "Please renew the Meal.",
                    CreatedAt = DateTime.Now
                };
               await _NotificationRepository.AddNotificationAsync(notification);
                return Ok(new { allowAccess = allowedAccessnew });

            }



            return NotFound(new { Message = "User not found or no allowed access" });
        }



        // POST: pageload/getcredits and selected date booking status
        [HttpPost("Credits_and_bookingstatus")]
        public async Task<IActionResult> Credits([FromBody] AllowedAccessDTO allowedAccessDTO)
        {
            string email = allowedAccessDTO.Email;
            DateTime selecteddate = allowedAccessDTO.SelectedDate;

            int credits = BookingRepository.FindDCreddits(email);  //method used from bookingrepo which return credits
            Boolean isbooked = await BookingRepository.IsBookedAsync(email, selecteddate); // methos used fro checking user's today's booking


            if (credits >= 0)
            {
                return Ok(new { credit = credits , todays_booking = isbooked});
            }

           
            return NotFound(new { Message = "User not found or no allowed credits" });
        }




        // POST api/<Booking>
        [HttpPost("CreateBooking")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDTO bookingDTO)
        {
            DateTime StartDate = bookingDTO.StartDate;
            DateTime EndDate= bookingDTO.EndDate;
            DateTime Today= DateTime.Now;

            if(StartDate > EndDate)
            {
                return BadRequest(new { Message = "Select proper Start Date and End date" });

            }

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
           // int bookeddays = user.BookingDays;

            int Credits = user.Credits;
            // int AllowedAccess = user.AllowedAccess;

            // Check for overlapping bookings
            var existingBookings = await BookingRepository.ExistingBookingsAsync(UserId, BookingType, StartDate, EndDate);

            List<DateTime> conflictingDates = new List<DateTime>();
            for (DateTime CurrDate = StartDate; CurrDate <= EndDate; CurrDate = CurrDate.AddDays(1))
            {
                if (existingBookings.Any(b => b.Date == CurrDate))
                {
                    conflictingDates.Add(CurrDate);
                }
            }

            if (conflictingDates.Any())
            {
                string conflictDatesMessage = string.Join(", ", conflictingDates.Select(d => d.ToString("yyyy-MM-dd")));
                return BadRequest(new { Message = $"Booking for the following dates is already booked: {conflictDatesMessage}" });
            }



            int RequestedBookings = 0;
            for (DateTime CurrDate = StartDate; CurrDate <= EndDate; CurrDate= CurrDate.AddDays(1))
            {
                if (CurrDate.DayOfWeek != DayOfWeek.Saturday && CurrDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    RequestedBookings++;
                }

            }
            if (RequestedBookings > Credits) {
                return BadRequest(new { Message = "RequestedBooking > Credits" });
            }
            if(Today.Subtract(StartDate).Days > 0){
                return BadRequest(new { Message = "StartDate must not be a past date" });
            }
            //// Bookingdays = RequestedBookings
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

            // Save bookings before updating user data
            await _authContext.SaveChangesAsync();

            User user1 = UserRepository.UpdateCredits(UserId, RequestedBookings);

          
            int credits = user1.Credits; //    
            int bookedday = user1.BookingDays;
            UserRepository.UpdateAllowedAccess(UserId, bookedday, credits);

            if(StartDate.DayOfWeek == DayOfWeek.Saturday) { StartDate = StartDate.AddDays(2); }
           
            if(StartDate.DayOfWeek == DayOfWeek.Sunday) { StartDate = StartDate.AddDays(1); }

            if(EndDate.DayOfWeek == DayOfWeek.Saturday) { EndDate = EndDate.AddDays(2); }

            if(EndDate.DayOfWeek == DayOfWeek.Sunday) { EndDate = EndDate.AddDays(1); }

            // Create and save notification
            var notification = new Notification
            {
                UserId = UserId,
                
                Description = $"Your meal has been booked from {StartDate.ToString("MMMM dd, yyyy")} to {EndDate.ToString("MMMM dd, yyyy")}.",
                CreatedAt = DateTime.Now
            };
            await _NotificationRepository.AddNotificationAsync(notification);

            // Send confirmation email
            string from = _configration["emailsettings:from"];
            string subject = "Booking Confirmation";
            string body = $"Hello {user.FirstName},\n\nYour booking from {StartDate.ToString("MMMM dd, yyyy")} to {EndDate.ToString("MMMM dd, yyyy")} has been successfully created.\n\nBest regards,\nYour Rishabh Software";
            var emailModel = new EmailModel(user.Email, subject, body);
            _emailRepository.SendEmail(emailModel);

            // Save changes to the context after sending the email
            await _authContext.SaveChangesAsync();

            return Ok(SavedBookings);
        }



        //post for Quick Booking
        [HttpPost("CreateBookingForTomorrow")]
        public async Task<IActionResult> CreateBookingForTomorrow([FromBody] QuickBookingDTO qbookingDTO)
        {
            // DateTime today1 = DateTime.Now;
            //  DateTime today = today1.AddDays(2);

            DateTime today = DateTime.Now;
            DateTime bookingDate = today.AddDays(1);

            // If bookingDate is Saturday, add 2 days to make it Monday
            if (bookingDate.DayOfWeek == DayOfWeek.Saturday)
            {
                bookingDate = bookingDate.AddDays(2);
            }
            // If bookingDate is Sunday, add 1 day to make it Monday
            else if (bookingDate.DayOfWeek == DayOfWeek.Sunday)
            {
                bookingDate = bookingDate.AddDays(1);
            }

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == qbookingDTO.Email);
            if (user == null)
            {
                return NotFound(new { message = "User Not Found!" });
            }

            Type bookingType = Type.Lunch; // Default booking type
            int userId = user.Id;
            int credits = user.Credits;

            // Check for existing booking on the target date
            var existingBooking = await _authContext.Bookings.FirstOrDefaultAsync(x=>x.UserId == userId && x.Status == Status.Booked && x.Type== Type.Lunch && x.Date.Date == bookingDate.Date);
             if(existingBooking != null)
            {
                return BadRequest(new { Message = $"Booking for {bookingDate.ToString("yyyy-MM-dd")} is already booked." });

            }





            if (credits < 1)
            {
                return BadRequest(new { Message = "Not enough credits for the booking." });
            }

            Booking newBooking = new Booking
            {
                UserId = userId,
                Date = bookingDate,
                Type = bookingType,
                Status = Status.Booked
            };
            Booking savedBooking = BookingRepository.Add(newBooking);

            // Save booking before updating user data
            await _authContext.SaveChangesAsync();

            User user1 = UserRepository.UpdateCredits(userId,1);


              int Credits = user1.Credits; //    
            int bookedday = user1.BookingDays;
            UserRepository.UpdateAllowedAccess(userId, bookedday, Credits);

            // Create and save notification
            var notification = new Notification
            {
                UserId = userId,
                Description = $"Your meal has been booked for {bookingDate.ToString("MMMM dd, yyyy")}.",
                CreatedAt = DateTime.Now
            };
            await _NotificationRepository.AddNotificationAsync(notification);

            ////// send confirmation email
            string from = _configration["emailsettings:from"];
            string subject = "Booking Confirmation";

            string body = $"hello {user.FirstName},\n\nyour booking for {bookingDate.ToString("mmmm dd, yyyy")} has been successfully created.\n\nbest regards,\nyour rishabh software"; var emailModel = new EmailModel(user.Email, subject, body);
            _emailRepository.SendEmail(emailModel);

            // Save changes to the context after sending the email
            await _authContext.SaveChangesAsync();

            return Ok(savedBooking);
        }


        // put Booking/CancelBooking/email,date
        // here id is booking id and it will come from userid and date 

        //[HttpPut("CancelBooking")]
        //public async Task<IActionResult> CancelBooking([FromBody] CancelBookingDTO cancelDTO)
        //{
        //    DateTime SelectedDate = cancelDTO.selecteddate;
        //    string Email = cancelDTO.Email;

        //    //this will find user id based on emailid

        //    int userid = UserRepository.FindUserid(Email);

        //    if (userid == -1)
        //    {
        //        NotFound(new { message = "User Not Found!" });
        //    }

        //    int bookingid = UserRepository.FindBookingid(userid, SelectedDate);

        //    if (bookingid == -1)
        //    {
        //        NotFound(new { message = "booking is not for this day" });
        //    }

        //    Booking CancelledBooking=BookingRepository.CancelBooking(bookingid);

        //    UserRepository.UpdateAllowedBooking(userid, +1);

        //    return Ok(CancelledBooking);
        //}
        [HttpPut("CancelBooking")]
        public async Task<IActionResult> CancelBooking([FromBody] CancelBookingDTO cancelDTO)
        {
            DateTime SelectedDate = cancelDTO.selecteddate;
            string Email = cancelDTO.Email;
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == Email);

            //this will find user id based on emailid
            int userid = UserRepository.FindUserid(Email);

            if (userid == -1)
            {
                return NotFound(new { message = "User Not Found!" });
            }

            int bookingid = UserRepository.FindBookingid(userid, SelectedDate);

            if (bookingid == -1)
            {
                return NotFound(new { message = "Booking is not for this day" });
            }

            Booking CancelledBooking = BookingRepository.CancelBooking(bookingid);

            //
            if (CancelledBooking != null)
            {

                UserRepository.UpdateCredits(userid, -1);

                int credits = user.Credits; //         because i think UpdateCredits will update it - RequestedBookings;
                int bookeddays = user.BookingDays;
                UserRepository.UpdateAllowedAccess(userid, bookeddays, credits);

               
                // Send cancellation email
                string from = _configration["emailsettings:from"];
                string subject = "Booking Cancellation Confirmation";
                string body = $"Hello {user.FirstName},\n\nYour booking for {SelectedDate.ToString("MMMM dd, yyyy")} has been successfully canceled.\n\nBest regards,\nYour Rishabh Software";
                var emailModel = new EmailModel(Email, subject, body);
                _emailRepository.SendEmail(emailModel);


                // Create and save notification
                var notification = new Notification
                {
                    UserId = userid,
                    Description = "MealCancelled: " + $"Your meal has been Cancelled for {SelectedDate.ToString("MMMM dd, yyyy")}.",
                    CreatedAt = DateTime.Now
                };
                await _NotificationRepository.AddNotificationAsync(notification);

                await _authContext.SaveChangesAsync();

                return Ok(new { message = "Booking canceled successfully and email sent!" });
            }

            return StatusCode(500, new { message = "Error occurred while canceling the booking" });
        }

      
            
           

         




        // DELETE api/<Booking>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
