namespace dream_team.Models;

public class UserStatus
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public string Status { get; set; } = "active";
}
