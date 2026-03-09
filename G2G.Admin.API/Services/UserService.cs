using Microsoft.EntityFrameworkCore;
using G2G.Admin.API.Data;
using G2G.Admin.API.Entities;

namespace G2G.Admin.API.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(CreateUserDto dto);
    Task<User?> UpdateAsync(int id, UpdateUserDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ResetPasswordAsync(int id, string newPassword);
    Task<bool> ToggleStatusAsync(int id);
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Password { get; set; } = string.Empty;
    public List<int> RoleIds { get; set; } = new();
}

public class UpdateUserDto
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public List<int> RoleIds { get; set; } = new();
}

public class UserService : IUserService
{
    private readonly G2GDbContext _dbContext;

    public UserService(G2GDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<User>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        return await _dbContext.Users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User> CreateAsync(CreateUserDto dto)
    {
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            Phone = dto.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        if (dto.RoleIds.Any())
        {
            foreach (var roleId in dto.RoleIds)
            {
                _dbContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });
            }
            await _dbContext.SaveChangesAsync();
        }

        return user;
    }

    public async Task<User?> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await GetByIdAsync(id);
        if (user == null) return null;

        user.Email = dto.Email ?? user.Email;
        user.Phone = dto.Phone ?? user.Phone;
        user.UpdatedAt = DateTime.UtcNow;

        if (dto.RoleIds.Any())
        {
            var existingRoles = _dbContext.UserRoles.Where(ur => ur.UserId == id).ToList();
            _dbContext.UserRoles.RemoveRange(existingRoles);

            foreach (var roleId in dto.RoleIds)
            {
                _dbContext.UserRoles.Add(new UserRole { UserId = id, RoleId = roleId });
            }
        }

        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null) return false;

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetPasswordAsync(int id, string newPassword)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleStatusAsync(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null) return false;

        user.Status = !user.Status;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
