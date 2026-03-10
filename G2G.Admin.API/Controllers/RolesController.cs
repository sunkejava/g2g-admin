using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using G2G.Admin.API.Services;

namespace G2G.Admin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;
    private readonly LogHelper _logHelper;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger, LogHelper logHelper)
    {
        _roleService = roleService;
        _logger = logger;
        _logHelper = logHelper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var roles = await _roleService.GetAllAsync(page, pageSize);
        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await _roleService.GetByIdAsync(id);
        if (role == null) return NotFound();
        return Ok(role);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        try
        {
            var role = await _roleService.CreateAsync(dto);
            
            // 记录操作日志
            await _logHelper.LogOperationAsync(
                $"创建角色：{dto.Name}",
                "角色管理",
                $"描述：{dto.Description}"
            );
            
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
        }
        catch (Exception ex)
        {
            await _logHelper.LogSystemAsync(
                "Error",
                "RolesController",
                $"创建角色 {dto.Name} 失败：{ex.Message}",
                ex.StackTrace
            );
            throw;
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleDto dto)
    {
        try
        {
            var role = await _roleService.UpdateAsync(id, dto);
            if (role == null) return NotFound();
            
            // 记录操作日志
            await _logHelper.LogOperationAsync(
                $"更新角色：{role.Name} (ID:{id})",
                "角色管理",
                $"新名称：{dto.Name ?? role.Name}, 新描述：{dto.Description ?? role.Description}"
            );
            
            return Ok(role);
        }
        catch (Exception ex)
        {
            await _logHelper.LogSystemAsync(
                "Error",
                "RolesController",
                $"更新角色 ID:{id} 失败：{ex.Message}",
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
            var result = await _roleService.DeleteAsync(id);
            if (!result) return NotFound();
            
            // 记录操作日志
            await _logHelper.LogOperationAsync(
                $"删除角色 (ID:{id})",
                "角色管理",
                $"角色 ID: {id}"
            );
            
            return Ok(new { message = "删除成功" });
        }
        catch (Exception ex)
        {
            await _logHelper.LogSystemAsync(
                "Error",
                "RolesController",
                $"删除角色 ID:{id} 失败：{ex.Message}",
                ex.StackTrace
            );
            throw;
        }
    }

    [HttpPost("{id}/menus")]
    public async Task<IActionResult> AssignMenus(int id, [FromBody] List<int> menuIds)
    {
        try
        {
            var result = await _roleService.AssignMenusAsync(id, menuIds);
            if (!result) return NotFound();
            
            // 记录操作日志
            await _logHelper.LogOperationAsync(
                $"分配角色权限 (ID:{id})",
                "角色管理",
                $"菜单数：{menuIds.Count}, 菜单 IDs: {string.Join(",", menuIds)}"
            );
            
            return Ok(new { message = "菜单权限分配成功" });
        }
        catch (Exception ex)
        {
            await _logHelper.LogSystemAsync(
                "Error",
                "RolesController",
                $"分配角色 ID:{id} 权限失败：{ex.Message}",
                ex.StackTrace
            );
            throw;
        }
    }

    [HttpGet("{id}/menus")]
    public async Task<IActionResult> GetMenus(int id)
    {
        var menus = await _roleService.GetMenusByRoleIdAsync(id);
        return Ok(menus);
    }
}
