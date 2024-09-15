using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalSMS.API.Service;
using PortalSMS.BL.DTOs;
using PortalSMS.DAL.Data.Context;
using PortalSMS.DAL.Data.Models;

namespace PortalSMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SystemContext _context;
        private readonly CsvService _csvService;
        private readonly SmsService _smsService;
        private readonly ILogger<UserController> _logger;


        public MessageController(IConfiguration configuration, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SystemContext context, CsvService csvService, SmsService smsService, ILogger<UserController> logger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _csvService = csvService;
            _smsService = smsService;
            _logger = logger;

        }



        [Authorize(Roles = "Admin,Sender")]
        [HttpPost("sendmessage")]
        public async Task<ActionResult> SendMessage([FromBody] SendMessageDto sendMessageDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Fetch the user based on UserId
            var user = await _userManager.FindByIdAsync(sendMessageDto.UserId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {sendMessageDto.UserId} does not exist."); // Logging the issue
                return Unauthorized("User does not exist.");
            }

            // Fetch the message template
            var template = await _context.MessageTemplates.FindAsync(sendMessageDto.TemplateId);
            if (template == null)
            {
                return NotFound($"MessageTemplate with ID {sendMessageDto.TemplateId} not found.");
            }

            // Split recipients by comma
            var recipientNumbers = sendMessageDto.Recipients.Split(',').Select(r => r.Trim()).ToList();

            // Create and save the SentMessage for each recipient
            foreach (var recipientNumber in recipientNumbers)
            {
                var sentMessage = new SentMessage
                {
                    MessageID = GenerateUniqueMessageID(),
                    UserID = sendMessageDto.UserId,
                    RecipientPhoneNumber = recipientNumber,
                    MessageContent = sendMessageDto.MessageContent,
                    TemplateID = sendMessageDto.TemplateId,
                    SentDate = DateTime.Now,
                    Status = "Pending" // or any other default status
                };

                _context.SentMessages.Add(sentMessage);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Messages sent successfully." });
        }


        private string GenerateUniqueMessageID()
        {
            return Guid.NewGuid().ToString(); // Example of generating a unique string ID
        }


        [Authorize(Roles = "Admin,Sender")]
        [HttpPost("send-sms")]
        public async Task<IActionResult> SendSms(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                var validPhoneNumbers = _csvService.ProcessCsv(file.OpenReadStream());

                if (validPhoneNumbers == null || !validPhoneNumbers.Any())
                {
                    return BadRequest("No valid phone numbers found.");
                }

                foreach (var phoneRecord in validPhoneNumbers)
                {
                    string messageContent = "This is a test message.";

                    try
                    {
                        await _smsService.SendSmsAsync(phoneRecord.PhoneNumber, messageContent);
                    }
                    catch (Exception ex)
                    {
                        // Log individual SMS send errors
                        Console.WriteLine($"Error sending SMS to {phoneRecord.PhoneNumber}: {ex.Message}");
                        // You might want to continue sending SMS to other numbers even if one fails
                    }
                }

                return Ok(new { Message = "SMS sent to all valid phone numbers.", PhoneNumbers = validPhoneNumbers.Select(p => p.PhoneNumber).ToList() });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error sending SMS: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("uploadcsv")]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                var validPhoneNumbers = _csvService.ProcessCsv(file.OpenReadStream());

                // Process the valid phone numbers (e.g., save to database, etc.)
                // For example:
                // await SavePhoneNumbersToDatabase(validPhoneNumbers);

                return Ok(new { Message = "CSV file processed successfully", PhoneNumbers = validPhoneNumbers });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error processing CSV file: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }




        [HttpPost("sms-status")]
        public async Task<IActionResult> SmsStatusCallback([FromForm] string SmsSid, [FromForm] string MessageStatus)
        {
            // Find the message in your database using the SmsSid
            var sentMessage = await _context.SentMessages.FirstOrDefaultAsync(m => m.MessageID == SmsSid);

            if (sentMessage != null)
            {
                // Update the message status
                sentMessage.Status = MessageStatus;

                // Create a log entry
                var log = new Log
                {
                    MessageID = sentMessage.MessageID,
                    LogMessage = $"Status updated to: {MessageStatus}",
                    LogDate = DateTime.UtcNow,
                    LogLevel = "Info"
                };

                // Add log entry to database
                _context.Logs.Add(log);

                // Save changes to database
                await _context.SaveChangesAsync();
            }

            return Ok();
        }


    }
}
