using Microsoft.AspNetCore.Authentication;

namespace dream_team.Utilities;

public static class AuthHelpers
{
    public static Task HandleRemoteFailure(RemoteFailureContext context)
    {
        context.Response.Cookies.Append(
            "AuthErrorHint",
            "external",
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
            }
        );

        context.Response.Redirect("/Auth/Login");
        context.HandleResponse();

        return Task.CompletedTask;
    }
}
