using Microsoft.AspNetCore.Mvc;
using ProjetDotNet.Models;
using ProjetDotNet.Service;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // GET: /Auth/Login
    [HttpGet]
    public IActionResult Login(string message = "")
    {
        if (!string.IsNullOrEmpty(message))
        {
            ViewBag.Message = message;
        }
        return View();
    }


    // POST: /Auth/Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        await Task.Delay(1000);
        if (!ModelState.IsValid)
        {
            return View(model); // Return the view with validation errors
        }

        var token = await _authService.LoginAsync(model);
        if (token == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials");
            return View(model);
        }
        
        if (token.Equals("Email not confirmed"))
        {
            ModelState.AddModelError(string.Empty, "Email not confirmed");
            return View(model);
        }
        // Store token in a cookie, session, or any other preferred method
        HttpContext.Response.Cookies.Append("JwtToken", token);
        return RedirectToAction("Index", "Home"); // Redirect to home after successful login
    }

    // GET: /Auth/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View(); // Render the Register view
    }

    // POST: /Auth/Register
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (isSuccess, errors) = await _authService.RegisterAsync(model);
        if (!isSuccess)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }
        // Registration successful, redirect to login
        return RedirectToAction("Login", "Auth", new { message = "Registration successful. Please confirm your email." });
    }
    
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return View("Error");
        }

        var isSuccess = await _authService.ConfirmEmailAsync(userId, token);
        if (!isSuccess)
        {
            return View("Error");
        }
        ViewBag.Message = "Email successfully confirmed.";
        return RedirectToAction("Login", "Auth", new { message = "Email successfully confirmed." });
    }


}