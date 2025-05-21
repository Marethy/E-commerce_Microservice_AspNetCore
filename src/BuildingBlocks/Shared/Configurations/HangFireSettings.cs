namespace Shared.Configurations;

public class HangFireSettings
{
    public string Route { get; set; }
    public string ServerName { get; set; }
    public Dashboard Dashboard { get; set; }
    public DatabaseSettings Storage { get; set; }
}

public class Dashboard
{
    public string AppPath { get; set; }
    public string StatsPollingInterval { get; set; }
    public string DashboardTitle { get; set; }
}