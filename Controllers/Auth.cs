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
    public IActionResult Login()
    {
        return View(); // Render the Login view
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
        await Task.Delay(1000);
        if (!ModelState.IsValid)
        {
            return View(model); // Return the view with validation errors
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
        return RedirectToAction("Login");
    }
}