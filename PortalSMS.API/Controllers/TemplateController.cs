using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalSMS.BL.DTOs;
using PortalSMS.DAL.Data.Context;
using PortalSMS.DAL.Data.Models;
using X.PagedList;
using X.PagedList.Extensions;


namespace PortalSMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplateController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SystemContext _context;
        public TemplateController(IConfiguration configuration, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SystemContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        //[Authorize(Roles = "Admin,Sender")]
        //[HttpGet("temp")]
        //public async Task<ActionResult<IEnumerable<GetMessageTemplateDto>>> GetTemplates()
        //{
        //    var templates = await _context.MessageTemplates
        //        .Select(t => new GetMessageTemplateDto
        //        {
        //            TemplateID = t.TemplateID,
        //            TemplateName = t.TemplateName,
        //            TemplateContent = t.TemplateContent,
        //            CreatedByUsername = t.Creator.UserName, // Get the username of the creator
        //            LastModifiedBy = t.LastModifiedBy, // Include this if needed
        //            CreatedDate = t.CreatedDate,
        //            LastModifiedDate = t.LastModifiedDate // This may be null
        //        })
        //        .ToListAsync();

        //    return Ok(templates);
        //}

        [Authorize(Roles = "Admin,Sender")]
        [HttpGet("temp")]
        public async Task<ActionResult<IEnumerable<GetMessageTemplateDto>>> GetTemplates(int page = 1, int pageSize = 5)
        {
            var templates = await _context.MessageTemplates
                .Select(t => new GetMessageTemplateDto
                {
                    TemplateID = t.TemplateID,
                    TemplateName = t.TemplateName,
                    TemplateContent = t.TemplateContent,
                    CreatedByUsername = t.Creator.UserName,
                    CreatedDate = t.CreatedDate,
                    LastModifiedDate = t.LastModifiedDate
                })
                .OrderBy(t => t.TemplateID) // Ensure consistent ordering
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.MessageTemplates.CountAsync(); // Get total count for pagination

            var result = new
            {
                Data = templates,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Ok(result);
        }



        [Authorize(Roles = "Admin,Sender")]
        [HttpPost("addtemp")]
        public async Task<ActionResult<MessageTemplateDto>> CreateTemplate([FromBody] MessageTemplateDto templateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                // Log the claims for debugging
                var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                return Unauthorized(new
                {
                    Message = "User identity name is missing.",
                    Claims = claims
                });
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var template = new MessageTemplate
            {
                TemplateName = templateDto.TemplateName,
                TemplateContent = templateDto.TemplateContent,
                CreatedBy = user.Id,
                CreatedDate = DateTime.Now,
                LastModifiedBy = user.Id,
                LastModifiedDate = DateTime.Now
            };

            _context.MessageTemplates.Add(template);
            await _context.SaveChangesAsync();

            templateDto.TemplateID = template.TemplateID;
            templateDto.CreatedDate = template.CreatedDate;

            return CreatedAtAction(nameof(GetTemplates), new { id = template.TemplateID }, templateDto);
        }




        [Authorize(Roles = "Admin,Sender")]
        [HttpPut("edittemp/{id}")]
        public async Task<ActionResult<EditMessageTemplateDto>> EditTemplate(int id, [FromBody] EditMessageTemplateDto templateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Retrieve the message template by ID
            var template = await _context.MessageTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound($"MessageTemplate with ID {id} not found.");
            }

            // Retrieve the username from the claims
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                return Unauthorized(new
                {
                    Message = "User identity name is missing.",
                    Claims = claims
                });
            }

            // Find the user by username
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            // Log the incoming DTO values
            Console.WriteLine($"Updating template ID {id} with Name: {templateDto.TemplateName} and Content: {templateDto.TemplateContent}");

            // Update the template properties
            template.TemplateName = templateDto.TemplateName;
            template.TemplateContent = templateDto.TemplateContent;

            _context.MessageTemplates.Update(template);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error updating template: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the template.");
            }

            return Ok(templateDto);
        }



        [Authorize(Roles = "Admin,Sender")]
        [HttpDelete("deletetemp/{id}")]
        public async Task<ActionResult> DeleteTemplate(int id)
        {
            // Retrieve the message template by ID
            var template = await _context.MessageTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound($"MessageTemplate with ID {id} not found.");
            }

            // Remove the template from the context
            _context.MessageTemplates.Remove(template);

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Log the exception for debugging and return a 500 status code
                Console.WriteLine($"Error deleting template: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the template.");
            }

            // Return a 204 No Content status code to indicate successful deletion
            return NoContent();
        }

        [Authorize(Roles = "Admin,Sender")]
        [HttpGet("template/{id}")]
        public async Task<ActionResult<GetMessageTemplateDto>> GetTemplateById(int id)
        {
            var template = await _context.MessageTemplates
                                         .Where(t => t.TemplateID == id)
                                         .Select(t => new GetMessageTemplateDto
                                         {
                                             TemplateID = t.TemplateID,
                                             TemplateName = t.TemplateName,
                                             TemplateContent = t.TemplateContent,
                                             CreatedByUsername = t.CreatedBy,
                                             CreatedDate = t.CreatedDate,
                                             LastModifiedDate = t.LastModifiedDate
                                         })
                                         .FirstOrDefaultAsync();

            if (template == null)
            {
                return NotFound($"MessageTemplate with ID {id} not found.");
            }

            return Ok(template);
        }



    }
}
