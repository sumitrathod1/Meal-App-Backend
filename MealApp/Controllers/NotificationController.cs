﻿using MealApp.Context;
using MealApp.Models.Dto;
using MealApp.Repo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MealApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {

        private readonly AppDbContext _authContext;
        private readonly INotificationRepository _NotificationRepository;

        public NotificationController(AppDbContext authContext, INotificationRepository notificationRepository)
        {
            _authContext = authContext;
            _NotificationRepository = notificationRepository;
        }


        // display all notifications
        // GET api/<NotificationController>/5
        [HttpPost("Show_notifications")]
        public async Task<IActionResult> GetNotification(NotificationDTO notificationDTO)
        {
            string email = notificationDTO.Email;
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            int userid = user.Id;

            return Ok(_NotificationRepository.FindNotification(userid));
        }

        //[HttpPost]
        // delete notification by NID
        //public async Task<IActionResult> DeleteNotification(NotificationDTO notificationDTO)
        //{
        //    string email = notificationDTO.Email;
        //    string message = notificationDTO.Description;
        //    var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == email);
        //    int userid = user.Id;

        //    FindNotificationId(userid,message);
        //    return Ok(_NotificationRepository.FindNotification(userid));
        //}



        // DELETE api/<NotificationController>/5
        // delete by userid 
        // on clear all notifications

        //[HttpDelete]    // i need when user click on notification messages goes in backend
        //[HttpDelete("DeleteNotificationsByUserId/{userId}")]
        //public async Task<IActionResult> DeleteNotificationsByUserId(int userId)
        //{
        //    await _NotificationRepository.DeleteNotificationsByUserIdAsync(userId);
        //    return Ok(new { message = "All Notifications deleted successfully." });
        //}

        // count of notification of specific user 
        [HttpPost("Count_notifications")]
        public async Task<IActionResult> CountNotifications(NotificationDTO notificationDTO)
        {
            string email = notificationDTO.Email;
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            var count = await _NotificationRepository.CountNotificationsByUserIdAsync(user.Id);
            return Ok(new { notificationCount = count });
        }

        // delete all notifications
        [HttpDelete("deleteAll_notifications")]
       
        public async Task<IActionResult> DeleteNotificationsByUserId(NotificationDTO notificationDTO)
        {
            string email = notificationDTO.Email;
            var user = _authContext.Users.FirstOrDefault(x => x.Email == email);
            await _NotificationRepository.DeleteNotificationsByUserIdAsync(user.Id);
            return Ok(new { message = "All Notifications deleted successfully." });
        }
    }
}
