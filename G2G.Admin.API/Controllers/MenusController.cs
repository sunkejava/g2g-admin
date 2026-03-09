using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using G2G.Admin.API.Services;

namespace G2G.Admin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenusController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenusController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var menus = await _menuService.GetAllAsync();
        return Ok(menus);
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetTree()
    {
        var tree = await _menuService.GetTreeAsync();
        return Ok(tree);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var menu = await _menuService.GetByIdAsync(id);
        if (menu == null) return NotFound();
        return Ok(menu);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuDto dto)
    {
        var menu = await _menuService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = menu.Id }, menu);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMenuDto dto)
    {
        var menu = await _menuService.UpdateAsync(id, dto);
        if (menu == null) return NotFound();
        return Ok(menu);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _menuService.DeleteAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "删除成功" });
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyMenus()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var menus = await _menuService.GetMenusByUserIdAsync(userId);
        return Ok(menus);
    }
}
