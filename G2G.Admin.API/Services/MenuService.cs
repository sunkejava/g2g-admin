using Microsoft.EntityFrameworkCore;
using G2G.Admin.API.Data;
using G2G.Admin.API.Entities;

namespace G2G.Admin.API.Services;

public interface IMenuService
{
    Task<List<Menu>> GetAllAsync();
    Task<List<Menu>> GetTreeAsync();
    Task<Menu?> GetByIdAsync(int id);
    Task<Menu> CreateAsync(CreateMenuDto dto);
    Task<Menu?> UpdateAsync(int id, UpdateMenuDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<Menu>> GetMenusByUserIdAsync(int userId);
}

public class CreateMenuDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Path { get; set; }
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public int Sort { get; set; } = 0;
}

public class UpdateMenuDto
{
    public string? Name { get; set; }
    public string? Path { get; set; }
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public int? Sort { get; set; }
}

public class MenuService : IMenuService
{
    private readonly G2GDbContext _dbContext;

    public MenuService(G2GDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Menu>> GetAllAsync()
    {
        return await _dbContext.Menus.OrderBy(m => m.Sort).ToListAsync();
    }

    public async Task<List<Menu>> GetTreeAsync()
    {
        var menus = await _dbContext.Menus.OrderBy(m => m.Sort).ToListAsync();
        return BuildTree(menus, null);
    }

    private List<Menu> BuildTree(List<Menu> menus, int? parentId)
    {
        return menus
            .Where(m => m.ParentId == parentId)
            .Select(m => m)
            .ToList();
    }

    public async Task<Menu?> GetByIdAsync(int id)
    {
        return await _dbContext.Menus.FindAsync(id);
    }

    public async Task<Menu> CreateAsync(CreateMenuDto dto)
    {
        var menu = new Menu
        {
            Code = dto.Code,
            Name = dto.Name,
            Path = dto.Path,
            Icon = dto.Icon,
            ParentId = dto.ParentId,
            Sort = dto.Sort
        };

        _dbContext.Menus.Add(menu);
        await _dbContext.SaveChangesAsync();
        return menu;
    }

    public async Task<Menu?> UpdateAsync(int id, UpdateMenuDto dto)
    {
        var menu = await _dbContext.Menus.FindAsync(id);
        if (menu == null) return null;

        menu.Name = dto.Name ?? menu.Name;
        menu.Path = dto.Path ?? menu.Path;
        menu.Icon = dto.Icon ?? menu.Icon;
        menu.ParentId = dto.ParentId ?? menu.ParentId;
        menu.Sort = dto.Sort ?? menu.Sort;

        await _dbContext.SaveChangesAsync();
        return menu;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var menu = await _dbContext.Menus.FindAsync(id);
        if (menu == null) return false;

        _dbContext.Menus.Remove(menu);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<Menu>> GetMenusByUserIdAsync(int userId)
    {
        return await _dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_dbContext.RoleMenus, ur => ur.RoleId, rm => rm.RoleId, (ur, rm) => rm.MenuId)
            .Distinct()
            .Join(_dbContext.Menus, menuId => menuId, m => m.Id, (menuId, m) => m)
            .OrderBy(m => m.Sort)
            .ToListAsync();
    }
}
