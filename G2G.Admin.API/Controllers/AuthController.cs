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
    private readonly ILogger<AuthController> _logger;
    private readonly LogHelper _logHelper;

    public AuthController(IAuthService authService, ILogger<AuthController> logger, LogHelper logHelper)
    {
        _authService = authService;
        _logger = logger;
        _logHelper = logHelper;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ip = GetClientIp();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _authService.LoginAsync(request, ip, userAgent);

        // 使用 LogHelper 记录登录日志
        await _logHelper.LogLoginAsync(request.Username, result != null, ip, userAgent);

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
        // 注册功能已关闭
        _logger.LogWarning("注册功能已关闭，IP: {IP} 尝试注册用户名：{Username}", GetClientIp(), request.Username);
        
        return BadRequest(new { 
            message = "注册功能已关闭，如需创建账号请联系管理员" 
        });
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


