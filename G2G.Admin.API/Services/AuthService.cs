using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using G2G.Admin.API.Data;
using G2G.Admin.API.Entities;
using G2G.Admin.API.Models;
using Microsoft.EntityFrameworkCore;

namespace G2G.Admin.API.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, string ip, string userAgent);
    Task<User?> RegisterAsync(RegisterRequest request, string ip);
    string GenerateToken(User user);
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class AuthService : IAuthService
{
    private readonly G2GDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(G2GDbContext dbContext, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, string ip, string userAgent)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !user.Status)
        {
            _logger.LogWarning("登录失败：用户不存在或被禁用 - {Username}", request.Username);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("登录失败：密码错误 - {Username}", request.Username);
            return null;
        }

        var token = GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            }
        };
    }

    public async Task<User?> RegisterAsync(RegisterRequest request, string ip)
    {
        // 检查用户名是否已存在
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);
        
        if (existingUser != null)
        {
            throw new Exception("用户名或邮箱已存在");
        }

        // 创建用户
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Status = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // 查找"普通用户"角色并分配
        var commonUserRole = await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == "普通用户");
        
        if (commonUserRole != null)
        {
            _dbContext.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = commonUserRole.Id
            });
            
            // 立即保存
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("新用户 {Username} (ID:{UserId}) 已分配'普通用户'角色 (ID:{RoleId})", 
                request.Username, user.Id, commonUserRole.Id);
        }
        else
        {
            _logger.LogWarning("未找到'普通用户'角色，请检查数据初始化。当前角色：{Roles}", 
                string.Join(", ", await _dbContext.Roles.Select(r => r.Name).ToListAsync()));
        }

        return user;
    }

    public string GenerateToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = _dbContext.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(_dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .ToList();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
