using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using G2G.Admin.API.Models;
using G2G.Admin.API.Services;
using G2G.Admin.API.Data;
using G2G.Admin.API.Entities.Logs;

namespace G2G.Admin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly G2GDbContext _dbContext;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, G2GDbContext dbContext, ILogger<AuthController> logger)
    {
        _authService = authService;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ip = GetClientIp();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _authService.LoginAsync(request, ip, userAgent);

        var loginLog = new LoginLog
        {
            Username = request.Username,
            Success = result != null,
            Ip = ip,
            UserAgent = userAgent
        };

        if (result != null)
        {
            loginLog.UserId = result.User.Id;
            _logger.LogInformation("用户登录成功：{Username}", request.Username);
        }
        else
        {
            _logger.LogWarning("用户登录失败：{Username}", request.Username);
        }

        _dbContext.LoginLogs.Add(loginLog);
        await _dbContext.SaveChangesAsync();

        if (result == null)
        {
            return Unauthorized(new { message = "用户名或密码错误" });
        }

        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var ip = GetClientIp();
            var result = await _authService.RegisterAsync(request, ip);
            
            _logger.LogInformation("用户注册成功：{Username}", request.Username);
            return Ok(new { message = "注册成功，请登录" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户注册失败：{Username}", request.Username);
            return BadRequest(new { message = "注册失败，用户名或邮箱可能已存在" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(new { message = "登出成功" });
    }

    private string GetClientIp()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }
        return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Password { get; set; } = string.Empty;
}
