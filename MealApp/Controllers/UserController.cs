using MealApp.Context;
using MealApp.Helpers;
using MealApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using MealApp.UtilityService;
using MealApp.Models.Dto;

namespace MealApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _authContext;
        private readonly IConfiguration _configration;
        private readonly IEmailService _emailService;
        public UserController(AppDbContext appDbContext, IConfiguration configuration, IEmailService emailService)
        {
            _authContext = appDbContext;
            _configration = configuration;
            _emailService = emailService;
        }

        [HttpPost("authenticate")]

        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == userObj.Username);

            if (user == null)
                return NotFound(new { message = "User Not Found!" });

            if (!PasswordHasher.VerifyPassword(userObj.Password, user.Password))
            {
                return BadRequest(new { Message = "Password is Incorrect" });
            }

            user.Token = CreateJwt(user);

            return Ok(new
            {
                Token = user.Token,
                message = "Login Success!"
            });
        }

        [HttpPost("register")]

        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            //check username
            if (await CheckUserNameExistAsync(userObj.Username))
                return BadRequest(new { Message = "Username Already Exists!" });
            //check userEmail
            if (await CheckUEmailExistAsync(userObj.Email))
                return BadRequest(new { Message = "Email Already Exists!" });

            //Check password Strength
            var pass = CheckPasswordStrenght(userObj.Password);
            if (!string.IsNullOrEmpty(pass))
                return BadRequest(new { Message = pass.ToString() });


            //
            userObj.Password = PasswordHasher.HashPassword(userObj.Password);
            userObj.Role = "User";
            userObj.Token = "";
            //
            await _authContext.Users.AddAsync(userObj);
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                message = "User Registered!"
            });
        }

        private Task<bool> CheckUserNameExistAsync(string userename)
           => _authContext.Users.AnyAsync(x => x.Username == userename);
        //{
        //    return await _authContext.Users.AnyAsync(x => x.Username == userename);
        //}

        private Task<bool> CheckUEmailExistAsync(string email)
           => _authContext.Users.AnyAsync(x => x.Email == email);

        private string CheckPasswordStrenght(string password)
        {
            StringBuilder sb = new StringBuilder();
            if (password.Length < 8)
                sb.Append("Minimun password length should be 8" + Environment.NewLine);
            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]")))
                sb.Append("Password should be Aplhanumiric" + Environment.NewLine);
            if (!Regex.IsMatch(password, "[<>,@!#$%^&*()]"))
                sb.Append("Password Should Contain Special char" + Environment.NewLine);
            return sb.ToString();
        }

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysceret....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name,$"{user.FirstName} {user.LastName}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddHours(2),
                SigningCredentials = credentials,
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<User>> GetAllUser()
        {
            return Ok(await _authContext.Users.ToListAsync());
        }

        ////[HttpPost("send-reset-email/{email}")]
        ////public async Task<IActionResult> SendEmail(string email)
        ////{
        ////    var user = await _authContext.Users.FirstOrDefaultAsync(a => a.Email == email);
        ////    if (user is null)
        ////    {
        ////        return NotFound(new
        ////        {
        ////            StatusCode = 404,
        ////            Message = "Email doesn't exist"
        ////        });
        ////    }

        ////    var token = Guid.NewGuid().ToString("N"); // Generate unique token
        ////    user.ResetPasswordToken = token;
        ////    user.RestPAsswordExpiry = DateTime.UtcNow.AddMinutes(15); // Use UTC time

        ////    try
        ////    {
        ////        string from = _configration["EmailSettings:From"];
        ////        var emailModel = new EmailModel(email, "Reset Password", EmailBody.EmailStringBody(email, token));
        ////        _emailService.SendEmail(emailModel); // Ensure EmailService is implemented correctly
        ////        await _authContext.SaveChangesAsync();

        ////        return Ok(new
        ////        {
        ////            StatusCode = 200,
        ////            Message = "Email Sent!"
        ////        });
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        // Log the exception
        ////        return StatusCode(500, new
        ////        {
        ////            StatusCode = 500,
        ////            Message = $"Internal Server Error : {ex.Message}"
        ////        });
        ////    }
        ////}


        [HttpPost("send-reset-email/{email}")]
        public async Task<IActionResult> sendemail(string email)
        {
            var user = await _authContext.Users.FirstOrDefaultAsync(a => a.Email == email);
            if (user is null)
            {
                return NotFound(new
                {
                    statuscode = 404,
                    message = "email doesn't exsit"
                });
            }
            var tokenbyte = RandomNumberGenerator.GetBytes(64);
            var emaitoken = Convert.ToBase64String(tokenbyte);
            user.ResetPasswordToken = emaitoken;
            user.RestPAsswordExpiry = DateTime.Now.AddMinutes(15);
            string from = _configration["emailsettings:from"];
            var emailmodel = new EmailModel(email, "reset password!!", EmailBody.EmailStringBody(email, emaitoken));
            _emailService.SendEmail(emailmodel);
            _authContext.Entry(user).State = EntityState.Modified;
            await _authContext.SaveChangesAsync();
            await _authContext.AddRangeAsync();
            return Ok(new
            {
                statuscode = 200,
                message = "email sent!"
            });

        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var newToken = resetPasswordDto.EmailToken.Replace(" ", "+");
            var user = await _authContext.Users.AsNoTracking().FirstOrDefaultAsync(a => a.Email == resetPasswordDto.Email);
            if (user is null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "User Doesn't Exsit"
                });
            }
            var tokenCode = user.ResetPasswordToken;
            DateTime emailTokenExpiry = user.RestPAsswordExpiry;
            if (tokenCode != resetPasswordDto.EmailToken || emailTokenExpiry < DateTime.Now)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Invalid Reset link"
                });
            }
            user.Password = PasswordHasher.HashPassword(resetPasswordDto.NewPassword);
            _authContext.Entry(user).State = EntityState.Modified;
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Password Reset Successfully"
            });
        }
    }
}
