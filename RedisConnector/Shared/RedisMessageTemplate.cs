using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedisConnector
{
    internal static class RedisMessageTemplate
    {
        public const string StreamName = "StreamName";
        public const string MessageId = "MessageId";
        public const string MessageKey = "MessageKey"; 
        public const string Message = "Message"; 
    }
}
