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

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
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
        var role = await _roleService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleDto dto)
    {
        var role = await _roleService.UpdateAsync(id, dto);
        if (role == null) return NotFound();
        return Ok(role);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _roleService.DeleteAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "删除成功" });
    }

    [HttpPost("{id}/menus")]
    public async Task<IActionResult> AssignMenus(int id, [FromBody] List<int> menuIds)
    {
        var result = await _roleService.AssignMenusAsync(id, menuIds);
        if (!result) return NotFound();
        return Ok(new { message = "菜单权限分配成功" });
    }

    [HttpGet("{id}/menus")]
    public async Task<IActionResult> GetMenus(int id)
    {
        var menus = await _roleService.GetMenusByRoleIdAsync(id);
        return Ok(menus);
    }
}
