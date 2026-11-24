namespace StockTrack.Models;

public static class DatabaseConfig
{
    public static string ConnectionString { get; set; } =
        "Server=127.0.0.1;Port=3306;Database=StockTrackDB;User ID=root;Password=;";
}
