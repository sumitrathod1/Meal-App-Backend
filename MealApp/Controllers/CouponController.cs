using MealApp.Context;
using MealApp.Migrations;
using MealApp.Models;
using MealApp.Models.Dto;
using MealApp.Repo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;
using System.ComponentModel;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MealApp.Controllers
{

    

    [Route("api/[controller]")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly AppDbContext _authContext;
        private readonly ICouponRepository _couponRepository;
        private readonly IBookingRepository _bookingRepository;
        public CouponController( AppDbContext appDbContext, ICouponRepository couponRepository,IBookingRepository bookingRepository) 
        {
            _authContext = appDbContext;
            _couponRepository = couponRepository;
          
            _bookingRepository = bookingRepository;
        }


        // generate QR
        [HttpPost("GenerateCouponCode")]
        public async Task<IActionResult> GenerateCouponCode([FromBody] EmailDTO couponDTO)
        {
            string email = couponDTO.Email;
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            int userId = user.Id;
            string username = user.Username;
            string Name = user.FirstName;
            
            DateTime currentDate = DateTime.Now;
            DateTime expirationTime;   // fixed for lunch and dinner

            // check booking of this time (lunch or dinner) is booked or not
             if(currentDate.Hour >= 13 && currentDate.Hour <18)
            {
                //check for lunch booked or not
                var booking = await _authContext.Bookings.FirstOrDefaultAsync(x => x.UserId == userId && x.Type == Models.Type.Lunch && x.Status == Status.Booked && x.Date.Date == currentDate.Date);
                if (booking == null)
                {
                    return BadRequest(new { message = "Booking for Lunch is not found!" });
                }
                // fatched satus of lunch on today
                var booking1 = await _authContext.Bookings.FirstOrDefaultAsync(x => x.UserId == userId && x.Type == Models.Type.Lunch  && x.Date.Date == currentDate.Date);
                if(booking1.Status == Status.Used)
                {
                    return BadRequest(new { message = "Coupon for today's Lunch is Aquired!" });
                }

                 expirationTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 23, 0, 0);  //expiredtime is fixed

            }
            else if (currentDate.Hour >= 18 && currentDate.Hour < 20)  // check for dinner
            {
                //check for lunch booked or not
                var booking = await _authContext.Bookings.FirstOrDefaultAsync(x => x.UserId == userId && x.Type == Models.Type.Dinner && x.Status == Status.Booked && x.Date.Date == currentDate.Date);
                if (booking == null)
                {
                    return BadRequest(new { message = "Booking for Dinner is not found!" });
                }
                // fatched satus of dinner on today
                var booking1 = await _authContext.Bookings.FirstOrDefaultAsync(x => x.UserId == userId && x.Type == Models.Type.Dinner && x.Date.Date == currentDate.Date);
                if (booking1.Status == Status.Used)
                {
                    return BadRequest(new { message = "Coupon for today's Dinner is Aquired!" });
                }

                 expirationTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 22, 0, 0);    //expiredtime is fixed
            }
            else
            {
                return BadRequest(new { message = "this is not right time to take lunch or dinner, Please come on time" });

            }

            // Check if a coupon for this user exists
            var Couponobj = await _authContext.coupons.FirstOrDefaultAsync(c => c.UserId == userId);
            if (Couponobj != null)
            {
                // Delete the existing coupon
                await _couponRepository.RemoveCouponAsync(Couponobj);
            }

            // Save new coupon data
            var coupon = new Coupon
            {
                UserId = userId,
                CreatedTime = currentDate,
                expiredTime = expirationTime
            };
            _authContext.coupons.Add(coupon);
            await _authContext.SaveChangesAsync();

            // Combine fields into a single string
            string combinedString = $"{username}%%{userId}%%{currentDate:yyyy-MM-dd HH:mm:ss}%%{expirationTime:yyyy-MM-dd HH:mm:ss}";

            // Encode the string
            string encodedString = _couponRepository.EncodeString(combinedString);

            // Save the encoded string as a coupon code
            await _couponRepository.SaveCouponCodeAsync(userId, encodedString);

            return Ok(new { couponCode = encodedString , FirstName = Name });
        }


        //scan QR
        [HttpPost("ValidateCouponCode")]
        public async Task<IActionResult> ValidateCouponCode([FromBody] CouponValidationDTO couponValidationDTO)
        {
          //  string email = couponValidationDTO.Email;
            string couponCode = couponValidationDTO.CouponCode;
            DateTime today = DateTime.Now;

            //find user based on coupon
            var coupon = await _authContext.coupons.FirstOrDefaultAsync(x=> x.CouponCode == couponCode);
            if(coupon == null)
            {
                return BadRequest(new { message = "Invalid coupon code" });
            }
            int userId = coupon.UserId;


            var bookingobj = await _bookingRepository.GetBookingStatusByUserIdAsync(userId);
           
            if(bookingobj == null || bookingobj.Status == Status.Used)
            {
                return BadRequest(new {message = "You have already acquired today's lunch" });
            }

            string decodedString = _couponRepository.DecodeString(couponCode);
            var parts = decodedString.Split("%%");
            string username = parts[0];
            DateTime createdTime = DateTime.Parse(parts[2]);
            DateTime expirationTime = DateTime.Parse(parts[3]);

            if (expirationTime < DateTime.Now)
            {
                return BadRequest(new { message = "Coupon code is expired" });
            }

            // change status of user booked to used on todays date 
            await _bookingRepository.ChangeBookingStatusAsync(userId , today);

            await _couponRepository.RemoveCouponAsync(coupon);

            return Ok(new { message = "Coupon code validated and booking status updated" });
            // can add one sound in angular like thank you
        }
    }

}
