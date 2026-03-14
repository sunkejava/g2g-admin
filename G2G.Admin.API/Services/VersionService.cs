using Microsoft.EntityFrameworkCore;
using G2G.Admin.API.Data;
using G2G.Admin.API.Entities;

namespace G2G.Admin.API.Services;

public interface IVersionService
{
    Task<PagedResult<AppVersion>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<AppVersion?> GetByIdAsync(int id);
    Task<AppVersion?> GetCurrentVersionAsync();
    Task<AppVersion> UploadAsync(UploadVersionDto dto, string filePath, string fileHash, long fileSize, int uploadedBy, string originalFileName);
    Task<bool> DeleteAsync(int id);
    Task<bool> RollbackAsync(int id);
    Task<VersionCheckResponse> CheckVersionAsync(int userId, string currentVersion);
    Task<VersionCompareResponse> CompareAsync(int id1, int id2);
}

public class UploadVersionDto
{
    public string VersionNo { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
}

public class VersionCheckResponse
{
    public bool HasUpdate { get; set; }
    public VersionInfo? LatestVersion { get; set; }
}

public class VersionInfo
{
    public string VersionNo { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty;
}

public class VersionCompareResponse
{
    public AppVersion Version1 { get; set; } = null!;
    public AppVersion Version2 { get; set; } = null!;
    public string Difference { get; set; } = string.Empty;
}

public class VersionService : IVersionService
{
    private readonly G2GDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public VersionService(G2GDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<PagedResult<AppVersion>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var query = _dbContext.Versions.AsQueryable();
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(v => v.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return new PagedResult<AppVersion> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<AppVersion?> GetByIdAsync(int id)
    {
        return await _dbContext.Versions.FindAsync(id);
    }

    public async Task<AppVersion?> GetCurrentVersionAsync()
    {
        return await _dbContext.Versions.FirstOrDefaultAsync(v => v.IsCurrent);
    }

    public async Task<AppVersion> UploadAsync(UploadVersionDto dto, string filePath, string fileHash, long fileSize, int uploadedBy, string originalFileName)
    {
        var currentVersion = await GetCurrentVersionAsync();
        if (currentVersion != null)
        {
            currentVersion.IsCurrent = false;
        }

        var version = new AppVersion
        {
            VersionNo = dto.VersionNo,
            FilePath = filePath,
            OriginalFileName = originalFileName,
            FileHash = fileHash,
            FileSize = fileSize,
            ReleaseNotes = dto.ReleaseNotes,
            IsCurrent = true,
            UploadedBy = uploadedBy
        };

        _dbContext.Versions.Add(version);
        await _dbContext.SaveChangesAsync();
        return version;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var version = await _dbContext.Versions.FindAsync(id);
        if (version == null) return false;
        if (version.IsCurrent) return false;

        var filePath = Path.Combine(_environment.ContentRootPath, version.FilePath);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        _dbContext.Versions.Remove(version);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RollbackAsync(int id)
    {
        var version = await _dbContext.Versions.FindAsync(id);
        if (version == null) return false;

        var currentVersion = await GetCurrentVersionAsync();
        if (currentVersion != null)
        {
            currentVersion.IsCurrent = false;
        }

        version.IsCurrent = true;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<VersionCheckResponse> CheckVersionAsync(int userId, string currentVersion)
    {
        var latestVersion = await GetCurrentVersionAsync();
        
        if (latestVersion == null)
        {
            return new VersionCheckResponse { HasUpdate = false };
        }

        var hasUpdate = latestVersion.VersionNo != currentVersion;
        
        return new VersionCheckResponse
        {
            HasUpdate = hasUpdate,
            LatestVersion = hasUpdate ? new VersionInfo
            {
                VersionNo = latestVersion.VersionNo,
                ReleaseNotes = latestVersion.ReleaseNotes,
                DownloadUrl = $"/api/versions/download/{latestVersion.Id}",
                FileSize = latestVersion.FileSize,
                FileHash = latestVersion.FileHash
            } : null
        };
    }

    public async Task<VersionCompareResponse> CompareAsync(int id1, int id2)
    {
        var version1 = await _dbContext.Versions.FindAsync(id1);
        var version2 = await _dbContext.Versions.FindAsync(id2);

        if (version1 == null || version2 == null)
        {
            throw new ArgumentException("版本不存在");
        }

        var differences = new List<string>();
        
        if (version1.VersionNo != version2.VersionNo)
            differences.Add($"版本号：{version1.VersionNo} → {version2.VersionNo}");
        if (version1.FileHash != version2.FileHash)
            differences.Add("文件内容已更改");
        if (version1.FileSize != version2.FileSize)
            differences.Add($"文件大小：{version1.FileSize} → {version2.FileSize} bytes");
        if (version1.ReleaseNotes != version2.ReleaseNotes)
            differences.Add("更新说明已更改");

        return new VersionCompareResponse
        {
            Version1 = version1,
            Version2 = version2,
            Difference = differences.Any() ? string.Join("\n", differences) : "无差异"
        };
    }
}
