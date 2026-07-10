namespace dream_team.Models.ViewModels;

public class UsersViewModel
{
    public List<User> Users { get; set; } = new List<User>();
    public string? StatusMessage { get; set; }
}
