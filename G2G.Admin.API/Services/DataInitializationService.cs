using G2G.Admin.API.Data;
using G2G.Admin.API.Entities;
using G2G.Admin.API.Entities.Logs;
using Microsoft.EntityFrameworkCore;

namespace G2G.Admin.API.Services;

public interface IDataInitializationService
{
    Task InitializeAsync();
}

public class DataInitializationService : IDataInitializationService
{
    private readonly G2GDbContext _dbContext;
    private readonly ILogger<DataInitializationService> _logger;

    public DataInitializationService(G2GDbContext dbContext, ILogger<DataInitializationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // 确保数据库已创建
            await _dbContext.Database.EnsureCreatedAsync();

            // 初始化角色
            await InitializeRolesAsync();

            // 初始化菜单
            await InitializeMenusAsync();

            // 初始化管理员账号
            await InitializeAdminUserAsync();

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("数据初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据初始化失败");
        }
    }

    private async Task InitializeRolesAsync()
    {
        if (!await _dbContext.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role { Name = "超级管理员", Description = "拥有所有权限", CreatedAt = DateTime.UtcNow },
                new Role { Name = "普通用户", Description = "普通用户角色，基础权限", CreatedAt = DateTime.UtcNow },
                new Role { Name = "审计员", Description = "只读权限，可查看日志和监控", CreatedAt = DateTime.UtcNow }
            };

            _dbContext.Roles.AddRange(roles);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("角色初始化完成，创建了 {Count} 个角色", roles.Count);

            // 初始化菜单并分配给超级管理员
            await InitializeMenusWithRolesAsync(roles);
        }
    }

    private async Task InitializeMenusAsync()
    {
        if (!await _dbContext.Menus.AnyAsync())
        {
            var menus = new List<Menu>
            {
                new Menu { Code = "dashboard", Name = "首页", Path = "/dashboard", Icon = "UserFilled", Sort = 1 },
                new Menu { Code = "users", Name = "用户管理", Path = "/users", Icon = "User", Sort = 2 },
                new Menu { Code = "roles", Name = "角色管理", Path = "/roles", Icon = "Lock", Sort = 3 },
                new Menu { Code = "versions", Name = "版本管理", Path = "/versions", Icon = "Upload", Sort = 4 },
                new Menu { Code = "logs", Name = "日志管理", Path = "/logs", Icon = "Document", Sort = 5 },
                new Menu { Code = "settings", Name = "系统配置", Path = "/settings", Icon = "Setting", Sort = 6 },
                new Menu { Code = "monitor", Name = "监控面板", Path = "/monitor", Icon = "Monitor", Sort = 7 }
            };

            _dbContext.Menus.AddRange(menus);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("菜单初始化完成，创建了 {Count} 个菜单", menus.Count);
        }
    }

    private async Task InitializeMenusWithRolesAsync(List<Role> roles)
    {
        var menus = await _dbContext.Menus.ToListAsync();
        var superAdminRole = roles.FirstOrDefault(r => r.Name == "超级管理员");
        var commonUserRole = roles.FirstOrDefault(r => r.Name == "普通用户");
        var auditorRole = roles.FirstOrDefault(r => r.Name == "审计员");

        if (superAdminRole != null && menus.Count > 0)
        {
            // 超级管理员拥有所有菜单权限
            foreach (var menu in menus)
            {
                _dbContext.RoleMenus.Add(new RoleMenu
                {
                    RoleId = superAdminRole.Id,
                    MenuId = menu.Id
                });
            }
            _logger.LogInformation("超级管理员已分配所有菜单权限");
        }

        if (commonUserRole != null && menus.Count > 0)
        {
            // 普通用户只有部分菜单权限
            var commonUserMenus = menus.Where(m => 
                m.Code is "dashboard" or "versions" or "logs"
            ).ToList();

            foreach (var menu in commonUserMenus)
            {
                _dbContext.RoleMenus.Add(new RoleMenu
                {
                    RoleId = commonUserRole.Id,
                    MenuId = menu.Id
                });
            }
            _logger.LogInformation("普通用户已分配 {Count} 个菜单权限", commonUserMenus.Count);
        }

        if (auditorRole != null && menus.Count > 0)
        {
            // 审计员只有查看权限
            var auditorMenus = menus.Where(m => 
                m.Code is "dashboard" or "logs" or "monitor"
            ).ToList();

            foreach (var menu in auditorMenus)
            {
                _dbContext.RoleMenus.Add(new RoleMenu
                {
                    RoleId = auditorRole.Id,
                    MenuId = menu.Id
                });
            }
            _logger.LogInformation("审计员已分配 {Count} 个菜单权限", auditorMenus.Count);
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task InitializeAdminUserAsync()
    {
        if (!await _dbContext.Users.AnyAsync())
        {
            var adminRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "超级管理员");
            
            if (adminRole != null)
            {
                var admin = new User
                {
                    Username = "admin",
                    Email = "admin@g2g.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Status = true,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Users.Add(admin);
                await _dbContext.SaveChangesAsync();

                _dbContext.UserRoles.Add(new UserRole
                {
                    UserId = admin.Id,
                    RoleId = adminRole.Id
                });

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("管理员账号初始化完成 - 用户名：admin, 密码：admin123");
            }
        }
    }
}
