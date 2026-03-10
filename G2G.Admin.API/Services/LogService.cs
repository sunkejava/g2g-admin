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
        var worksheet = package.Workbook.Worksheets.Add("操作日志");
        
        // 设置表头样式
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Font.Size = 12;
        headerRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        headerRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(102, 126, 234));
        headerRow.Style.Font.Color.SetColor(System.Drawing.Color.White);
        
        // 设置列宽
        worksheet.Column(1).Width = 20;
        worksheet.Column(2).Width = 10;
        worksheet.Column(3).Width = 15;
        worksheet.Column(4).Width = 12;
        worksheet.Column(5).Width = 40;
        worksheet.Column(6).Width = 15;
        
        // 表头
        worksheet.Cells[1, 1].Value = "时间";
        worksheet.Cells[1, 2].Value = "用户 ID";
        worksheet.Cells[1, 3].Value = "操作";
        worksheet.Cells[1, 4].Value = "模块";
        worksheet.Cells[1, 5].Value = "详情";
        worksheet.Cells[1, 6].Value = "IP 地址";

        // 数据行
        for (int i = 0; i < logs.Count; i++)
        {
            var log = logs[i];
            var row = i + 2;
            worksheet.Cells[row, 1].Value = log.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[row, 2].Value = log.UserId;
            worksheet.Cells[row, 3].Value = log.Action;
            worksheet.Cells[row, 4].Value = log.Module;
            worksheet.Cells[row, 5].Value = log.Details ?? "";
            worksheet.Cells[row, 6].Value = log.Ip ?? "";
            
            // 隔行换色
            if (row % 2 == 0)
            {
                worksheet.Row(row).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Row(row).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(248, 249, 255));
            }
        }
        
        // 添加统计信息
        var summaryRow = logs.Count + 3;
        worksheet.Cells[summaryRow, 1].Value = $"导出时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        worksheet.Cells[summaryRow, 2].Value = $"总记录数：{logs.Count}";
        if (from.HasValue)
        {
            worksheet.Cells[summaryRow, 3].Value = $"开始日期：{from.Value:yyyy-MM-dd}";
        }
        if (to.HasValue)
        {
            worksheet.Cells[summaryRow, 4].Value = $"结束日期：{to.Value:yyyy-MM-dd}";
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
        var worksheet = package.Workbook.Worksheets.Add("系统日志");
        
        // 设置表头样式
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Font.Size = 12;
        headerRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        headerRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(102, 126, 234));
        headerRow.Style.Font.Color.SetColor(System.Drawing.Color.White);
        
        // 设置列宽
        worksheet.Column(1).Width = 20;
        worksheet.Column(2).Width = 10;
        worksheet.Column(3).Width = 15;
        worksheet.Column(4).Width = 50;
        worksheet.Column(5).Width = 40;
        
        // 表头
        worksheet.Cells[1, 1].Value = "时间";
        worksheet.Cells[1, 2].Value = "级别";
        worksheet.Cells[1, 3].Value = "来源";
        worksheet.Cells[1, 4].Value = "消息";
        worksheet.Cells[1, 5].Value = "堆栈";

        // 数据行
        for (int i = 0; i < logs.Count; i++)
        {
            var log = logs[i];
            var row = i + 2;
            worksheet.Cells[row, 1].Value = log.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[row, 2].Value = log.Level;
            worksheet.Cells[row, 3].Value = log.Source;
            worksheet.Cells[row, 4].Value = log.Message;
            worksheet.Cells[row, 5].Value = log.StackTrace ?? "";
            
            // 根据日志级别设置颜色
            var levelCell = worksheet.Cells[row, 2];
            if (log.Level == "Error" || log.Level == "Critical")
            {
                levelCell.Style.Font.Color.SetColor(System.Drawing.Color.Red);
                levelCell.Style.Font.Bold = true;
            }
            else if (log.Level == "Warning")
            {
                levelCell.Style.Font.Color.SetColor(System.Drawing.Color.Orange);
            }
            
            // 隔行换色
            if (row % 2 == 0)
            {
                worksheet.Row(row).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Row(row).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(248, 249, 255));
            }
        }
        
        // 添加统计信息
        var summaryRow = logs.Count + 3;
        worksheet.Cells[summaryRow, 1].Value = $"导出时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        worksheet.Cells[summaryRow, 2].Value = $"总记录数：{logs.Count}";
        if (from.HasValue)
        {
            worksheet.Cells[summaryRow, 3].Value = $"开始日期：{from.Value:yyyy-MM-dd}";
        }
        if (to.HasValue)
        {
            worksheet.Cells[summaryRow, 4].Value = $"结束日期：{to.Value:yyyy-MM-dd}";
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
        var worksheet = package.Workbook.Worksheets.Add("登录日志");
        
        // 设置表头样式
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Font.Size = 12;
        headerRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        headerRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(102, 126, 234));
        headerRow.Style.Font.Color.SetColor(System.Drawing.Color.White);
        
        // 设置列宽
        worksheet.Column(1).Width = 20;
        worksheet.Column(2).Width = 10;
        worksheet.Column(3).Width = 15;
        worksheet.Column(4).Width = 10;
        worksheet.Column(5).Width = 15;
        worksheet.Column(6).Width = 40;
        
        // 表头
        worksheet.Cells[1, 1].Value = "时间";
        worksheet.Cells[1, 2].Value = "用户 ID";
        worksheet.Cells[1, 3].Value = "用户名";
        worksheet.Cells[1, 4].Value = "结果";
        worksheet.Cells[1, 5].Value = "IP 地址";
        worksheet.Cells[1, 6].Value = "UserAgent";

        // 数据行
        for (int i = 0; i < logs.Count; i++)
        {
            var log = logs[i];
            var row = i + 2;
            worksheet.Cells[row, 1].Value = log.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[row, 2].Value = log.UserId;
            worksheet.Cells[row, 3].Value = log.Username;
            worksheet.Cells[row, 4].Value = log.Success ? "成功" : "失败";
            worksheet.Cells[row, 5].Value = log.Ip ?? "";
            worksheet.Cells[row, 6].Value = log.UserAgent ?? "";
            
            // 根据登录结果设置颜色
            var resultCell = worksheet.Cells[row, 4];
            if (log.Success)
            {
                resultCell.Style.Font.Color.SetColor(System.Drawing.Color.Green);
            }
            else
            {
                resultCell.Style.Font.Color.SetColor(System.Drawing.Color.Red);
                resultCell.Style.Font.Bold = true;
            }
            
            // 隔行换色
            if (row % 2 == 0)
            {
                worksheet.Row(row).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Row(row).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(248, 249, 255));
            }
        }
        
        // 添加统计信息
        var summaryRow = logs.Count + 3;
        var successCount = logs.Count(l => l.Success);
        var failCount = logs.Count - successCount;
        
        worksheet.Cells[summaryRow, 1].Value = $"导出时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        worksheet.Cells[summaryRow, 2].Value = $"总记录数：{logs.Count}";
        worksheet.Cells[summaryRow, 3].Value = $"成功：{successCount}";
        worksheet.Cells[summaryRow, 4].Value = $"失败：{failCount}";
        if (from.HasValue)
        {
            worksheet.Cells[summaryRow, 5].Value = $"开始日期：{from.Value:yyyy-MM-dd}";
        }
        if (to.HasValue)
        {
            worksheet.Cells[summaryRow, 6].Value = $"结束日期：{to.Value:yyyy-MM-dd}";
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
