using Microsoft.EntityFrameworkCore;
using G2G.Admin.API.Entities;
using G2G.Admin.API.Entities.Logs;

namespace G2G.Admin.API.Data;

public class G2GDbContext : DbContext
{
    public G2GDbContext(DbContextOptions<G2GDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RoleMenu> RoleMenus => Set<RoleMenu>();
    public DbSet<AppVersion> Versions => Set<AppVersion>();
    public DbSet<OperationLog> OperationLogs => Set<OperationLog>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();
    public DbSet<Setting> Settings => Set<Setting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User 配置
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        // Role 配置
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        });

        // Menu 配置
        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        });

        // UserRole 配置 (多对多)
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasOne(e => e.User).WithMany(u => u.UserRoles).HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Role).WithMany(r => r.UserRoles).HasForeignKey(e => e.RoleId);
        });

        // RoleMenu 配置 (多对多)
        modelBuilder.Entity<RoleMenu>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.MenuId });
            entity.HasOne(e => e.Role).WithMany(r => r.RoleMenus).HasForeignKey(e => e.RoleId);
            entity.HasOne(e => e.Menu).WithMany(m => m.RoleMenus).HasForeignKey(e => e.MenuId);
        });

        // AppVersion 配置
        modelBuilder.Entity<AppVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VersionNo).IsUnique();
            entity.Property(e => e.VersionNo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FilePath).IsRequired();
            entity.Property(e => e.FileHash).IsRequired();
        });

        // OperationLog 配置
        modelBuilder.Entity<OperationLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Module).IsRequired().HasMaxLength(100);
        });

        // SystemLog 配置
        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Level);
            entity.Property(e => e.Level).IsRequired().HasMaxLength(20);
        });

        // LoginLog 配置
        modelBuilder.Entity<LoginLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Username);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
        });

        // Setting 配置
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
        });

        // 初始化数据 - 默认管理员账户 (密码：admin123)
        var adminPassword = BCrypt.Net.BCrypt.HashPassword("admin123");
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "admin", Email = "admin@g2g.com", PasswordHash = adminPassword, Status = true, CreatedAt = DateTime.UtcNow }
        );

        // 初始化数据 - 默认角色
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "系统管理员", CreatedAt = DateTime.UtcNow },
            new Role { Id = 2, Name = "User", Description = "普通用户", CreatedAt = DateTime.UtcNow }
        );

        // 初始化数据 - 默认菜单
        modelBuilder.Entity<Menu>().HasData(
            new Menu { Id = 1, Code = "dashboard", Name = "首页", Path = "/dashboard", Icon = "Home", Sort = 1 },
            new Menu { Id = 2, Code = "users", Name = "用户管理", Path = "/users", Icon = "User", Sort = 2 },
            new Menu { Id = 3, Code = "roles", Name = "角色管理", Path = "/roles", Icon = "Lock", Sort = 3 },
            new Menu { Id = 4, Code = "versions", Name = "版本管理", Path = "/versions", Icon = "Upload", Sort = 4 },
            new Menu { Id = 5, Code = "logs", Name = "日志管理", Path = "/logs", Icon = "FileText", Sort = 5 },
            new Menu { Id = 6, Code = "settings", Name = "系统配置", Path = "/settings", Icon = "Settings", Sort = 6 },
            new Menu { Id = 7, Code = "monitor", Name = "监控面板", Path = "/monitor", Icon = "Activity", Sort = 7 }
        );

        // 初始化数据 - 管理员角色分配所有菜单
        for (int i = 1; i <= 7; i++)
        {
            modelBuilder.Entity<RoleMenu>().HasData(
                new RoleMenu { RoleId = 1, MenuId = i }
            );
        }

        // 初始化数据 - 管理员用户分配管理员角色
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { UserId = 1, RoleId = 1 }
        );
    }
}
