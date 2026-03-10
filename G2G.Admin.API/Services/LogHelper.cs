using System.Security.Claims;
using G2G.Admin.API.Data;
using G2G.Admin.API.Entities.Logs;
using Microsoft.AspNetCore.Http;

namespace G2G.Admin.API.Services;

public interface ILogHelper
{
    Task LogOperationAsync(string action, string module, string? details = null);
    Task LogSystemAsync(string level, string source, string message, string? stackTrace = null);
    Task LogLoginAsync(string username, bool success, string? ip = null, string? userAgent = null);
}

public class LogHelper : ILogHelper
{
    private readonly G2GDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LogHelper> _logger;

    public LogHelper(G2GDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<LogHelper> logger)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// 记录操作日志
    /// </summary>
    /// <param name="action">操作类型（如：创建用户、删除角色）</param>
    /// <param name="module">所属模块（如：用户管理、角色管理）</param>
    /// <param name="details">操作详情</param>
    public async Task LogOperationAsync(string action, string module, string? details = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var ip = GetClientIp();

            var log = new OperationLog
            {
                UserId = userId,
                Action = action,
                Module = module,
                Details = details,
                Ip = ip,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.OperationLogs.Add(log);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("操作日志：{Module} - {Action} by User {UserId}", module, action, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录操作日志失败");
        }
    }

    /// <summary>
    /// 记录系统日志
    /// </summary>
    /// <param name="level">日志级别（Information, Warning, Error, Critical）</param>
    /// <param name="source">日志来源（如：UserService, AuthController）</param>
    /// <param name="message">日志消息</param>
    /// <param name="stackTrace">错误堆栈（仅错误日志）</param>
    public async Task LogSystemAsync(string level, string source, string message, string? stackTrace = null)
    {
        try
        {
            var log = new SystemLog
            {
                Level = level,
                Source = source,
                Message = message,
                StackTrace = stackTrace,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.SystemLogs.Add(log);
            await _dbContext.SaveChangesAsync();

            // 同时记录到 Serilog
            switch (level.ToUpper())
            {
                case "ERROR":
                case "CRITICAL":
                    _logger.LogError("{Source} - {Message}", source, message);
                    break;
                case "WARNING":
                    _logger.LogWarning("{Source} - {Message}", source, message);
                    break;
                default:
                    _logger.LogInformation("{Source} - {Message}", source, message);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录系统日志失败");
        }
    }

    /// <summary>
    /// 记录登录日志
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="success">是否成功</param>
    /// <param name="ip">IP 地址</param>
    /// <param name="userAgent">UserAgent</param>
    public async Task LogLoginAsync(string username, bool success, string? ip = null, string? userAgent = null)
    {
        try
        {
            var userId = GetCurrentUserId();

            var log = new LoginLog
            {
                UserId = userId > 0 ? userId : null,
                Username = username,
                Success = success,
                Ip = ip ?? GetClientIp(),
                UserAgent = userAgent ?? _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.LoginLogs.Add(log);
            await _dbContext.SaveChangesAsync();

            if (success)
            {
                _logger.LogInformation("登录成功：{Username}", username);
            }
            else
            {
                _logger.LogWarning("登录失败：{Username}", username);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录登录日志失败");
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return 0;
    }

    private string? GetClientIp()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }
        return context.Connection.RemoteIpAddress?.ToString();
    }
}
