using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BookHub.Models;
using BookHub.DTOs;
using Microsoft.AspNetCore.Authorization;
using BookHub.Services;

namespace BookHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : BaseController
    {
        // Attributes
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        private readonly EmailService _emailService;

        // Email Service Constructor

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            EmailService emailService,
            ILogger<AccountController> logger)
            : base(logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        // Methods
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            Logger.LogInformation("Register called for email {Email}", registerDto.Email);

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                DateJoined = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                Logger.LogWarning("Register failed for email {Email}: {Errors}", registerDto.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(result.Errors);
            }

            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Build verification link
            var verificationLink = Url.Action(
                "VerifyEmail",
                "Account",
                new { userId = user.Id, token },
                Request.Scheme
            );

            // Send email
            _emailService.SendEmail(user.Email, "BookHub Email Verification",
                $"Please verify your email by clicking the following link: {verificationLink}");

            Logger.LogInformation("Register successful for email {Email}. Verification email sent.", registerDto.Email);
            return Ok("User registered successfully. An email verification link has been sent.");
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string userId, string token)
        {
            Logger.LogInformation("VerifyEmail called for userId {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) 
            {
                Logger.LogWarning("VerifyEmail: user not found for id {UserId}", userId);
                return NotFound("User not found.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded) 
            {
                Logger.LogInformation("VerifyEmail: email verification successful for userId {UserId}", userId);
                return Ok("Email verification successful.");
            }

            Logger.LogWarning("VerifyEmail: verification failed for user {UserId}", userId);
            return BadRequest("Email verification failed.");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            Logger.LogInformation("Login called for email {Email}", loginDto.Email);

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                Logger.LogWarning("Login failed: user not found for email {Email}", loginDto.Email);
                return Unauthorized("Invalid credentials");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
            {
                Logger.LogWarning("Login failed: invalid password for email {Email}", loginDto.Email);
                return Unauthorized("Invalid credentials");
            }

            Logger.LogInformation("Login: user {UserId} logged in successfully", user.Id);
            var token = await GenerateJwtToken(user);
            return Ok(new { token });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            Logger.LogInformation("Me endpoint called");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                Logger.LogWarning("Me endpoint: user ID claim not found");
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                Logger.LogWarning("Me endpoint: user not found for id {UserId}", userId);
                return NotFound();
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return Ok(userDto);
        }

        // GET: api/account/users
        // To find user Ids for DeleteUser() testing
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            Logger.LogInformation("GetAllUsers called");

            var users = _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.DateJoined,
                    u.DisplayName
                })
                .ToList();

            Logger.LogInformation("GetAllUsers returned {Count} users", users.Count);
            return Ok(users);
        }

        // Deletes account of currently authenticated user based on JWT token
        // Will not delete if user has reviews
        // Allows users to delete their own accounts, but not other users' accounts (even if admin)
        [HttpDelete("delete")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = GetCurrentUserId(); // from BaseController
            if (userId == null)
                return Unauthorized();

            Logger.LogInformation("DeleteAccount called for user id {UserId}", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    Logger.LogWarning("DeleteAccount: user not found for id {UserId}", userId);
                    return NotFound("User not found.");
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    Logger.LogError("DeleteAccount: failed to delete user {UserId}: {Errors}", userId,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return BadRequest(result.Errors);
                }

                Logger.LogInformation("DeleteAccount: user {UserId} deleted successfully", userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "DeleteAccount: unexplected error occurred deleting user with id {UserId}", userId);
                return StatusCode(500, "An unexpected error occurred while deleting the account.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.NameIdentifier,
                    user.Id
                ),
                new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.Name,
                    user.UserName
                ),
                new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.Email,
                    user.Email
                ),
                new System.Security.Claims.Claim(
                    System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()
                )
            };

            // Add role claims
            foreach (var role in userRoles)
            {
                claims.Add(new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.Role,
                    role
                ));
            }


            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
            );

            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                key,
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256
            );

            var expires = DateTime.UtcNow.AddHours(
                Convert.ToDouble(_configuration["Jwt:ExpireHours"])
            );

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}