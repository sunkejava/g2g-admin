using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using G2G.Admin.API.Services;
using G2G.Admin.API.Data;

namespace G2G.Admin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    private readonly LogHelper _logHelper;

    public UsersController(IUserService userService, ILogger<UsersController> logger, LogHelper logHelper)
    {
        _userService = userService;
        _logger = logger;
        _logHelper = logHelper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? keyword = null)
    {
        var users = await _userService.GetAllAsync(page, pageSize, keyword);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = await _userService.CreateAsync(dto);
            
            // 记录操作日志
            await _logHelper.LogOperationAsync(
                $"创建用户：{dto.Username}",
                "用户管理",
                $"邮箱：{dto.Email}, 角色数：{dto.RoleIds?.Count ?? 0}"
            );
            
            // 记录系统日志
            await _logHelper.LogSystemAsync(
                "Information",
                "UsersController",
                $"用户 {dto.Username} 创建成功"
            );
            
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建用户失败");
            
            // 记录系统错误日志
            await _logHelper.LogSystemAsync(
                "Error",
                "UsersController",
                $"创建用户 {dto.Username} 失败：{ex.Message}",
                ex.StackTrace
            );
            
            return BadRequest(new { message = "创建失败，用户名或邮箱可能已存在" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var user = await _userService.UpdateAsync(id, dto);
            if (user == null) return NotFound();
            
            // 记录操作日志
            await _logHelper.LogOperationAsync(
                $"更新用户：{user.Username} (ID:{id})",
                "用户管理",
                $"邮箱：{dto.Email}, 角色数：{dto.RoleIds?.Count ?? 0}"
            );
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            await _logHelper.LogSystemAsync(
                "Error",
                "UsersController",
                $"更新用户 ID:{id} 失败：{ex.Message}",
                ex.StackTrace
            );
            throw;
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _userService.DeleteAsync(id);
            if (!result) return NotFound();
            
            // 记录操作日志
            await _logHelper.LogOperationAsync(
                $"删除用户 (ID:{id})",
                "用户管理",
                $"用户 ID: {id}"
            );
            
            // 记录系统日志
            await _logHelper.LogSystemAsync(
                "Warning",
                "UsersController",
                $"用户 ID:{id} 已被删除"
            );
            
            return Ok(new { message = "删除成功" });
        }
        catch (Exception ex)
        {
            await _logHelper.LogSystemAsync(
                "Error",
                "UsersController",
                $"删除用户 ID:{id} 失败：{ex.Message}",
                ex.StackTrace
            );
            throw;
        }
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
    {
        try
        {
            var result = await _userService.ResetPasswordAsync(id, dto.NewPassword);
            if (!result) return NotFound();
            
            // 记录操作日志
            await _logHelper.LogOperationAsync(
                $"重置密码 (ID:{id})",
                "用户管理",
                $"用户 ID: {id}"
            );
            
            // 记录系统日志
            await _logHelper.LogSystemAsync(
                "Warning",
                "UsersController",
                $"用户 ID:{id} 密码已重置"
            );
            
            return Ok(new { message = "密码重置成功" });
        }
        catch (Exception ex)
        {
            await _logHelper.LogSystemAsync(
                "Error",
                "UsersController",
                $"重置用户 ID:{id} 密码失败：{ex.Message}",
                ex.StackTrace
            );
            throw;
        }
    }

    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        try
        {
            var result = await _userService.ToggleStatusAsync(id);
            if (!result) return NotFound();
            
            // 记录操作日志
            await _logHelper.LogOperationAsync(
                $"切换用户状态 (ID:{id})",
                "用户管理",
                $"用户 ID: {id}"
            );
            
            return Ok(new { message = "状态更新成功" });
        }
        catch (Exception ex)
        {
            await _logHelper.LogSystemAsync(
                "Error",
                "UsersController",
                $"切换用户 ID:{id} 状态失败：{ex.Message}",
                ex.StackTrace
            );
            throw;
        }
    }
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}
