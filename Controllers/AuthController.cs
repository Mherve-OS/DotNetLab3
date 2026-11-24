using System.Security.Cryptography;
using System.Text;
using StockTrack.Models;

namespace StockTrack.Controllers;

public class AuthController
{
    private readonly UserRepository _userRepository;
    private readonly AuditRepository _auditRepository;

    public static User? CurrentUser { get; private set; }

    public static bool IsLoggedIn => CurrentUser != null;
    public static bool IsAdmin => CurrentUser?.Role == "Admin";
    public static bool IsInventoryManager => CurrentUser?.Role is "Admin" or "Inventory Manager";
    public static bool IsStaff => CurrentUser != null;

    public AuthController(UserRepository userRepository, AuditRepository auditRepository)
    {
        _userRepository = userRepository;
        _auditRepository = auditRepository;
    }

    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return string.Empty;
        using var sha = SHA256.Create();
        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        var builder = new StringBuilder();
        foreach (byte b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }

    public User Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.");
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.");

        string hash = HashPassword(password);
        var user = _userRepository.Authenticate(username.Trim(), hash);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password, or account is deactivated.");
        }

        CurrentUser = user;
        _auditRepository.LogAction(user.UserID, "LOGIN", $"User '{user.Username}' ({user.Role}) logged in.");
        return user;
    }

    public void Logout()
    {
        if (CurrentUser != null)
        {
            _auditRepository.LogAction(CurrentUser.UserID, "LOGOUT", $"User '{CurrentUser.Username}' logged out.");
            CurrentUser = null;
        }
    }
}
