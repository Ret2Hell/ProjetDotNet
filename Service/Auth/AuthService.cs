using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProjetDotNet.Models;
using ProjetDotNet.Service.Email;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProjetDotNet.Service
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public AuthService(UserManager<IdentityUser> userManager, IConfiguration configuration, EmailSender emailSender, IHttpContextAccessor httpContextAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        /// <summary>
        /// Attempts to log in a user and returns a JSON-friendly response.
        /// </summary>
        public async Task<LoginResponse> LoginAsync(LoginViewModel model)
        {
            Console.WriteLine($"[INFO] LoginAsync started for Email: {model.Email}");

            // Check if a user exists with the provided email.
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                Console.WriteLine($"[WARNING] Login failed: No user found with Email: {model.Email}");
                return new LoginResponse { IsSuccess = false, Message = "Invalid credentials" };
            }

            // Validate the password.
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                Console.WriteLine($"[WARNING] Login failed: Invalid password for Email: {model.Email}");
                return new LoginResponse { IsSuccess = false, Message = "Invalid credentials" };
            }

            // Ensure the user's email is confirmed.
            var isConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (!isConfirmed)
            {
                Console.WriteLine($"[WARNING] Login failed: Email not confirmed for Email: {model.Email}");
                return new LoginResponse { IsSuccess = false, Message = "Email not confirmed" };
            }

            Console.WriteLine($"[INFO] Email confirmed for Email: {model.Email}. Generating JWT token.");

            // Prepare claims for the JWT token.
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Generate the signing key.
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            // Generate the JWT token.
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            Console.WriteLine($"[INFO] JWT token successfully generated for Email: {model.Email}");

            return new LoginResponse
            {
                IsSuccess = true,
                Token = tokenString,
                Message = "Login successful"
            };
        }

        /// <summary>
        /// Registers a new user and returns a JSON-friendly response.
        /// </summary>
        public async Task<RegisterResponse> RegisterAsync(RegisterViewModel model)
        {
            Console.WriteLine($"[INFO] RegisterAsync started for Email: {model.Email}");
            var errors = new List<string>();

            // Check if a user already exists with the provided email.
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                Console.WriteLine($"[WARNING] Registration failed: User with Email {model.Email} already exists.");
                errors.Add("Email already exists.");
                return new RegisterResponse { IsSuccess = false, Errors = errors, Message = "Registration failed" };
            }

            Console.WriteLine($"[INFO] No existing user found for Email: {model.Email}. Proceeding with registration.");
            var user = new IdentityUser
            {
                Email = model.Email,
                UserName = model.UserName
            };

            Console.WriteLine($"[INFO] Attempting to create user in the database for Email: {model.Email}");
            try
            {
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    Console.WriteLine($"[INFO] User with Email: {model.Email} successfully registered.");

                    // Generate email confirmation token.
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var actionContext = new ActionContext(
                        _httpContextAccessor.HttpContext,
                        _httpContextAccessor.HttpContext.GetRouteData(),
                        new ActionDescriptor()
                    );

                    // Generate confirmation link.
                    var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
                    var confirmationLink = urlHelper.Action(
                        "ConfirmEmail",
                        "Auth",
                        new { userId = user.Id, token },
                        protocol: _httpContextAccessor.HttpContext.Request.Scheme);

                    // Send confirmation email.
                    await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                        $"Please confirm your email by clicking this link: <a href='{confirmationLink}'>link</a>");

                    return new RegisterResponse
                    {
                        IsSuccess = true,
                        Errors = new List<string>(),
                        Message = "Registration successful. Please confirm your email."
                    };
                }
                else
                {
                    Console.WriteLine($"[ERROR] Registration failed for Email: {model.Email}. Errors:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"[ERROR] {error.Description}");
                        errors.Add(error.Description);
                    }
                    return new RegisterResponse { IsSuccess = false, Errors = errors, Message = "Registration failed" };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An exception occurred during registration for Email: {model.Email}");
                Console.WriteLine($"[ERROR] Exception Message: {ex.Message}");
                errors.Add("An unexpected error occurred. Please try again later.");
                return new RegisterResponse { IsSuccess = false, Errors = errors, Message = "Registration failed" };
            }
        }

        /// <summary>
        /// Confirms a user's email and returns a JSON-friendly response.
        /// </summary>
        public async Task<ConfirmEmailResponse> ConfirmEmailAsync(string userId, string token)
        {
            Console.WriteLine($"[INFO] ConfirmEmailAsync started for UserId: {userId}");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"[WARNING] ConfirmEmail failed: No user found with UserId: {userId}");
                return new ConfirmEmailResponse { IsSuccess = false, Message = "User not found" };
            }

            Console.WriteLine($"[INFO] User found for UserId: {userId}. Confirming email.");
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                Console.WriteLine($"[INFO] Email confirmed for UserId: {userId}");
                return new ConfirmEmailResponse { IsSuccess = true, Message = "Email confirmed successfully" };
            }
            else
            {
                var errorMessages = result.Errors.Select(e => e.Description);
                var errorMessage = string.Join(", ", errorMessages);
                Console.WriteLine($"[WARNING] Email confirmation failed for UserId: {userId}. Errors: {errorMessage}");
                return new ConfirmEmailResponse { IsSuccess = false, Message = errorMessage };
            }
        }
    }

    // Response Models for API-friendly responses

    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }
    }

    public class RegisterResponse
    {
        public bool IsSuccess { get; set; }
        public List<string> Errors { get; set; }
        public string Message { get; set; }
    }

    public class ConfirmEmailResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
