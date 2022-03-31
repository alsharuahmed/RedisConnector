using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedisConnector.Core
{
    public class RedisMessage : EventArgs, IRedisMessage
    {
        public string StreamName { get; }
        public string MessageId { get; private set; }
        public string MessageKey { get; }
        public string Message { get; }    
        public DateTime? AddedAt { get; private set; }

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
            streamName.Guard(nameof(streamName));
            messageKey.Guard(nameof(messageKey));
            message.Guard(nameof(message));

            StreamName = streamName;
            MessageKey = messageKey;
            Message = message; 
        }


        public RedisMessage SetMessageId(string messageId)
        {
            messageId.Guard(nameof(messageId));

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
             
            nameValueEntries.Add(new NameValueEntry(RedisMessageTemplate.MessageKey, MessageKey));
            nameValueEntries.Add(new NameValueEntry(RedisMessageTemplate.Message, Message));

            return nameValueEntries.ToArray();
        }

        public RedisMessage SetAddedAt(string addedAtUnixEntry)
        {
            if (!String.IsNullOrWhiteSpace(addedAtUnixEntry))
            {
                var addedAtDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(long.Parse(addedAtUnixEntry));
                AddedAt = addedAtDateTimeOffset.DateTime.ToLocalTime(); 
            }
            
            return this;
        }
    }
}
