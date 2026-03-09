using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using G2G.Admin.API.Services;

namespace G2G.Admin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ISettingService _settingService;

    public SettingsController(ISettingService settingService)
    {
        _settingService = settingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var settings = await _settingService.GetAllAsync();
        return Ok(settings);
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var setting = await _settingService.GetByKeyAsync(key);
        if (setting == null) return NotFound();
        return Ok(setting);
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> Set(string key, [FromBody] SetSettingDto dto)
    {
        var setting = await _settingService.SetAsync(key, dto.Value, dto.Description);
        return Ok(setting);
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key)
    {
        var result = await _settingService.DeleteAsync(key);
        if (!result) return NotFound();
        return Ok(new { message = "删除成功" });
    }

    [HttpPost("cache/clear")]
    public async Task<IActionResult> ClearCache()
    {
        await _settingService.ClearCacheAsync();
        return Ok(new { message = "缓存已清除" });
    }
}

public class SetSettingDto
{
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}
