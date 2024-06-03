using MealApp.Models;

namespace MealApp.Helpers
{
    public static class EmailBody
    {
        public static string EmailStringBody(string email,string emailToken)
        {
            return $@"<html>

<head>
</head>
  <body style=""margin:0;padding:0;font-family:arial,Helvetica,sans-serif;"">
    <div style=""height: auto:background: linear-gradient(to top, #c9c9ff 50%, #6e6ef6 90%) no-reapeat;width:400px;padding:30px"">
      <div>
        <div>
          <h1>Reset your Password</h1>
          <hr>
          <p>You're reciving this e-mail beacuse you requested a password rest</p>
          <p>Please tap the button below to choose a new password.</p>
  
          <a href=""http://localhost:4200/reset?email={email}&code={emailToken}"" target=""_blank"" style=""background:#0d6efd;padding:10px;border:none; color:white; border-radius:4px;display:block;margin:0 auto;width:50;text-align:center:text-descoration:none"">reaset Password</a><br>
  
          <p>Kind regards, <br><br> Meal App</p>
        </div>
      </div>
    </div>
  </body>
</html>";
        }

        public static string BookingConfirmationEmailBody(string firstName, string startDate, string endDate, string bookingtype)
        {
            return $@"<html>
            <head></head>
            <body style=""margin:0;padding:0;font-family:arial,Helvetica,sans-serif;"">
                <div >
                    <div>
                        <h1>Booking Confirmation</h1>
                        <hr>
                        <p>Hello {firstName},</p><br>
                        <p>Your booking from <strong>{startDate}</strong> to <strong>{endDate}</strong> for <strong>{bookingtype}</strong> has been successfully created.</p>
                        <p>If you have any questions or need further assistance, please do not hesitate to contact us.</p>
                        <p>Best regards, <br><br>RISE MEAL FACILITY</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string BookingConfirmationEmailBodytoAdmin(string firstName, string startDate, string endDate, string bookingtype)
        {
            return $@"<html>
            <head></head>
            <body style=""margin:0;padding:0;font-family:arial,Helvetica,sans-serif;"">
                <div >
                    <div>
                        <h1>Booking Confirmation of {firstName}</h1>
                        <hr>
                        <p>Hello Admin,</p><br>
                        <p>{firstName}'s Booking from <strong>{startDate}</strong> to <strong>{endDate}</strong> for <strong>{bookingtype}</strong> has been successfully created.</p>
                        
                        <p>Best regards, <br><br> {firstName}</p>
                    </div>
                </div>
            </body>
            </html>
            ";
        }

        public static string BookingCancellationEmailBody( string FirstName,string SelectedDate, string Bookingtype)
        {
            return $@"<html>
            <head></head>
            <body style=""margin:0;padding:0;font-family:arial,Helvetica,sans-serif;"">
                <div >
                    <div>
                        <h1>Booking Cancellation Confirmation</h1>
                        <hr>
                        <p>Hello {FirstName},</p><br>
                        <p>Your booking for <strong>{SelectedDate}</strong> of  <strong>{Bookingtype}</strong> has been successfully canceled.</p>
                        <p>If you have any questions or need further assistance, please do not hesitate to contact us.</p>
                        <p>Best regards, <br><br>RISE MEAL FACILITY</p>
                    </div>
                </div>
            </body>
            </html>";
        }
        public static string BookingCancellationEmailBodytoAdmin(string FirstName, string SelectedDate, string Bookingtype)
        {
            return $@"<html>
            <head></head>
            <body style=""margin:0;padding:0;font-family:arial,Helvetica,sans-serif;"">
                <div >
                    <div>
                        <h1>Booking Cancellation of {FirstName}</h1>
                        <hr>
                        <p>Hello Admin,</p><br>
                        <p>{FirstName}'s Booking of <strong>{Bookingtype}</strong> for <strong>{SelectedDate}</strong> has been successfully canceled.</p>
                        
                        <p>Best regards, <br><br> {FirstName}</p>
                    </div>
                </div>
            </body>
            </html>
            ";
        }


        public static string BookingConfirmationQuickEmailBody(string firstName, string BookingDate, string bookingtype)
        {
            return $@"<html>
            <head></head>
            <body style=""margin:0;padding:0;font-family:arial,Helvetica,sans-serif;"">
                <div >
                    <div>
                        <h1>Quick Booking Confirmation</h1>
                        <hr>
                        <p>Hello {firstName},</p><br>
                        <p>Your booking for <strong>{bookingtype}</strong> on <strong>{BookingDate}</strong>   has been successfully created.</p>
                        <p>If you have any questions or need further assistance, please do not hesitate to contact us.</p>
                        <p>Best regards, <br><br>RISE MEAL FACILITY</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string BookingConfirmationQuickEmailBodytoAdmin(string firstName, string bookingdate, string bookingtype)
        {
            return $@"<html>
            <head></head>
            <body style=""margin:0;padding:0;font-family:arial,Helvetica,sans-serif;"">
                <div >
                    <div>
                        <h1> Quick Booking Confirmation of {firstName}</h1>
                        <hr>
                        <p>Hello Admin,</p><br>
                        <p>{firstName}'s Booking for <strong>{bookingtype}</strong>  on <strong>{bookingdate}</strong> has been successfully created.</p>
                        
                        <p>Best regards, <br><br> {firstName}</p>
                    </div>
                </div>
            </body>
            </html>
            ";
        }


    }
}
