using Microsoft.AspNetCore.Mvc;
using ProjetDotNet.Models;
using ProjetDotNet.Service;
using System.Threading.Tasks;
using System.Collections.Generic;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(model);
        if (!result.IsSuccess)
        {
            return Unauthorized(new { message = result.Message });
        }
        
        return Ok(new { token = result.Token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(model);
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new { message = "Registration successful. Please confirm your email." });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Invalid request." });
        }

        var result = await _authService.ConfirmEmailAsync(userId, token);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = "Email confirmation failed." });
        }
        return Ok(new { message = "Email successfully confirmed." });
    }
}