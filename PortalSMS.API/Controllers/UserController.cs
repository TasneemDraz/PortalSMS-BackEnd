using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PortalSMS.BL.DTOs;
using PortalSMS.BL.DTOs.Authentication;
using PortalSMS.DAL.Data.Context;
using PortalSMS.DAL.Data.Models;
using System.Formats.Asn1;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Text.RegularExpressions;
using PortalSMS.API.Service;
namespace PortalSMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SystemContext _context;
        private readonly ILogger<UserController> _logger;




        public UserController(ILogger<UserController> logger,IConfiguration configuration, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SystemContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;


        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login(LoginDTO credentials)
        {
            var user = await _userManager.FindByNameAsync(credentials.UserName);
            if (user == null)
            {
                // Log the unsuccessful login attempt
                _logger.LogWarning($"Login attempt failed: User {credentials.UserName} not found.");

                var log = new Log
                {
                    MessageID = Guid.NewGuid().ToString(), // Or generate based on your needs
                    LogMessage = $"Login attempt failed: User {credentials.UserName} not found.",
                    LogDate = DateTime.UtcNow,
                    LogLevel = "Warning"
                };
                _context.Logs.Add(log);
                await _context.SaveChangesAsync();

                return BadRequest("User not found");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                // Log the unsuccessful login attempt
                _logger.LogWarning($"Login attempt failed: User {credentials.UserName} is locked out.");

                var log = new Log
                {
                    MessageID = Guid.NewGuid().ToString(), // Or generate based on your needs
                    LogMessage = $"Login attempt failed: User {credentials.UserName} is locked out.",
                    LogDate = DateTime.UtcNow,
                    LogLevel = "Warning"
                };
                _context.Logs.Add(log);
                await _context.SaveChangesAsync();

                return BadRequest("Try again later");
            }

            bool isAuthenticated = await _userManager.CheckPasswordAsync(user, credentials.Password);
            if (!isAuthenticated)
            {
                await _userManager.AccessFailedAsync(user);

                // Log the unsuccessful login attempt
                _logger.LogWarning($"Login attempt failed: Invalid credentials for user {credentials.UserName}.");

                var log = new Log
                {
                    MessageID = Guid.NewGuid().ToString(), // Or generate based on your needs
                    LogMessage = $"Login attempt failed: Invalid credentials for user {credentials.UserName}.",
                    LogDate = DateTime.UtcNow,
                    LogLevel = "Warning"
                };
                _context.Logs.Add(log);
                await _context.SaveChangesAsync();

                return Unauthorized("Invalid credentials");
            }

            // Log successful login
            _logger.LogInformation($"User {credentials.UserName} logged in successfully.");

            var logSuccess = new Log
            {
                MessageID = Guid.NewGuid().ToString(), // Or generate based on your needs
                LogMessage = $"User {credentials.UserName} logged in successfully.",
                LogDate = DateTime.UtcNow,
                LogLevel = "Information"
            };
            _context.Logs.Add(logSuccess);
            await _context.SaveChangesAsync();

            // Initialize userClaims as a List<Claim>
            var userClaims = (await _userManager.GetClaimsAsync(user)).ToList();

            // Add additional claims
            userClaims.AddRange(new[]
            {
        new Claim(ClaimTypes.Name, user.UserName), // User's username
        new Claim(ClaimTypes.Email, user.Email),   // User's email
        new Claim(ClaimTypes.NameIdentifier, user.Id) // User's unique identifier
    });

            // Get user roles and add them as claims
            var userRoles = await _userManager.GetRolesAsync(user);
            userClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Retrieve the secret key from configuration
            var secretKey = _configuration.GetValue<string>("SecretKey");
            var secretKeyInBytes = Encoding.ASCII.GetBytes(secretKey);
            var key = new SymmetricSecurityKey(secretKeyInBytes);

            // Define signing credentials
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var expiration = DateTime.Now.AddMinutes(15);

            // Create the JWT token
            var jwt = new JwtSecurityToken(
                claims: userClaims,
                notBefore: DateTime.Now,
                issuer: "backendApplication",
                audience: "weather",
                expires: expiration,
                signingCredentials: signingCredentials
            );

            // Create the token handler and write the token as a string
            var tokenHandler = new JwtSecurityTokenHandler();
            string tokenString = tokenHandler.WriteToken(jwt);

            // Return the token and its expiration time
            return Ok(new
            {
                Status = "Success",
                Message = "User logged in successfully!",
                Token = tokenString,
                ExpiryDate = expiration
            });
        }



        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<string>> Register(RegisterDTO registerDTO)
        {
            var newUser = new User
            {
                UserName = registerDTO.Username,
                Email = registerDTO.Email,

            };

            var creationResult = await _userManager.CreateAsync(newUser, registerDTO.Password);
            if (!creationResult.Succeeded)
            {
                return BadRequest(creationResult.Errors);
            }

          
            await _userManager.AddToRoleAsync(newUser, registerDTO.Role);

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, newUser.UserName),
                new Claim(ClaimTypes.Email, newUser.Email),
            };

            await _userManager.AddClaimsAsync(newUser, userClaims);

            return Ok("User registered successfully");
        }

        private async Task<IActionResult> AssignRoleToUser(User user, string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                return BadRequest($"Role {role} does not exist.");
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                return BadRequest($"Failed to assign role {role} to user.");
            }

            return Ok();
        }



        [HttpPost("Sender")]
        [Authorize(Roles = "Sender")]
        public IActionResult SenderActions()
        {
            // Only users with the "Sender" role can create messages
            return Ok("Message created");
        }

        [HttpGet("Viewer")]
        [Authorize(Roles = "Viewer")]
        public IActionResult ViewerActions()
        {
            // Only users with the "Viewer" role can view messages
            return Ok("Viewing messages");
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminActions()
        {
            // Only users with the "Admin" role can access this
            return Ok("Admin actions");
        }








      






    }
}




