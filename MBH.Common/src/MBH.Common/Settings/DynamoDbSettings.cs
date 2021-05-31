namespace MBH.Common.Settings
{
    public class DynamoDbSettings
    {
        public string Host { get; init; }

        public int Port { get; init; }

        public string ConnectionString => $"http://{Host}:{Port}";
        
    }
}