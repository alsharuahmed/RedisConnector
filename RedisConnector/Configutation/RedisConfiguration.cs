using System.Collections.Generic;
using System.Text;

namespace RedisConnector
{

    public sealed class RedisConfiguration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public List<RedisEndPoints> EndPoints { get; set; }
        public Dictionary<string, string> Streams { get; set; }
        public string Group { get; set; }
        public string Consumer { get; set; }
        public int ExponentialRetryDeltaBackOffMilliseconds { get; set; }
        public int ConnectRetry { get; set; }
        public PoisonMessage PoisonMessage { get; set; }
        public bool GeneralHandlerForAll { get; set; }

        public RedisConfiguration()
        {
#if NET5_0_OR_GREATER
            EndPoints = new();
#elif NETCOREAPP3_1
            EndPoints = new List<RedisEndPoints>();
#endif


            ConnectRetry = 2147483647;
            ExponentialRetryDeltaBackOffMilliseconds = 5000; 
        }
    }
}
