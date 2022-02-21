namespace RedisConnector.Core
{
    public class NameValueExtraProp
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        NameValueExtraProp()
        {
        }

        public NameValueExtraProp(
            string name,
            string value)
        {
            Name = name;
            Value = value;
        }
    }
}
