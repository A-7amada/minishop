using MiniShop.Domain.Entities;

namespace MiniShop.Domain.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> FindByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task AddAsync(AppUser user);
}
