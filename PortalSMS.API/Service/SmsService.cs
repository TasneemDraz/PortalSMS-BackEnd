using Twilio.Types;
using Twilio;
using Twilio.Rest.Api.V2010.Account;


namespace PortalSMS.API.Service
{
    public class SmsService
    {
        private readonly IConfiguration _configuration;

        public SmsService(IConfiguration configuration)
        {
            _configuration = configuration;
            Console.WriteLine($"Twilio Account SID: {_configuration["Twilio:AccountSid"]}");
            Console.WriteLine($"Twilio Auth Token: {_configuration["Twilio:AuthToken"]}");
            Console.WriteLine($"Twilio From Number: {_configuration["Twilio:FromNumber"]}");

            TwilioClient.Init(
                _configuration["Twilio:AccountSid"],
                _configuration["Twilio:AuthToken"]
            );
        }




        public async Task SendSmsAsync(string to, string message)
        {
            try
            {
                var from = new PhoneNumber(_configuration["Twilio:FromNumber"]);
                var toPhoneNumber = new PhoneNumber(to);

                var messageResource = await MessageResource.CreateAsync(
                    body: message,
                    from: from,
                    to: toPhoneNumber,
                   statusCallback: new Uri(_configuration["Twilio:StatusCallbackUrl"]) // Add this line

                );

                // Log the message SID for tracking
                Console.WriteLine($"Message sent to {to} with SID: {messageResource.Sid}");
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error sending SMS to {to}: {ex.Message}");
                throw; // Optionally rethrow if you want to handle it at a higher level
            }
        }


    }


}
