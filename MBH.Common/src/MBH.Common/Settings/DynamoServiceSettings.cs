namespace MBH.Common.Settings
{
    public class DynamoServiceSettings
    {
        public string AttirbuteName { get; init; }
        public string AttributeType { get; init; }
        public string KeyType { get; init; }
        public int ReadCapacityUnits { get; init; }
        public int WriteCapacityUnits { get; init; }
    }
}