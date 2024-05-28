using MealApp.Context;
using MealApp.Migrations;
using MealApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace MealApp.Repo
{
    public class CouponRepository : ICouponRepository
    {
        private readonly AppDbContext _authContext;
       
        public CouponRepository(AppDbContext appDbContext ) 
        {
            _authContext = appDbContext;
          

            
        }
        public async Task RemoveCouponAsync(Coupon coupon)
        {
            _authContext.coupons.Remove(coupon);
            await _authContext.SaveChangesAsync();
        }
        public string EncodeString(string input)
        {
            StringBuilder encoded = new StringBuilder();

            foreach (char c in input)
            {
                char encodedChar = (char)(c + 4);
                encoded.Append(encodedChar);
            }

            return encoded.ToString();
        }

        public string DecodeString(string input)
        {
            StringBuilder decoded = new StringBuilder();

            foreach (char c in input)
            {
                char decodedChar = (char)(c - 4);
                decoded.Append(decodedChar);
            }

            return decoded.ToString();
        }

        public async Task SaveCouponCodeAsync(int userId, string couponCode)
        {
            var coupon = await _authContext.coupons.FirstOrDefaultAsync(x => x.UserId == userId);

           
            if (coupon != null)
            {
               coupon.CouponCode = couponCode;

                await _authContext.SaveChangesAsync();
            }
        }

      
    }
}
