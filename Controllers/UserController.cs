using dream_team.Data;
using dream_team.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dream_team.Controllers;

[Authorize(Roles = "administrator")]
public class UsersController : Controller
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var model = new UsersViewModel
        {
            Users = await _db.Users.Include(u => u.Status).Include(u => u.Roles).ToListAsync(),
            StatusMessage = TempData["StatusMessage"] as string,
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Block(List<int> ids)
    {
        if (ids.Count == 0)
            return RedirectToAction("Index");
        await _db
            .UserStatuses.Where(s => ids.Contains(s.UserId))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "blocked"));
        TempData["StatusMessage"] = "Selected users have been blocked.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Unblock(List<int> ids)
    {
        if (ids.Count == 0)
            return RedirectToAction("Index");
        await _db
            .UserStatuses.Where(s => ids.Contains(s.UserId) && s.Status == "blocked")
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "active"));
        TempData["StatusMessage"] = "Selected users have been unblocked.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(List<int> ids)
    {
        if (ids.Count == 0)
            return RedirectToAction("Index");
        await _db.Users.Where(u => ids.Contains(u.Id)).ExecuteDeleteAsync();
        TempData["StatusMessage"] = "Selected users have been deleted.";
        return RedirectToAction("Index");
    }
}
