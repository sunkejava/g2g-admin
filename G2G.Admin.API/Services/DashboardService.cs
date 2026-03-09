using Microsoft.EntityFrameworkCore;
using G2G.Admin.API.Data;
using G2G.Admin.API.Entities;

namespace G2G.Admin.API.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
    Task<UserTrendDto> GetUserTrendAsync(int days = 7);
    Task<LogDistributionDto> GetLogDistributionAsync();
    Task<VersionDistributionDto> GetVersionDistributionAsync();
}

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalRoles { get; set; }
    public int TotalVersions { get; set; }
    public int TotalOperationLogs { get; set; }
    public int TotalLoginLogs { get; set; }
    public int TotalSystemLogs { get; set; }
}

public class UserTrendDto
{
    public List<string> Dates { get; set; } = new();
    public List<int> Counts { get; set; } = new();
}

public class LogDistributionDto
{
    public int OperationLogs { get; set; }
    public int SystemLogs { get; set; }
    public int LoginLogs { get; set; }
    public int FailedLogins { get; set; }
}

public class VersionDistributionDto
{
    public List<VersionCountDto> Versions { get; set; } = new();
}

public class VersionCountDto
{
    public string VersionNo { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DashboardService : IDashboardService
{
    private readonly G2GDbContext _dbContext;

    public DashboardService(G2GDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var now = DateTime.UtcNow;
        var today = new DateTime(now.Year, now.Month, now.Day);

        return new DashboardStatsDto
        {
            TotalUsers = await _dbContext.Users.CountAsync(),
            ActiveUsers = await _dbContext.Users.CountAsync(u => u.Status),
            TotalRoles = await _dbContext.Roles.CountAsync(),
            TotalVersions = await _dbContext.Versions.CountAsync(),
            TotalOperationLogs = await _dbContext.OperationLogs.CountAsync(),
            TotalLoginLogs = await _dbContext.LoginLogs.CountAsync(),
            TotalSystemLogs = await _dbContext.SystemLogs.CountAsync()
        };
    }

    public async Task<UserTrendDto> GetUserTrendAsync(int days = 7)
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddDays(-days + 1);
        
        var dates = new List<string>();
        var counts = new List<int>();

        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var nextDate = date.AddDays(1);
            
            dates.Add(date.ToString("yyyy-MM-dd"));
            
            var count = await _dbContext.Users.CountAsync(u => 
                u.CreatedAt >= date && u.CreatedAt < nextDate);
            counts.Add(count);
        }

        return new UserTrendDto { Dates = dates, Counts = counts };
    }

    public async Task<LogDistributionDto> GetLogDistributionAsync()
    {
        return new LogDistributionDto
        {
            OperationLogs = await _dbContext.OperationLogs.CountAsync(),
            SystemLogs = await _dbContext.SystemLogs.CountAsync(),
            LoginLogs = await _dbContext.LoginLogs.CountAsync(),
            FailedLogins = await _dbContext.LoginLogs.CountAsync(l => !l.Success)
        };
    }

    public async Task<VersionDistributionDto> GetVersionDistributionAsync()
    {
        var versions = await _dbContext.Versions
            .OrderByDescending(v => v.UploadedAt)
            .Take(10)
            .Select(v => new VersionCountDto
            {
                VersionNo = v.VersionNo,
                Count = 1
            })
            .ToListAsync();

        return new VersionDistributionDto { Versions = versions };
    }
}
