namespace BizSecureDemo.Models;
public class AppUser
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";

    // Brute-force protection
    public int FailedLogins { get; set;} 
    public DateTime? LockoutUntilUtc { get; set; } 
}

