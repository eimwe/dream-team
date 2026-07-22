using System.Security.Claims;
using dream_team.Models.ViewModels;
using dream_team.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace dream_team.Controllers;

public class AuthController : Controller
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult Login(string? error)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return User.IsInRole("administrator")
                ? RedirectToAction("Index", "Users")
                : RedirectToAction("Index", "Home");
        }

        var model = new AuthViewModel();

        if (error == "external")
        {
            model.ErrorMessage = "Sign-in failed. Please try again.";
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Login(AuthViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Login) || string.IsNullOrWhiteSpace(model.Password))
        {
            model.ErrorMessage = "Please enter both login and password.";
            return View(model);
        }

        var user = await _userService.AuthWithPassword(model.Login, model.Password);

        if (user == null)
        {
            model.ErrorMessage = "Invalid login or password";
            return View(model);
        }

        var claims = await _userService.CreateClaims(user);
        var identity = _userService.CreateIdentity(claims);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity)
        );

        return RedirectToAction("Index", "Users");
    }

    [HttpGet]
    public IActionResult ExternalLogin(string provider)
    {
        var redirectUrl = Url.Action("ExternalLoginCallback", "Auth");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback()
    {
        var result = await HttpContext.AuthenticateAsync("External");

        if (!result.Succeeded || result.Principal == null)
        {
            return RedirectToAction("Login");
        }

        var user = await _userService.FindUser(result) ?? await _userService.CreateUser(result);

        var claims = await _userService.CreateClaims(user);

        var identity = _userService.CreateIdentity(claims);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity)
        );

        await HttpContext.SignOutAsync("External");

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
