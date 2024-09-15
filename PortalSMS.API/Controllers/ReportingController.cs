using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalSMS.BL.DTOs;
using PortalSMS.DAL.Data.Context;
using PortalSMS.DAL.Data.Models;
using System.Text;

namespace PortalSMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportingController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SystemContext _context;
        public ReportingController(IConfiguration configuration, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SystemContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [Authorize(Roles = "Admin,Viewer")]
        [HttpGet]
        public IActionResult GetMessagesReport(string username, DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _context.SentMessages
                .Include(m => m.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(username))
            {
                query = query.Where(m => m.User.UserName == username);
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(m => m.SentDate >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(m => m.SentDate <= dateTo.Value);
            }

            var reportData = query.Select(m => new
            {
                m.MessageID,
                SenderUsername = m.User.UserName,
                PhoneNumber = m.RecipientPhoneNumber,
                m.MessageContent,
                SenderUser = m.User.UserName,
                DateTimeSent = m.SentDate
            }).ToList();

            return Ok(reportData);
        }


        [Authorize(Roles = "Admin,Viewer")]
        [HttpGet("export")]
        public IActionResult ExportMessagesReport(string username, DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _context.SentMessages
                .Include(m => m.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(username))
            {
                query = query.Where(m => m.User.UserName == username);
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(m => m.SentDate >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(m => m.SentDate <= dateTo.Value);
            }

            var reportData = query.Select(m => new
            {
                ID = m.MessageID,
                SenderUsername = m.User.UserName,
                PhoneNumber = m.RecipientPhoneNumber,
                MessageContent = m.MessageContent,
                SenderUser = m.User.UserName,
                DateTimeSent = m.SentDate
            }).ToList();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("ID,SenderUsername,PhoneNumber,MessageContent,SenderUser,DateTimeSent");

            foreach (var item in reportData)
            {
                csvBuilder.AppendLine($"{item.ID},{item.SenderUsername},{item.PhoneNumber},{item.MessageContent},{item.SenderUser},{item.DateTimeSent:yyyy-MM-dd HH:mm:ss}");
            }

            var csvBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            var stream = new MemoryStream(csvBytes);
            return File(stream, "text/csv", "MessagesReport.csv");
        }

    }
}
