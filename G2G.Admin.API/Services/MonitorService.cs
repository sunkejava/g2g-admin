using System.Diagnostics;
using G2G.Admin.API.Data;

namespace G2G.Admin.API.Services;

public interface IMonitorService
{
    Task<SystemInfoDto> GetSystemInfoAsync();
    Task<HealthCheckDto> GetHealthCheckAsync();
}

public class SystemInfoDto
{
    public double CpuUsage { get; set; }
    public long TotalMemory { get; set; }
    public long UsedMemory { get; set; }
    public double MemoryUsagePercent { get; set; }
    public long TotalDisk { get; set; }
    public long UsedDisk { get; set; }
    public double DiskUsagePercent { get; set; }
    public string OsVersion { get; set; } = string.Empty;
    public int ProcessorCount { get; set; }
    public DateTime Uptime { get; set; }
}

public class HealthCheckDto
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, ServiceHealth> Services { get; set; } = new();
}

public class ServiceHealth
{
    public bool IsHealthy { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class MonitorService : IMonitorService
{
    private readonly G2GDbContext _dbContext;
    private readonly ILogger<MonitorService> _logger;
    private static readonly Process _currentProcess = Process.GetCurrentProcess();

    public MonitorService(G2GDbContext dbContext, ILogger<MonitorService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public Task<SystemInfoDto> GetSystemInfoAsync()
    {
        var totalMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        var usedMemory = GC.GetTotalMemory(false);
        
        var cpuUsage = GetCpuUsage();
        var diskInfo = GetDiskInfo();

        var info = new SystemInfoDto
        {
            CpuUsage = cpuUsage,
            TotalMemory = totalMemory,
            UsedMemory = usedMemory,
            MemoryUsagePercent = Math.Round((double)usedMemory / totalMemory * 100, 2),
            TotalDisk = diskInfo.Total,
            UsedDisk = diskInfo.Used,
            DiskUsagePercent = Math.Round((double)diskInfo.Used / diskInfo.Total * 100, 2),
            OsVersion = Environment.OSVersion.ToString(),
            ProcessorCount = Environment.ProcessorCount,
            Uptime = _currentProcess.StartTime
        };

        return Task.FromResult(info);
    }

    public async Task<HealthCheckDto> GetHealthCheckAsync()
    {
        var result = new HealthCheckDto { IsHealthy = true, Status = "Healthy" };

        try
        {
            await _dbContext.Database.CanConnectAsync();
            result.Services["Database"] = new ServiceHealth { IsHealthy = true, Message = "Connected" };
        }
        catch (Exception ex)
        {
            result.Services["Database"] = new ServiceHealth { IsHealthy = false, Message = ex.Message };
            result.IsHealthy = false;
            result.Status = "Unhealthy";
        }

        var diskInfo = GetDiskInfo();
        var diskPercent = (double)diskInfo.Used / diskInfo.Total * 100;
        result.Services["Disk"] = new ServiceHealth 
        { 
            IsHealthy = diskPercent < 90, 
            Message = $"{diskPercent:F1}% used" 
        };

        if (!result.Services["Disk"].IsHealthy)
        {
            result.IsHealthy = false;
            result.Status = "Degraded";
        }

        return result;
    }

    private double GetCpuUsage()
    {
        try
        {
            _currentProcess.Refresh();
            var cpuTime = _currentProcess.TotalProcessorTime.TotalMilliseconds;
            var uptime = (DateTime.UtcNow - _currentProcess.StartTime.ToUniversalTime()).TotalMilliseconds;
            return Math.Round(cpuTime / uptime * 100 / Environment.ProcessorCount, 2);
        }
        catch
        {
            return 0;
        }
    }

    private (long Total, long Used) GetDiskInfo()
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(Directory.GetCurrentDirectory()));
            return (driveInfo.TotalSize, driveInfo.TotalSize - driveInfo.AvailableFreeSpace);
        }
        catch
        {
            return (0, 0);
        }
    }
}
