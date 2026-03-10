using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using G2G.Admin.API.Services;
using G2G.Admin.API.Entities;

namespace G2G.Admin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VersionsController : ControllerBase
{
    private readonly IVersionService _versionService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<VersionsController> _logger;
    private readonly LogHelper _logHelper;

    public VersionsController(IVersionService versionService, IWebHostEnvironment environment, ILogger<VersionsController> logger, LogHelper logHelper)
    {
        _versionService = versionService;
        _environment = environment;
        _logger = logger;
        _logHelper = logHelper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var versions = await _versionService.GetAllAsync(page, pageSize);
        return Ok(versions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var version = await _versionService.GetByIdAsync(id);
        if (version == null) return NotFound();
        return Ok(version);
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent()
    {
        var version = await _versionService.GetCurrentVersionAsync();
        if (version == null) return NotFound();
        return Ok(version);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string versionNo, [FromForm] string releaseNotes)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var uploadedBy = userIdClaim != null && int.TryParse(userIdClaim.Value, out var id) ? id : 0;

            // 保存文件
            var uploadsDir = Path.Combine(_environment.ContentRootPath, "uploads", "versions");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{versionNo}_{DateTime.UtcNow:yyyyMMddHHmmss}_{file.FileName}";
            var filePath = Path.Combine("uploads", "versions", fileName);
            var fullPath = Path.Combine(_environment.ContentRootPath, filePath);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 计算文件 Hash
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var fileStream = System.IO.File.OpenRead(fullPath))
                {
                    var hashBytes = md5.ComputeHash(fileStream);
                    var fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                    var dto = new UploadVersionDto { VersionNo = versionNo, ReleaseNotes = releaseNotes };
                    var version = await _versionService.UploadAsync(dto, filePath, fileHash, file.Length, uploadedBy);

                    // 记录操作日志
                    await _logHelper.LogOperationAsync(
                        $"上传版本：{versionNo}",
                        "版本管理",
                        $"文件大小：{file.Length} bytes, Hash: {fileHash}"
                    );
                    
                    // 记录系统日志
                    await _logHelper.LogSystemAsync(
                        "Information",
                        "VersionsController",
                        $"版本 {versionNo} 上传成功，文件大小：{file.Length / 1024.0:F2} KB"
                    );

                    _logger.LogInformation("版本上传成功：{VersionNo}", versionNo);
                    return Ok(version);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "版本上传失败");
            
            // 记录系统错误日志
            await _logHelper.LogSystemAsync(
                "Error",
                "VersionsController",
                $"上传版本 {versionNo} 失败：{ex.Message}",
                ex.StackTrace
            );
            
            return StatusCode(500, new { message = "上传失败" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _versionService.DeleteAsync(id);
            if (!result) return NotFound(new { message = "无法删除当前版本" });
            
            // 记录操作日志
            await _logHelper.LogOperationAsync(
                $"删除版本 (ID:{id})",
                "版本管理",
                $"版本 ID: {id}"
            );
            
            return Ok(new { message = "删除成功" });
        }
        catch (Exception ex)
        {
            await _logHelper.LogSystemAsync(
                "Error",
                "VersionsController",
                $"删除版本 ID:{id} 失败：{ex.Message}",
                ex.StackTrace
            );
            throw;
        }
    }

    [HttpPost("{id}/rollback")]
    public async Task<IActionResult> Rollback(int id)
    {
        try
        {
            var result = await _versionService.RollbackAsync(id);
            if (!result) return NotFound();
            
            // 记录操作日志
            await _logHelper.LogOperationAsync(
                $"回滚版本 (ID:{id})",
                "版本管理",
                $"版本 ID: {id}"
            );
            
            // 记录系统日志
            await _logHelper.LogSystemAsync(
                "Warning",
                "VersionsController",
                $"系统已回滚到版本 ID:{id}"
            );
            
            return Ok(new { message = "回滚成功" });
        }
        catch (Exception ex)
        {
            await _logHelper.LogSystemAsync(
                "Error",
                "VersionsController",
                $"回滚版本 ID:{id} 失败：{ex.Message}",
                ex.StackTrace
            );
            throw;
        }
    }

    [HttpGet("check/{currentVersion}")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckVersion(string currentVersion)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var userId = userIdClaim != null && int.TryParse(userIdClaim.Value, out var id) ? id : 0;

        var result = await _versionService.CheckVersionAsync(userId, currentVersion);
        return Ok(result);
    }

    [HttpGet("compare/{id1}/{id2}")]
    public async Task<IActionResult> Compare(int id1, int id2)
    {
        try
        {
            var result = await _versionService.CompareAsync(id1, id2);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var version = await _versionService.GetByIdAsync(id);
        if (version == null) return NotFound();

        var fullPath = Path.Combine(_environment.ContentRootPath, version.FilePath);
        if (!System.IO.File.Exists(fullPath)) return NotFound();

        // 记录下载日志
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var userId = userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid) ? uid : 0;
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
        
        _logger.LogInformation("用户 {Username} (ID={UserId}) 下载版本 {VersionNo}", username, userId, version.VersionNo);

        return PhysicalFile(fullPath, "application/octet-stream", $"{version.VersionNo}.zip");
    }
}
