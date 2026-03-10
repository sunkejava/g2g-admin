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
    private DateTime _lastReadTime = DateTime.MinValue;
    private long _lastCpuTicks = 0;

    public MonitorService(G2GDbContext dbContext, ILogger<MonitorService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        InitializeCpu();
    }

    private void InitializeCpu()
    {
        _lastReadTime = DateTime.UtcNow;
        _lastCpuTicks = _currentProcess.TotalProcessorTime.Ticks;
    }

    public async Task<SystemInfoDto> GetSystemInfoAsync()
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
            Uptime = DateTime.UtcNow - TimeSpan.FromMilliseconds(Environment.TickCount64)
        };

        return info;
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
            var now = DateTime.UtcNow;
            var currentCpuTicks = _currentProcess.TotalProcessorTime.Ticks;
            var timeDiff = (now - _lastReadTime).Ticks;
            var cpuDiff = currentCpuTicks - _lastCpuTicks;
            
            if (timeDiff <= 0) return 0;
            
            // 计算 CPU 使用率（考虑多核）
            var cpuUsage = (double)cpuDiff / timeDiff * 100 / Environment.ProcessorCount;
            
            _lastReadTime = now;
            _lastCpuTicks = currentCpuTicks;
            
            return Math.Min(Math.Round(cpuUsage, 2), 100);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取 CPU 使用率失败");
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
