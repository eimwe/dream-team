using Microsoft.AspNetCore.Authentication;

namespace dream_team.Utilities;

public static class AuthHelpers
{
    public static Task HandleRemoteFailure(RemoteFailureContext context)
    {
        context.Response.Redirect("/Auth/Login?error=external");
        context.HandleResponse();

        return Task.CompletedTask;
    }
}
