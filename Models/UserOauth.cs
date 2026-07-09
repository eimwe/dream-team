namespace dream_team.Models;

public class UserOauth
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Provider { get; set; } = "";
    public string ProviderUid { get; set; } = "";
}
