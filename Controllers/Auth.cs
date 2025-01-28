using Microsoft.AspNetCore.Mvc;

namespace ProjetDotNet.Controllers;

public class Auth : Controller
{
    public IActionResult Login() => View();
    public IActionResult Register() => View();
}