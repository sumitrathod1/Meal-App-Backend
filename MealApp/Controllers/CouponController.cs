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
        public CouponController( AppDbContext appDbContext, ICouponRepository couponRepository) 
        {
            _authContext = appDbContext;
            _couponRepository = couponRepository;
        }

        [HttpPost("GenerateCouponCode")]
        public async Task<IActionResult> GenerateCouponCode([FromBody] CouponDTO couponDTO)
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
            DateTime expirationTime = currentDate.AddMinutes(1);

            // save data in coupon
            var coupon = new Coupon
            {
                UserId = userId,
                CreatedTime = currentDate,
                expiredTime = expirationTime

            };
            _authContext.coupons.Add(coupon);
            await _authContext.SaveChangesAsync();

           // await _couponRepository.AddCouponAsync(coupon);

          
            // Combine fields into a single string
            string combinedString = $"{username}%%{userId}%%{currentDate:yyyy-MM-dd HH:mm:ss}%%{expirationTime:yyyy-MM-dd HH:mm:ss}";

            // Encode the string
            string encodedString = _couponRepository.EncodeString(combinedString);

            // Save the encoded string as a coupon code
            await _couponRepository.SaveCouponCodeAsync(userId, encodedString);

            return Ok(new { couponCode = encodedString });
        }

        
       
    }
}