using MealApp.Context;
using MealApp.Models.Dto;
using MealApp.Repo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MealApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        private readonly AppDbContext _dbContext;

        public AdminController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
           
        }



        // POST api/<AdminController>
        [HttpPost("Setcredits")]
        public async Task<IActionResult> GiveCredits ([FromBody] AdminDTO adminDTO)
        {
            int credits = adminDTO.Credits;
            string email = adminDTO.Email;

            
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            if(user == null)
            {
                return BadRequest(new { Message = "User not found!" });      // if email is not matched
            }

            if(credits <= 0)
            {
                return BadRequest(new { Message = "Please enter valid Credits!" });   //credits are not negitive
            }
            int ReservedCredits = user.Credits;
            int OldAllowedAccess = user.AllowedAccess;

           int NewCredits = ReservedCredits + credits;                      //update credits 
            int NewAllowedAccess = OldAllowedAccess + credits;              // update allowedaccess also
           
            if(NewAllowedAccess > 66)          // prevent user to get access more then 3 Months
            { 
                return BadRequest(new { Message = " Not valid ! , User exceed the limit of 3 Months." });   //credits are not negitive

            }
            user.Credits = NewCredits; 
            user.AllowedAccess = NewAllowedAccess;
          

            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Credits Set successfully" });

        }

       
    }
}
