namespace mini_mes_be.Models;

/// <summary>
/// Application user with authentication support.
/// Login is performed via Email + Password.
/// </summary>
public class User : BaseEntity
{
    public string username { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string password_hash { get; set; } = string.Empty;
    public string role { get; set; } = "User";
    public bool is_active { get; set; } = true;

    // Navigation
    public ICollection<RefreshToken> refresh_tokens { get; set; } = [];
}
