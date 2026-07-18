using dream_team.Data;
using dream_team.Models;
using Microsoft.EntityFrameworkCore;

namespace dream_team.Services;

public class DbService
{
    private readonly AppDbContext _db;

    public DbService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserOauth?> GetOauthAccount(string provider, string providerUid)
    {
        return await _db.UserOauths.FirstOrDefaultAsync<UserOauth>(account =>
            account.Provider == provider && account.ProviderUid == providerUid
        );
    }

    public async Task<User> InsertNewUser(User user, string provider, string providerUid)
    {
        user.OauthAccounts.Add(new UserOauth { Provider = provider, ProviderUid = providerUid });
        user.Roles.Add(new UserRole { Role = "candidate" });
        user.Status = new UserStatus { Status = "active" };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return user;
    }
}
