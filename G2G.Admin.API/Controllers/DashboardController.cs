using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using G2G.Admin.API.Services;

namespace G2G.Admin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _dashboardService.GetStatsAsync();
        return Ok(stats);
    }

    [HttpGet("user-trend")]
    public async Task<IActionResult> GetUserTrend([FromQuery] int days = 7)
    {
        var trend = await _dashboardService.GetUserTrendAsync(days);
        return Ok(trend);
    }

    [HttpGet("log-distribution")]
    public async Task<IActionResult> GetLogDistribution()
    {
        var distribution = await _dashboardService.GetLogDistributionAsync();
        return Ok(distribution);
    }

    [HttpGet("version-distribution")]
    public async Task<IActionResult> GetVersionDistribution()
    {
        var distribution = await _dashboardService.GetVersionDistributionAsync();
        return Ok(distribution);
    }
}
