using Microsoft.EntityFrameworkCore;
using G2G.Admin.API.Data;
using G2G.Admin.API.Entities;

namespace G2G.Admin.API.Services;

public interface IRoleService
{
    Task<PagedResult<Role>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<Role?> GetByIdAsync(int id);
    Task<Role> CreateAsync(CreateRoleDto dto);
    Task<Role?> UpdateAsync(int id, UpdateRoleDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> AssignMenusAsync(int roleId, List<int> menuIds);
    Task<List<Menu>> GetMenusByRoleIdAsync(int roleId);
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateRoleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class RoleService : IRoleService
{
    private readonly G2GDbContext _dbContext;

    public RoleService(G2GDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<Role>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var query = _dbContext.Roles.AsQueryable();
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return new PagedResult<Role> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        return await _dbContext.Roles
            .Include(r => r.RoleMenus)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Role> CreateAsync(CreateRoleDto dto)
    {
        var role = new Role
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        return role;
    }

    public async Task<Role?> UpdateAsync(int id, UpdateRoleDto dto)
    {
        var role = await _dbContext.Roles.FindAsync(id);
        if (role == null) return null;

        role.Name = dto.Name ?? role.Name;
        role.Description = dto.Description ?? role.Description;
        await _dbContext.SaveChangesAsync();
        return role;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var role = await _dbContext.Roles.FindAsync(id);
        if (role == null) return false;

        _dbContext.Roles.Remove(role);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignMenusAsync(int roleId, List<int> menuIds)
    {
        var role = await _dbContext.Roles.FindAsync(roleId);
        if (role == null) return false;

        var existingMenus = _dbContext.RoleMenus.Where(rm => rm.RoleId == roleId).ToList();
        _dbContext.RoleMenus.RemoveRange(existingMenus);

        foreach (var menuId in menuIds)
        {
            _dbContext.RoleMenus.Add(new RoleMenu { RoleId = roleId, MenuId = menuId });
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<Menu>> GetMenusByRoleIdAsync(int roleId)
    {
        return await _dbContext.RoleMenus
            .Where(rm => rm.RoleId == roleId)
            .Join(_dbContext.Menus, rm => rm.MenuId, m => m.Id, (rm, m) => m)
            .OrderBy(m => m.Sort)
            .ToListAsync();
    }
}
