using Microsoft.EntityFrameworkCore;
using MiniShop.Domain.Entities;
using MiniShop.Domain.Interfaces;
using MiniShop.Infrastructure.Persistence;

namespace MiniShop.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public Task<AppUser?> FindByEmailAsync(string email) => _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    public Task<bool> EmailExistsAsync(string email) => _db.Users.AnyAsync(u => u.Email == email);

    public async Task AddAsync(AppUser user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }
}
