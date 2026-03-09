using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using G2G.Admin.API.Services;

namespace G2G.Admin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LogsController : ControllerBase
{
    private readonly ILogService _logService;

    public LogsController(ILogService logService)
    {
        _logService = logService;
    }

    [HttpGet("operation")]
    public async Task<IActionResult> GetOperationLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10, 
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string? keyword = null)
    {
        var result = await _logService.GetOperationLogsAsync(page, pageSize, from, to, keyword);
        return Ok(result);
    }

    [HttpGet("system")]
    public async Task<IActionResult> GetSystemLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string? level = null, [FromQuery] string? keyword = null)
    {
        var result = await _logService.GetSystemLogsAsync(page, pageSize, from, to, level, keyword);
        return Ok(result);
    }

    [HttpGet("login")]
    public async Task<IActionResult> GetLoginLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string? keyword = null)
    {
        var result = await _logService.GetLoginLogsAsync(page, pageSize, from, to, keyword);
        return Ok(result);
    }

    [HttpGet("export/operation")]
    public async Task<IActionResult> ExportOperationLogs([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var bytes = await _logService.ExportOperationLogsAsync(from, to);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"operation-logs-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }

    [HttpGet("export/system")]
    public async Task<IActionResult> ExportSystemLogs([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var bytes = await _logService.ExportSystemLogsAsync(from, to);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"system-logs-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }

    [HttpGet("export/login")]
    public async Task<IActionResult> ExportLoginLogs([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var bytes = await _logService.ExportLoginLogsAsync(from, to);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"login-logs-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }

    [HttpPost("archive")]
    public async Task<IActionResult> ArchiveLogs([FromBody] ArchiveLogsDto dto)
    {
        await _logService.ArchiveOldLogsAsync(dto.BeforeDate);
        return Ok(new { message = "归档完成" });
    }
}

public class ArchiveLogsDto
{
    public DateTime BeforeDate { get; set; }
}
