namespace dream_team.Models.ViewModels;

public class AuthViewModel
{
    public string? Login { get; set; }
    public string? Password { get; set; }

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
}
