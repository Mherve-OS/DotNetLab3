namespace StockTrack.Models;

public class AuditLog
{
    public int LogID { get; set; }
    public int UserID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime LogTimestamp { get; set; } = DateTime.Now;
}
