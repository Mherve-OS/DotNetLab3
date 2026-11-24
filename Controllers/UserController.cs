using System.Data;
using StockTrack.Models;

namespace StockTrack.Controllers;

public class UserController
{
    private readonly UserRepository _userRepository;
    private readonly AuditRepository _auditRepository;

    public UserController(UserRepository userRepository, AuditRepository auditRepository)
    {
        _userRepository = userRepository;
        _auditRepository = auditRepository;
    }

    public DataTable GetUsers() => _userRepository.GetAll();

    public void AddUser(User user, string plainPassword, int currentUserId)
    {
        Validate(user);
        if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 6)
        {
            throw new ArgumentException("Password must be at least 6 characters.");
        }

        user.PasswordHash = AuthController.HashPassword(plainPassword);
        _userRepository.Insert(user);
        _auditRepository.LogAction(currentUserId, "USER_CREATE", $"Created user '{user.Username}' with role '{user.Role}'.");
    }

    public void EditUser(User user, string? plainPassword, int currentUserId)
    {
        if (user.UserID <= 0)
            throw new ArgumentException("Select a valid user to update.");

        Validate(user);

        if (!string.IsNullOrWhiteSpace(plainPassword))
        {
            if (plainPassword.Length < 6)
                throw new ArgumentException("New password must be at least 6 characters.");
            user.PasswordHash = AuthController.HashPassword(plainPassword);
        }

        _userRepository.Update(user);
        _auditRepository.LogAction(currentUserId, "USER_UPDATE", $"Updated user '{user.Username}' (ID: {user.UserID}).");
    }

    public void ToggleUserStatus(int userId, bool isActive, int currentUserId)
    {
        if (userId <= 0)
            throw new ArgumentException("Select a valid user.");

        if (userId == currentUserId)
            throw new InvalidOperationException("You cannot deactivate your own account.");

        _userRepository.Deactivate(userId, isActive);
        string action = isActive ? "USER_ACTIVATE" : "USER_DEACTIVATE";
        _auditRepository.LogAction(currentUserId, action, $"Changed user {userId} active status to {isActive}.");
    }

    public void RemoveUser(int userId, int currentUserId)
    {
        if (userId <= 0)
            throw new ArgumentException("Select a valid user to delete.");

        if (userId == currentUserId)
            throw new InvalidOperationException("You cannot delete your own account.");

        _userRepository.Delete(userId);
        _auditRepository.LogAction(currentUserId, "USER_DELETE", $"Deleted user ID {userId}.");
    }

    private static void Validate(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Username) || user.Username.Length < 3)
            throw new ArgumentException("Username must be at least 3 characters.");
        if (string.IsNullOrWhiteSpace(user.FullName))
            throw new ArgumentException("Full name is required.");
        if (user.Role is not ("Admin" or "Inventory Manager" or "Staff"))
            throw new ArgumentException("Role must be Admin, Inventory Manager, or Staff.");
    }
}
