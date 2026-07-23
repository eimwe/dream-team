using dream_team.Data;
using dream_team.Models;
using dream_team.Models.ViewModels;
using dream_team.Services;
using dream_team.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dream_team.Controllers;

[Authorize(Roles = "administrator")]
public class UsersController : Controller
{
    private static readonly string[] AssignableRoles =
    {
        "candidate",
        "recruiter",
        "administrator",
    };

    private readonly AppDbContext _db;
    private readonly UserService _userService;

    public UsersController(AppDbContext db, UserService userService)
    {
        _db = db;
        _userService = userService;
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

    [HttpPost]
    public async Task<IActionResult> AddRole(List<int> ids, string role)
    {
        if (ids.Count == 0 || !AssignableRoles.Contains(role))
            return RedirectToAction("Index");

        var usersAlreadyWithRole = await _db
            .UserRoles.Where(r => ids.Contains(r.UserId) && r.Role == role)
            .Select(r => r.UserId)
            .ToListAsync();

        var newRoles = ids.Except(usersAlreadyWithRole)
            .Select(userId => new UserRole { UserId = userId, Role = role });

        _db.UserRoles.AddRange(newRoles);
        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = $"Role \"{role}\" added to selected users.";

        return await CheckAdminRole(ids) ?? RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveRole(List<int> ids, string role)
    {
        if (ids.Count == 0 || !AssignableRoles.Contains(role))
            return RedirectToAction("Index");

        await _db
            .UserRoles.Where(r => ids.Contains(r.UserId) && r.Role == role)
            .ExecuteDeleteAsync();

        TempData["StatusMessage"] = $"Role \"{role}\" removed from selected users.";

        return await CheckAdminRole(ids) ?? RedirectToAction("Index");
    }

    private async Task<IActionResult?> CheckAdminRole(List<int> ids)
    {
        var stillAdmin = await AuthHelpers.IsInAdminRole(HttpContext, User, _userService, ids);

        if (stillAdmin == null)
        {
            return null;
        }

        return stillAdmin.Value ? RedirectToAction("Index") : RedirectToAction("Index", "Home");
    }
}
