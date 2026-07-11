using System.Diagnostics;
using dream_team.Data;
using dream_team.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IActionResult> DbCheck([FromServices] AppDbContext db)
    {
        var canConnect = await db.Database.CanConnectAsync();
        var userCount = canConnect ? await db.Users.CountAsync() : -1;
        return Content($"CanConnect: {canConnect}, Users: {userCount}");
    }
}
