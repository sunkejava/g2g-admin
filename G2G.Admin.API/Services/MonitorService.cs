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
    private TimeSpan _lastTotalProcessorTime = TimeSpan.Zero;

    public MonitorService(G2GDbContext dbContext, ILogger<MonitorService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        InitializeCpu();
    }

    private void InitializeCpu()
    {
        _lastReadTime = DateTime.UtcNow;
        _lastTotalProcessorTime = _currentProcess.TotalProcessorTime;
    }

    public async Task<SystemInfoDto> GetSystemInfoAsync()
    {
        // 获取系统内存信息（跨平台）
        var (totalMemory, usedMemory, memoryPercent) = GetSystemMemoryInfo();
        
        // 获取 CPU 使用率
        var cpuUsage = GetCpuUsage();
        
        // 获取磁盘信息
        var diskInfo = GetDiskInfo();

        var info = new SystemInfoDto
        {
            CpuUsage = cpuUsage,
            TotalMemory = totalMemory,
            UsedMemory = usedMemory,
            MemoryUsagePercent = memoryPercent,
            TotalDisk = diskInfo.Total,
            UsedDisk = diskInfo.Used,
            DiskUsagePercent = diskInfo.Total > 0 ? Math.Round((double)diskInfo.Used / diskInfo.Total * 100, 2) : 0,
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
        var diskPercent = diskInfo.Total > 0 ? (double)diskInfo.Used / diskInfo.Total * 100 : 0;
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

    private (long total, long used, double percent) GetSystemMemoryInfo()
    {
        try
        {
            // 在 Linux 上读取 /proc/meminfo
            if (OperatingSystem.IsLinux())
            {
                var meminfoPath = "/proc/meminfo";
                if (File.Exists(meminfoPath))
                {
                    var lines = File.ReadAllLines(meminfoPath);
                    long totalKb = 0, freeKb = 0, buffersKb = 0, cachedKb = 0;
                    
                    foreach (var line in lines)
                    {
                        var parts = line.Split(':');
                        if (parts.Length != 2) continue;
                        
                        var key = parts[0].Trim();
                        var valuePart = parts[1].Trim().Split(' ')[0];
                        if (!long.TryParse(valuePart, out var value)) continue;
                        
                        switch (key)
                        {
                            case "MemTotal": totalKb = value; break;
                            case "MemFree": freeKb = value; break;
                            case "Buffers": buffersKb = value; break;
                            case "Cached": cachedKb = value; break;
                        }
                    }
                    
                    if (totalKb > 0)
                    {
                        // 可用内存 = 空闲 + 缓冲区 + 缓存
                        var availableKb = freeKb + buffersKb + cachedKb;
                        var usedKb = totalKb - availableKb;
                        var percent = Math.Round((double)usedKb / totalKb * 100, 2);
                        
                        return (totalKb * 1024, usedKb * 1024, percent);
                    }
                }
            }
            
            // 在 Windows 上或使用回退方案
            var totalMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
            var usedMemory = GC.GetTotalMemory(false);
            var percent = totalMemory > 0 ? Math.Round((double)usedMemory / totalMemory * 100, 2) : 0;
            
            return (totalMemory, usedMemory, percent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取系统内存信息失败");
            return (0, 0, 0);
        }
    }

    private double GetCpuUsage()
    {
        try
        {
            var now = DateTime.UtcNow;
            var currentTotalProcessorTime = _currentProcess.TotalProcessorTime;
            var timeDiff = (now - _lastReadTime).TotalMilliseconds;
            var cpuDiff = (currentTotalProcessorTime - _lastTotalProcessorTime).TotalMilliseconds;
            
            if (timeDiff <= 0) return 0;
            
            // 计算 CPU 使用率（考虑多核）
            var cpuUsage = cpuDiff / timeDiff * 100 / Environment.ProcessorCount;
            
            _lastReadTime = now;
            _lastTotalProcessorTime = currentTotalProcessorTime;
            
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
            var driveInfo = new DriveInfo(Path.GetPathRoot(Directory.GetCurrentDirectory()) ?? "/");
            return (driveInfo.TotalSize, driveInfo.TotalSize - driveInfo.AvailableFreeSpace);
        }
        catch
        {
            return (0, 0);
        }
    }
}
