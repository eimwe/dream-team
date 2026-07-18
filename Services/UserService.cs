using System.Security.Claims;
using dream_team.Data;
using dream_team.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace dream_team.Services;

public class UserService
{
    private readonly AppDbContext _db;
    private readonly DbService _dbService;

    public UserService(AppDbContext db, DbService dbService)
    {
        _db = db;
        _dbService = dbService;
    }

    public (string Provider, string ProviderUid) GetUserProvider(AuthenticateResult result)
    {
        return (
            Provider: result.Principal?.Identity?.AuthenticationType ?? "",
            ProviderUid: result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? ""
        );
    }

    public (string FirstName, string LastName) GetUserFullName(AuthenticateResult result)
    {
        return (
            FirstName: result.Principal?.FindFirstValue(ClaimTypes.GivenName) ?? "",
            LastName: result.Principal?.FindFirstValue(ClaimTypes.Surname) ?? ""
        );
    }

    public async Task<User?> FindUser(AuthenticateResult result)
    {
        var (Provider, ProviderUid) = GetUserProvider(result);

        var oauthAccount = await _dbService.GetOauthAccount(Provider, ProviderUid);

        if (oauthAccount == null)
        {
            return null;
        }

        return await _db.Users.FirstAsync(user => user.Id == oauthAccount.UserId);
    }

    public async Task<User> CreateUser(AuthenticateResult result)
    {
        var (Provider, ProviderUid) = GetUserProvider(result);
        var (FirstName, LastName) = GetUserFullName(result);

        var user = new User
        {
            Login = ProviderUid,
            FirstName = FirstName,
            LastName = LastName,
        };

        return await _dbService.InsertNewUser(user, Provider, ProviderUid);
    }

    public async Task<List<Claim>> CreateClaims(User user)
    {
        var roles = await _db
            .UserRoles.Where(role => role.UserId == user.Id)
            .Select(entry => entry.Role)
            .ToListAsync();

        var claims = new List<Claim> { new("userId", user.Id.ToString()) };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return claims;
    }

    public ClaimsIdentity CreateIdentity(List<Claim> claims)
    {
        return new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
