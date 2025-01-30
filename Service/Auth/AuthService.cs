using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using ProjetDotNet.Service.Email;

namespace ProjetDotNet.Service;

using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ProjetDotNet.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


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

    public async Task<string> LoginAsync(LoginViewModel model)
    {
        Console.WriteLine($"[INFO] LoginAsync started for Email: {model.Email}");

        // Attempt to find the user by email
        Console.WriteLine($"[INFO] Checking if a user exists with Email: {model.Email}");
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            Console.WriteLine($"[WARNING] Login failed: No user found with Email: {model.Email}");
            return null;
        }

        Console.WriteLine($"[INFO] User found for Email: {model.Email}. Validating password.");

        // Validate the password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!isPasswordValid)
        {
            Console.WriteLine($"[WARNING] Login failed: Invalid password for Email: {model.Email}");
            return null;
        }

        Console.WriteLine($"[INFO] Password validated for Email: {model.Email}. Generating JWT token.");
        
        var isConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        if (!isConfirmed)
        {
            Console.WriteLine($"[WARNING] Login failed: Email not confirmed for Email: {model.Email}");
            return "Email not confirmed";
        }
        
        Console.WriteLine($"[INFO] Email confirmed for Email: {model.Email}. Generating JWT token.");
        
        // Prepare claims for the JWT token
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Generate the signing key
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

        // Generate the JWT token
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        Console.WriteLine($"[INFO] JWT token successfully generated for Email: {model.Email}");

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    public async Task<(bool isSuccess, List<string> errors)> RegisterAsync(RegisterViewModel model)
    {
        Console.WriteLine($"[INFO] RegisterAsync started for Email: {model.Email}");
        var errors = new List<string>();

        // Check if the user already exists
        Console.WriteLine($"[INFO] Checking if a user already exists with Email: {model.Email}");
        var userExists = await _userManager.FindByEmailAsync(model.Email);
        if (userExists != null)
        {
            Console.WriteLine($"[WARNING] Registration failed: User with Email {model.Email} already exists.");
            errors.Add("Email already exists.");
            return (false, errors);
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
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var actionContext = new ActionContext(
                    _httpContextAccessor.HttpContext,
                    _httpContextAccessor.HttpContext.GetRouteData(),
                    new ActionDescriptor()
                );

                // Generate confirmation link
                var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
                var confirmationLink = urlHelper.Action(
                    "ConfirmEmail",
                    "Auth",
                    new { userId = user.Id, token },
                    protocol: _httpContextAccessor.HttpContext.Request.Scheme);


                // Send confirmation email
                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your email by clicking this link: <a href='{confirmationLink}'>link</a>");
                return (true, errors);
            }
            else
            {
                Console.WriteLine($"[ERROR] Registration failed for Email: {model.Email}. Errors:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"[ERROR] {error.Description}");
                    errors.Add(error.Description);
                }
                return (false, errors);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] An exception occurred during registration for Email: {model.Email}");
            Console.WriteLine($"[ERROR] Exception Message: {ex.Message}");
            errors.Add("An unexpected error occurred. Please try again later.");
            return (false, errors);
        }
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string token) {
        Console.WriteLine($"[INFO] ConfirmEmailAsync started for UserId: {userId}");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            Console.WriteLine($"[WARNING] ConfirmEmail failed: No user found with UserId: {userId}");
            return false;
        }

        Console.WriteLine($"[INFO] User found for UserId: {userId}. Confirming email.");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            Console.WriteLine($"[INFO] Email confirmed for UserId: {userId}");
            return true;
        }

        Console.WriteLine($"[WARNING] Email confirmation failed for UserId: {userId}. Errors:");
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"[WARNING] {error.Description}");
        }

        return false;
    }
}
