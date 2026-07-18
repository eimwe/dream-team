using System.Diagnostics;
using dream_team.Models;
using Microsoft.AspNetCore.Mvc;

namespace dream_team.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }

    public IActionResult HeaderCheck()
    {
        var proto = Request.Headers["X-Forwarded-Proto"].ToString();
        var forwardedFor = Request.Headers["X-Forwarded-For"].ToString();
        var scheme = Request.Scheme;
        return Content(
            $"X-Forwarded-Proto: '{proto}' | X-Forwarded-For: '{forwardedFor}' | Request.Scheme (post-middleware): '{scheme}'"
        );
    }
}
