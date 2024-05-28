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
            DateTime currentDate = DateTime.Now;
            DateTime expirationTime = currentDate.AddMinutes(4);

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

            return Ok(new { couponCode = encodedString });
        }


        //scan QR
        [HttpPost("ValidateCouponCode")]
        public async Task<IActionResult> ValidateCouponCode([FromBody] CouponValidationDTO couponValidationDTO)
        {
            string email = couponValidationDTO.Email;
            string couponCode = couponValidationDTO.CouponCode;
            DateTime today = DateTime.Now;

            //find user based on email
            var user = await _authContext.Users.FirstOrDefaultAsync(x=> x.Email == email);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            int userId = user.Id;
            //find the object where userid matched in coupon table
            var couponobj = await _authContext.coupons.FirstOrDefaultAsync(y => y.UserId == userId);
            

            if (couponobj == null || couponobj.CouponCode != couponCode)
            {
                return BadRequest(new { message = "Invalid coupon code" });
            }

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

            await _couponRepository.RemoveCouponAsync(couponobj);

            return Ok(new { message = "Coupon code validated and booking status updated" });
            // can add one sound in angular like thank you
        }
    }

}
