using MiniShop.Domain.Entities;
using MiniShop.Domain.Interfaces;
using MiniShop.Domain.Security;

namespace MiniShop.Application.Services;

public class AccountService
{
    private readonly IUserRepository _users;
    public AccountService(IUserRepository users) => _users = users;

    public async Task<(bool ok, string? error, AppUser? user)> RegisterAsync(string fullName, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "All fields are required.", null);

        var normalized = email.Trim().ToLowerInvariant();
        if (await _users.EmailExistsAsync(normalized))
            return (false, "This email is already registered.", null);

        var (hash, salt) = PasswordHasher.Hash(password);
        var user = new AppUser { FullName = fullName.Trim(), Email = normalized, PasswordHash = hash, PasswordSalt = salt };
        await _users.AddAsync(user);
        return (true, null, user);
    }

    public async Task<AppUser?> ValidateLoginAsync(string email, string password)
    {
        var user = await _users.FindByEmailAsync(email.Trim().ToLowerInvariant());
        return user is not null && user.IsActive && PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt) ? user : null;
    }

    public Task<List<AppUser>> ListUsersAsync() => _users.ListAsync();

    // Admin accounts cannot be deactivated or deleted from the admin area (demo lockout protection).
    public async Task<bool> SetActiveAsync(int id, bool active)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null || user.IsAdmin) return false;
        user.IsActive = active;
        await _users.UpdateAsync(user);
        return true;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null || user.IsAdmin) return false;
        await _users.DeleteAsync(user);
        return true;
    }
}
