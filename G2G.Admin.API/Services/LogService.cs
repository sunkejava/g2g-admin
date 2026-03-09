using Microsoft.EntityFrameworkCore;
using G2G.Admin.API.Data;
using G2G.Admin.API.Entities.Logs;

namespace G2G.Admin.API.Services;

public interface ILogService
{
    Task<PagedResult<OperationLog>> GetOperationLogsAsync(int page, int pageSize, DateTime? from, DateTime? to, string? keyword);
    Task<PagedResult<SystemLog>> GetSystemLogsAsync(int page, int pageSize, DateTime? from, DateTime? to, string? level, string? keyword);
    Task<PagedResult<LoginLog>> GetLoginLogsAsync(int page, int pageSize, DateTime? from, DateTime? to, string? keyword);
    Task<byte[]> ExportOperationLogsAsync(DateTime? from, DateTime? to);
    Task<byte[]> ExportSystemLogsAsync(DateTime? from, DateTime? to);
    Task<byte[]> ExportLoginLogsAsync(DateTime? from, DateTime? to);
    Task ArchiveOldLogsAsync(DateTime beforeDate);
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}

public class LogService : ILogService
{
    private readonly G2GDbContext _dbContext;

    public LogService(G2GDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<OperationLog>> GetOperationLogsAsync(int page, int pageSize, DateTime? from, DateTime? to, string? keyword)
    {
        var query = _dbContext.OperationLogs.AsQueryable();

        if (from.HasValue) query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(l => l.CreatedAt <= to.Value);
        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(l => l.Action.Contains(keyword) || l.Module.Contains(keyword) || (l.Details != null && l.Details.Contains(keyword)));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<OperationLog> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<SystemLog>> GetSystemLogsAsync(int page, int pageSize, DateTime? from, DateTime? to, string? level, string? keyword)
    {
        var query = _dbContext.SystemLogs.AsQueryable();

        if (from.HasValue) query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(l => l.CreatedAt <= to.Value);
        if (!string.IsNullOrEmpty(level)) query = query.Where(l => l.Level == level);
        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(l => l.Message.Contains(keyword) || l.Source.Contains(keyword));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<SystemLog> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<LoginLog>> GetLoginLogsAsync(int page, int pageSize, DateTime? from, DateTime? to, string? keyword)
    {
        var query = _dbContext.LoginLogs.AsQueryable();

        if (from.HasValue) query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(l => l.CreatedAt <= to.Value);
        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(l => l.Username.Contains(keyword) || (l.Ip != null && l.Ip.Contains(keyword)));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<LoginLog> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<byte[]> ExportOperationLogsAsync(DateTime? from, DateTime? to)
    {
        var query = _dbContext.OperationLogs.AsQueryable();
        if (from.HasValue) query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(l => l.CreatedAt <= to.Value);
        var logs = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();

        using var package = new OfficeOpenXml.ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("OperationLogs");
        
        worksheet.Cells[1, 1].Value = "时间";
        worksheet.Cells[1, 2].Value = "用户 ID";
        worksheet.Cells[1, 3].Value = "操作";
        worksheet.Cells[1, 4].Value = "模块";
        worksheet.Cells[1, 5].Value = "详情";
        worksheet.Cells[1, 6].Value = "IP";

        for (int i = 0; i < logs.Count; i++)
        {
            var log = logs[i];
            worksheet.Cells[i + 2, 1].Value = log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[i + 2, 2].Value = log.UserId;
            worksheet.Cells[i + 2, 3].Value = log.Action;
            worksheet.Cells[i + 2, 4].Value = log.Module;
            worksheet.Cells[i + 2, 5].Value = log.Details;
            worksheet.Cells[i + 2, 6].Value = log.Ip;
        }

        return package.GetAsByteArray();
    }

    public async Task<byte[]> ExportSystemLogsAsync(DateTime? from, DateTime? to)
    {
        var query = _dbContext.SystemLogs.AsQueryable();
        if (from.HasValue) query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(l => l.CreatedAt <= to.Value);
        var logs = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();

        using var package = new OfficeOpenXml.ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("SystemLogs");
        
        worksheet.Cells[1, 1].Value = "时间";
        worksheet.Cells[1, 2].Value = "级别";
        worksheet.Cells[1, 3].Value = "来源";
        worksheet.Cells[1, 4].Value = "消息";
        worksheet.Cells[1, 5].Value = "堆栈";

        for (int i = 0; i < logs.Count; i++)
        {
            var log = logs[i];
            worksheet.Cells[i + 2, 1].Value = log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[i + 2, 2].Value = log.Level;
            worksheet.Cells[i + 2, 3].Value = log.Source;
            worksheet.Cells[i + 2, 4].Value = log.Message;
            worksheet.Cells[i + 2, 5].Value = log.StackTrace;
        }

        return package.GetAsByteArray();
    }

    public async Task<byte[]> ExportLoginLogsAsync(DateTime? from, DateTime? to)
    {
        var query = _dbContext.LoginLogs.AsQueryable();
        if (from.HasValue) query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(l => l.CreatedAt <= to.Value);
        var logs = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();

        using var package = new OfficeOpenXml.ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("LoginLogs");
        
        worksheet.Cells[1, 1].Value = "时间";
        worksheet.Cells[1, 2].Value = "用户 ID";
        worksheet.Cells[1, 3].Value = "用户名";
        worksheet.Cells[1, 4].Value = "成功";
        worksheet.Cells[1, 5].Value = "IP";
        worksheet.Cells[1, 6].Value = "UserAgent";

        for (int i = 0; i < logs.Count; i++)
        {
            var log = logs[i];
            worksheet.Cells[i + 2, 1].Value = log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[i + 2, 2].Value = log.UserId;
            worksheet.Cells[i + 2, 3].Value = log.Username;
            worksheet.Cells[i + 2, 4].Value = log.Success ? "是" : "否";
            worksheet.Cells[i + 2, 5].Value = log.Ip;
            worksheet.Cells[i + 2, 6].Value = log.UserAgent;
        }

        return package.GetAsByteArray();
    }

    public async Task ArchiveOldLogsAsync(DateTime beforeDate)
    {
        var oldOperationLogs = _dbContext.OperationLogs.Where(l => l.CreatedAt < beforeDate);
        var oldSystemLogs = _dbContext.SystemLogs.Where(l => l.CreatedAt < beforeDate);
        var oldLoginLogs = _dbContext.LoginLogs.Where(l => l.CreatedAt < beforeDate);

        _dbContext.OperationLogs.RemoveRange(oldOperationLogs);
        _dbContext.SystemLogs.RemoveRange(oldSystemLogs);
        _dbContext.LoginLogs.RemoveRange(oldLoginLogs);

        await _dbContext.SaveChangesAsync();
    }
}
