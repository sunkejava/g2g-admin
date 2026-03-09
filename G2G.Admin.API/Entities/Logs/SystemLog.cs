namespace G2G.Admin.API.Entities.Logs;

public class SystemLog
{
    public int Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
