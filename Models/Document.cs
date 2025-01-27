namespace ProjetDotNet.Models;

public class Document
{
    public int Id { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentPath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long DocumentSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}