namespace G2G.Admin.API.Entities;

public class AppVersion
{
    public int Id { get; set; }
    public string VersionNo { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ReleaseNotes { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int? UploadedBy { get; set; }
}
