using System.Diagnostics;
using System.Runtime.InteropServices;
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
    private DateTime _lastReadTime = DateTime.MinValue;
    private TimeSpan _lastTotalProcessorTime = TimeSpan.Zero;
    private System.Diagnostics.PerformanceCounter? _cpuCounter;

    public MonitorService(G2GDbContext dbContext, ILogger<MonitorService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        InitializeCpu();
        InitializeSystemCpuCounter();
    }

    private void InitializeCpu()
    {
        _lastReadTime = DateTime.UtcNow;
        _lastTotalProcessorTime = Process.GetCurrentProcess().TotalProcessorTime;
    }

    private void InitializeSystemCpuCounter()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                _cpuCounter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue(); // 第一次读取总是 0，预热一下
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "初始化 Windows CPU 性能计数器失败，将使用回退方案");
            }
        }
    }

    public async Task<SystemInfoDto> GetSystemInfoAsync()
    {
        var (totalMemory, usedMemory, memoryPercent) = GetSystemMemoryInfo();
        var cpuUsage = GetCpuUsage();
        var diskInfo = GetDiskInfo();

        return new SystemInfoDto
        {
            CpuUsage = cpuUsage,
            TotalMemory = totalMemory,
            UsedMemory = usedMemory,
            MemoryUsagePercent = memoryPercent,
            TotalDisk = diskInfo.Total,
            UsedDisk = diskInfo.Used,
            DiskUsagePercent = diskInfo.Total > 0 ? Math.Round((double)diskInfo.Used / diskInfo.Total * 100, 2) : 0,
            OsVersion = GetOsVersion(),
            ProcessorCount = Environment.ProcessorCount,
            Uptime = DateTime.UtcNow - TimeSpan.FromMilliseconds(Environment.TickCount64)
        };
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

    /// <summary>
    /// 跨平台获取系统内存信息
    /// 支持：Windows, Linux, macOS, Unix
    /// </summary>
    private (long total, long used, double percent) GetSystemMemoryInfo()
    {
        try
        {
            // Linux: 读取 /proc/meminfo
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetLinuxMemoryInfo();
            }
            
            // macOS: 使用 sysctl 命令
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return GetMacOSMemoryInfo();
            }
            
            // Windows: 使用 PerformanceCounter 或 GC 信息
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsMemoryInfo();
            }
            
            // 其他 Unix 系统：尝试通用方法
            return GetUnixMemoryInfo();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取系统内存信息失败，使用回退方案");
            return GetFallbackMemoryInfo();
        }
    }

    /// <summary>
    /// Linux 内存信息（读取 /proc/meminfo）
    /// </summary>
    private (long total, long used, double percent) GetLinuxMemoryInfo()
    {
        var meminfoPath = "/proc/meminfo";
        if (!File.Exists(meminfoPath))
        {
            return GetFallbackMemoryInfo();
        }

        var lines = File.ReadAllLines(meminfoPath);
        var memInfo = new Dictionary<string, long>();
        
        foreach (var line in lines)
        {
            var parts = line.Split(':');
            if (parts.Length != 2) continue;
            
            var key = parts[0].Trim();
            var valuePart = parts[1].Trim().Split(' ')[0];
            if (long.TryParse(valuePart, out var value))
            {
                memInfo[key] = value; // KB
            }
        }

        if (memInfo.TryGetValue("MemTotal", out var totalKb))
        {
            var freeKb = memInfo.GetValueOrDefault("MemFree", 0);
            var buffersKb = memInfo.GetValueOrDefault("Buffers", 0);
            var cachedKb = memInfo.GetValueOrDefault("Cached", 0);
            var availableKb = memInfo.GetValueOrDefault("MemAvailable", freeKb + buffersKb + cachedKb);
            
            var usedKb = totalKb - availableKb;
            var percent = Math.Round((double)usedKb / totalKb * 100, 2);
            
            return (totalKb * 1024, usedKb * 1024, percent);
        }

        return GetFallbackMemoryInfo();
    }

    /// <summary>
    /// macOS 内存信息（使用 sysctl 命令）
    /// </summary>
    private (long total, long used, double percent) GetMacOSMemoryInfo()
    {
        try
        {
            // 获取总内存
            var totalMemory = ExecuteCommand("sysctl", "-n", "hw.memsize");
            if (long.TryParse(totalMemory, out var total))
            {
                // 获取活跃内存（已使用）
                var usedMemoryStr = ExecuteCommand("vm_stat");
                var pageSize = 4096; // macOS 默认页大小
                
                // 解析 vm_stat 输出
                var activePages = ParseVmStat(usedMemoryStr, "active");
                var wiredPages = ParseVmStat(usedMemoryStr, "wired");
                var compressedPages = ParseVmStat(usedMemoryStr, "compressed");
                
                var used = (activePages + wiredPages + compressedPages) * pageSize;
                var percent = Math.Round((double)used / total * 100, 2);
                
                return (total, used, percent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取 macOS 内存信息失败");
        }

        return GetFallbackMemoryInfo();
    }

    /// <summary>
    /// Windows 内存信息
    /// </summary>
    private (long total, long used, double percent) GetWindowsMemoryInfo()
    {
        try
        {
            var gcInfo = GC.GetGCMemoryInfo();
            var totalMemory = gcInfo.TotalAvailableMemoryBytes;
            var usedMemory = GC.GetTotalMemory(false);
            
            // 更准确的 Windows 内存信息（使用 PerformanceCounter）
            try
            {
                using var totalRamCounter = new PerformanceCounter("Memory", "Available Bytes");
                var availableBytes = totalRamCounter.NextValue();
                var totalBytes = totalMemory;
                var usedBytes = totalBytes - availableBytes;
                var percent = Math.Round((double)usedBytes / totalBytes * 100, 2);
                
                return (totalBytes, (long)usedBytes, percent);
            }
            catch
            {
                // PerformanceCounter 失败时使用 GC 信息
                var percent = totalMemory > 0 ? Math.Round((double)usedMemory / totalMemory * 100, 2) : 0;
                return (totalMemory, usedMemory, percent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取 Windows 内存信息失败");
            return GetFallbackMemoryInfo();
        }
    }

    /// <summary>
    /// 其他 Unix 系统内存信息
    /// </summary>
    private (long total, long used, double percent) GetUnixMemoryInfo()
    {
        try
        {
            // 尝试使用 free 命令（适用于大多数 Unix 系统）
            var freeOutput = ExecuteCommand("free", "-b");
            var lines = freeOutput.Split('\n');
            
            foreach (var line in lines)
            {
                if (line.StartsWith("Mem:"))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3 && 
                        long.TryParse(parts[1], out var total) && 
                        long.TryParse(parts[2], out var used))
                    {
                        var percent = Math.Round((double)used / total * 100, 2);
                        return (total, used, percent);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取 Unix 内存信息失败");
        }

        return GetFallbackMemoryInfo();
    }

    /// <summary>
    /// 回退方案：使用 GC 信息
    /// </summary>
    private (long total, long used, double percent) GetFallbackMemoryInfo()
    {
        var gcInfo = GC.GetGCMemoryInfo();
        var totalMemory = gcInfo.TotalAvailableMemoryBytes;
        var usedMemory = GC.GetTotalMemory(false);
        var percent = totalMemory > 0 ? Math.Round((double)usedMemory / totalMemory * 100, 2) : 0;
        
        return (totalMemory, usedMemory, percent);
    }

    /// <summary>
    /// 执行 shell 命令
    /// </summary>
    private string ExecuteCommand(string command, params string[] args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = string.Join(" ", args),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        var output = process?.StandardOutput.ReadToEnd() ?? "";
        process?.WaitForExit(5000);
        
        return output.Trim();
    }

    /// <summary>
    /// 解析 vm_stat 输出
    /// </summary>
    private long ParseVmStat(string output, string key)
    {
        foreach (var line in output.Split('\n'))
        {
            if (line.Contains(key + " pages", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split(':');
                if (parts.Length == 2 && long.TryParse(parts[1].Trim(), out var pages))
                {
                    return pages;
                }
            }
        }
        return 0;
    }

    /// <summary>
    /// 获取 CPU 使用率（跨平台）
    /// </summary>
    private double GetCpuUsage()
    {
        // Windows: 使用 PerformanceCounter 获取系统 CPU 使用率
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && _cpuCounter != null)
        {
            try
            {
                var cpuUsage = _cpuCounter.NextValue();
                return Math.Min(Math.Round(cpuUsage, 2), 100);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "读取 Windows CPU 性能计数器失败");
                // 回退到进程级计算
            }
        }
        
        // Linux/macOS: 读取 /proc/stat 或使用回退方案
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                return GetLinuxCpuUsage();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "读取 Linux CPU 使用率失败");
            }
        }
        
        // 回退方案：基于当前进程 CPU 时间计算（不准确但可用）
        try
        {
            var now = DateTime.UtcNow;
            var process = Process.GetCurrentProcess();
            var currentTotalProcessorTime = process.TotalProcessorTime;
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

    /// <summary>
    /// Linux CPU 使用率（读取 /proc/stat）
    /// </summary>
    private double GetLinuxCpuUsage()
    {
        var statPath = "/proc/stat";
        if (!File.Exists(statPath))
        {
            return 0;
        }

        var line = File.ReadLines(statPath).FirstOrDefault(l => l.StartsWith("cpu "));
        if (string.IsNullOrEmpty(line))
        {
            return 0;
        }

        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 5)
        {
            return 0;
        }

        // cpu user nice system idle iowait irq softirq
        var user = ulong.Parse(parts[1]);
        var nice = ulong.Parse(parts[2]);
        var system = ulong.Parse(parts[3]);
        var idle = ulong.Parse(parts[4]);
        var iowait = parts.Length > 5 ? ulong.Parse(parts[5]) : 0;
        var irq = parts.Length > 6 ? ulong.Parse(parts[6]) : 0;
        var softirq = parts.Length > 7 ? ulong.Parse(parts[7]) : 0;

        var total = user + nice + system + idle + iowait + irq + softirq;
        var idleTotal = idle + iowait;

        // 计算与上次读取的差值
        var now = DateTime.UtcNow;
        if (_lastReadTime == DateTime.MinValue)
        {
            _lastReadTime = now;
            _lastTotalProcessorTime = TimeSpan.FromTicks((long)(total - idleTotal));
            return 0;
        }

        var prevTotal = _lastTotalProcessorTime.Ticks;
        var prevIdle = (total - idleTotal - (ulong)(prevTotal / TimeSpan.TicksPerMillisecond * 10)); // 近似值

        _lastReadTime = now;
        _lastTotalProcessorTime = TimeSpan.FromTicks((long)(total - idleTotal));

        var totalDiff = total - prevIdle - (ulong)(prevTotal / TimeSpan.TicksPerMillisecond * 10);
        var idleDiff = idleTotal - prevIdle;

        if (totalDiff == 0) return 0;

        var cpuUsage = (1.0 - (double)idleDiff / totalDiff) * 100.0;
        return Math.Min(Math.Round(cpuUsage, 2), 100);
    }

    /// <summary>
    /// 获取磁盘信息（跨平台）
    /// </summary>
    private (long Total, long Used) GetDiskInfo()
    {
        try
        {
            var rootPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? Path.GetPathRoot(Directory.GetCurrentDirectory()) 
                : "/";
            
            var driveInfo = new DriveInfo(rootPath ?? "/");
            return (driveInfo.TotalSize, driveInfo.TotalSize - driveInfo.AvailableFreeSpace);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取磁盘信息失败");
            return (0, 0);
        }
    }

    /// <summary>
    /// 获取操作系统版本（跨平台）
    /// </summary>
    private string GetOsVersion()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $"Windows {Environment.OSVersion.VersionString}";
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var osRelease = "/etc/os-release";
                if (File.Exists(osRelease))
                {
                    var lines = File.ReadAllLines(osRelease);
                    var name = lines.FirstOrDefault(l => l.StartsWith("PRETTY_NAME="));
                    if (name != null)
                    {
                        return name.Split('=')[1].Trim('"');
                    }
                }
                return $"Linux {Environment.OSVersion.VersionString}";
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var version = ExecuteCommand("sw_vers", "-productVersion");
                return $"macOS {version}";
            }
            return Environment.OSVersion.ToString();
        }
        catch
        {
            return Environment.OSVersion.ToString();
        }
    }
}
