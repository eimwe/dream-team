namespace dream_team.Models;

public class User
{
    public int Id { get; set; }

    public string Login { get; set; } = "";
    public string? Password { get; set; }

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";

    public string? Photo { get; set; }
    public string? Location { get; set; }

    public uint Xmin { get; set; }

    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    public UserStatus? Status { get; set; }
    public ICollection<UserOauth> OauthAccounts { get; set; } = new List<UserOauth>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}
