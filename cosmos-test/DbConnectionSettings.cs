namespace Mongo2CosmosExporter
{
    public class DbConnectionSettings
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string AuthDbName { get; set; }
        public string DbName { get; set; }
        public string CollectionName { get; set; }
        public bool UseSsl { get; set; }

        public override string ToString()
        {
            var value = $"Host: {Host}\n Port: {Port}\n AuthDbName: {AuthDbName}\n DbName: {DbName}\n CollectionName: {CollectionName}\n";
            value+= $"UserName: {UserName}\n Password: {Password}\n UseSsl: {UseSsl}\n\n";

            return value;
        }
    }
}