namespace Mongo2CosmosExporter
{
    public class AppSettings
    {
        public int BatchSize { get; set; } = 1000;
        public int MaxDegreeOfParallelism { get; set; } = -1;
        public DbConnectionSettings DestinationDb { get; set; }
    }
}