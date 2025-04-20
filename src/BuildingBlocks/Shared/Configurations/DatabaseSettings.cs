namespace Shared.Configurations
{
    public class DatabaseSettings
    {
        public string DBProvider { get; set; }

        public string ConnectionString { get; set; } = string.Empty;
    }
}