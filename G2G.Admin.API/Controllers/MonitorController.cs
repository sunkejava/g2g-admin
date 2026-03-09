using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using G2G.Admin.API.Services;

namespace G2G.Admin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MonitorController : ControllerBase
{
    private readonly IMonitorService _monitorService;

    public MonitorController(IMonitorService monitorService)
    {
        _monitorService = monitorService;
    }

    [HttpGet("system")]
    public async Task<IActionResult> GetSystemInfo()
    {
        var info = await _monitorService.GetSystemInfoAsync();
        return Ok(info);
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHealthCheck()
    {
        var health = await _monitorService.GetHealthCheckAsync();
        return Ok(health);
    }
}
