using System.Security.Claims;
using dream_team.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace dream_team.Utilities;

public static class AuthHelpers
{
    public static Task HandleRemoteFailure(RemoteFailureContext context)
    {
        context.Response.Redirect("/Auth/Login?error=external");
        context.HandleResponse();

        return Task.CompletedTask;
    }

    public static async Task<bool?> IsInAdminRole(
        HttpContext httpContext,
        ClaimsPrincipal user,
        UserService userService,
        List<int> userIds
    )
    {
        var currentUserId = userService.GetCurrentUserId(user);

        if (currentUserId == null || !userIds.Contains(currentUserId.Value))
            return null;

        var currentUser = await userService.FindUser(currentUserId.Value);

        if (currentUser == null)
            return null;

        var claims = await userService.CreateClaims(currentUser);
        var identity = userService.CreateIdentity(claims);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity)
        );

        return claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "administrator");
    }

    public static bool IsOwner(this ClaimsPrincipal user, int ownerId)
    {
        if (user.IsInRole("administrator"))
        {
            return true;
        }

        var currentUserIdClaim = user.FindFirstValue("userId");

        return int.TryParse(currentUserIdClaim, out var currentUserId) && currentUserId == ownerId;
    }
}
