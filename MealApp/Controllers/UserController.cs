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

using MealApp.Models.Dto;
using MealApp.Repo;
using Org.BouncyCastle.Bcpg;

namespace MealApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _authContext;
        private readonly IConfiguration _configration;
        private readonly IEmailRepository _emailRepository;
        private readonly INotificationRepository _NotificationRepository;
        private readonly IUserRepository _userRepository;
      //  private readonly NotificationRepository notificationRepository;
        public UserController(AppDbContext appDbContext,IUserRepository userRepository, IConfiguration configuration, INotificationRepository notificationRepository, IEmailRepository emailrepository)
        {
            _authContext = appDbContext;
            _configration = configuration;
            _emailRepository = emailrepository;
            _NotificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        [HttpPost("getfirstname")]
        public async Task<IActionResult> firstname([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == userObj.Username);

            if (user == null)
                return NotFound(new { message = "User Not Found!" });

            string firstname = user.FirstName;
            return Ok(new
            {
                name = firstname
            }) ;
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
            
            // email for successfull registration

            string from = _configration["emailsettings:from"];
            string subject = "Successfully Registered ";
            string body = $"Hello {userObj.FirstName},\n\n You are successfully registered in Rishabh Meal. Now you can book your delicious food. \n we are concern about healthy food.\n\nBest regards,\nYour Rishabh Software";
            var emailModel = new EmailModel(userObj.Email, subject, body);
            _emailRepository.SendEmail(emailModel);
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
            _emailRepository.SendEmail(emailmodel);
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



        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            if (changePasswordDTO == null)
                return BadRequest();


            string oldPasswordINPUT = changePasswordDTO.OldPassword;
            string newPassword = changePasswordDTO.NewPassword;
            string EMAIL = changePasswordDTO.Email;

           
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == EMAIL);
            string oldPasswordSTORED = user.Password;
            if (user == null)
                return NotFound(new { message = "User Not Found!" });

            if (!PasswordHasher.VerifyPassword(oldPasswordINPUT, oldPasswordSTORED))
            {
                return BadRequest(new { Message = "Password is Incorrect" });
            }

            user.Password = PasswordHasher.HashPassword(newPassword);  

            // Update user entity instead of adding a new one.
              _authContext.Users.Update(user);
            //
            DateTime time = DateTime.Now;
            var notification = new Notification
            {
                UserId = user.Id,
                Description = "NewPassword_Set: " + $"Your password has been Changed on {time}.",
                CreatedAt = DateTime.Now
            };
            await _NotificationRepository.AddNotificationAsync(notification);
        
            // Save changes to the database.
            await _authContext.SaveChangesAsync();


            // Send reminder for renewal of meal email
            string from = _configration["emailsettings:from"];
            string subject = "Password Changed!";
            string body = $"Hello {user.FirstName},\n\n" +
               "We wanted to let you know that your password has been changed successfully.\n\n" +
               "If you did not make this change, please contact our support team immediately to secure your account.\n\n" +
               "If you have any questions or need further assistance, feel free to reach out to us.\n\n" +
               "Thank you for using our services.\n\n" +
               "Best regards,\n" +
               "Rishabh Software";
            var emailModel = new EmailModel(user.Email, subject, body);
            _emailRepository.SendEmail(emailModel);

           
           


            return Ok(new
               {
                   StatusCode = 200,
                   Message = "Password reset successfully"
               });

           
        }


        [HttpPost("contectus")]
        public async Task<IActionResult> contect([FromBody] ContectUsDTO contectDTO)
        {
            if (contectDTO == null)
                return BadRequest();


            string fullname = contectDTO.FullName;
            string email = contectDTO.Email;
            string sub = contectDTO.Subject;
            string message = contectDTO.Body;


            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == email);
         
            if (user == null)
            {
                var contect = new Contect
                {
                    FullName = fullname,
                    Email = email,
                    Subject = sub,
                    Body = message

                };
                await _userRepository.AddContectusDataAsync(contect);

            }
            else
            {
                int userid = user.Id;
                var contect = new Contect
                {
                    FullName = fullname,
                    Email = email,
                    UserId = userid,
                    Subject = sub,
                    Body = message

                };
                await _userRepository.AddContectusDataAsync(contect);
            }


            // Send reminder for renewal of meal email
            // from : 00aj0009 to 00aj0009 (rise to rise)

            string ToEmailAddress = "00aj0009@gmail.com";
            string from = _configration["emailsettings:from"];
            string subject = sub;
            string body = $"Hello RISE Team,\n\n" +
              message + "Best regards,\n" + fullname;

            var emailModel = new EmailModel(ToEmailAddress, subject, body);
            _emailRepository.SendEmail(emailModel);





            return Ok(new  { Message = "Email sent...Suppoert Team will shortly contect you or gives attention on this."  });


        }


        //send data in feedback
        [HttpPost("feedback")]
        public async Task<IActionResult> givefeedback([FromBody] FeedbackDTO feedbackDTO)
        {
            if (feedbackDTO == null)
                return BadRequest();


            string mealtype = feedbackDTO.MealType;
            string message = feedbackDTO.Message;
            int rating = feedbackDTO.Rating;
            string email = feedbackDTO.Email;



            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            if(user == null)
            {
                return BadRequest(new { Message = "user not found!." });
            }

            int userid = user.Id;
            string Fname = user.FirstName;
            DateTime cuurdate = DateTime.Now;

            if (cuurdate.Hour > 12 && cuurdate.Hour < 16)  // feedback is allowed for lunch 12pm to 6pm
            {
                if(mealtype == "Dinner")              // time for lunch only
                {
                    return BadRequest(new { Message = "Feedback form is closed now for Dinner." });

                }
                //check for todays lunch is booked  ////////////////////////////////
                bool todaysLunch = await _userRepository.IstodaysLunchAquired(userid, cuurdate);

                if (todaysLunch)           //  aquired or not 
                {
                    // check to give only one feedback for day and for meal type
                    bool todayslunchfeedback = await _userRepository.IsLunchFeedbackstored(userid, cuurdate);

                    if (todayslunchfeedback)    // once given then return
                    {
                        return BadRequest(new { Message = "Your Feedback is stored Once." });
                    }


                    // then allowed to give feedback
                    var feedback = new Feedback
                    {
                        UserId = userid,
                        Rating = rating,
                        FeedbackTimeStamp = cuurdate,
                        FirstName = Fname,
                        Message = message,
                        MealType = mealtype,

                    };
                    _authContext.feedbacks.Add(feedback);
                    await _authContext.SaveChangesAsync();

                    return Ok(new { Message = "Thank You for Your Valueable Feedback" });

                }
                else
                {
                    return BadRequest(new { Message = "Sorry!...You have not Readeem Todays Lunch." });

                }
                }
            else if (cuurdate.Hour > 20 && cuurdate.Hour < 24)  // feedback is allowed for dinner 8pm to 12am
              {
                if (mealtype == "Dinner")              // time for dinner only
                    {
                    return BadRequest(new { Message = "Feedback form is closed now for Dinner." });

                    }
                    //check for todays lunch is booked  ////////////////////////////////
                bool todaysDinner = await _userRepository.IstodaysDinnerAquired(userid, cuurdate);

                if (todaysDinner)
                    {

                    // check to give only one feedback for day and for meal type
                    bool todaysdinnerfeedback = await _userRepository.IsDinnerFeedbackstored(userid, cuurdate);

                    if (todaysdinnerfeedback)    // once given then return
                    {
                        return BadRequest(new { Message = "Your Feedback is stored Once." });
                    }

                    // then allowed to give feedback
                    var feedback = new Feedback
                        {
                        UserId = userid,
                        Rating = rating,
                        FeedbackTimeStamp = cuurdate,
                        FirstName = Fname,
                        Message = message,
                        MealType = mealtype,

                         };
                     _authContext.feedbacks.Add(feedback);
                     await _authContext.SaveChangesAsync();

                     return Ok(new { Message = "Thank You for Your Valueable Feedback" });

                 }
                 else
                 {
                    return BadRequest(new { Message = "Sorry!...You have not Readeem Todays Dinner." });

                 }

            } 
            else
            {
                return BadRequest(new { Message = "Sorry!...Feedback form is closed now. It will open between 12pm to 4pm for lunch and open for dinner between 8pm to 12am" });

            }

        }
    }
}
