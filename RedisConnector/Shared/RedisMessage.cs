using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedisConnector
{
    public class RedisMessage : EventArgs, IRedisMessage
    {
        public string StreamName { get; }
        public string MessageId { get; private set; }
        public string MessageKey { get; }
        public string Message { get; } 

        List<NameValueEntry> _extraProp { get; set; }

        public IReadOnlyList<NameValueExtraProp> ExtraProp
        {
            get
            {
                return _extraProp.ToNameValueExtraProp();
            }
        }

        public RedisMessage()
        { 
#if NET5_0_OR_GREATER
                _extraProp = new();
#elif NETCOREAPP3_1
            _extraProp = new List<NameValueEntry>();
#endif 
        }

        public RedisMessage(
            string streamName,
            string messageKey,
            string message) : this()
        {
            StreamName = streamName;
            MessageKey = messageKey;
            Message = message; 
        }


        public RedisMessage SetMessageId(string messageId)
        { 
            MessageId = messageId;
            return this;
        }

        public RedisMessage AddExtraProp(List<NameValueExtraProp> nameValueEntries)
        {
            _extraProp.AddRange(nameValueEntries.ToNameValueEntry());
            return this;
        }

        public string GetExtraProp(string name) 
            => _extraProp.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)).Value;

        public NameValueEntry[] ToEntry()
        {
            List<NameValueEntry> nameValueEntries = _extraProp;

            nameValueEntries.Add(new NameValueEntry(RedisMessageTemplate.StreamName, StreamName));
            nameValueEntries.Add(new NameValueEntry(RedisMessageTemplate.MessageKey, MessageKey));
            nameValueEntries.Add(new NameValueEntry(RedisMessageTemplate.Message, Message));

            return nameValueEntries.ToArray();
        }
    }
}
