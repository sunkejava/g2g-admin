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
    private static readonly PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    private DateTime _lastCpuReadTime = DateTime.MinValue;
    private double _lastCpuValue = 0;

    public MonitorService(G2GDbContext dbContext, ILogger<MonitorService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        
        // 初始化 CPU 计数器
        _cpuCounter.NextValue(); // 第一次调用返回 0，预热
    }

    public async Task<SystemInfoDto> GetSystemInfoAsync()
    {
        // 获取系统内存信息
        var totalMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        var usedMemory = GC.GetTotalMemory(false);
        
        // 获取 CPU 使用率
        var cpuUsage = GetCpuUsage();
        
        // 获取磁盘信息
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
            // 确保两次读取间隔至少 250ms
            var now = DateTime.Now;
            var timeSinceLastRead = (now - _lastCpuReadTime).TotalMilliseconds;
            
            if (timeSinceLastRead < 250)
            {
                return _lastCpuValue;
            }
            
            _lastCpuReadTime = now;
            _lastCpuValue = _cpuCounter.NextValue();
            
            // 限制最大值 100
            return Math.Min(Math.Round(_lastCpuValue, 2), 100);
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
