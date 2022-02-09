using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedisConnector.Core
{
    public class OutboxMessage
    {
        public Guid Id { get; }
        public string StreamName { get; } 
        public string MessageKey { get; }
        [NotMapped]
        public object Message { get; }
        public string SerializedMessage
        {
            get => Message.Serialize();
            set { }
        }
        public DateTime AddedAt { get; private set; }

        public OutboxMessage()
        {
        }

        public OutboxMessage(
           string streamName,
           string messageKey,
           object message)
        { 
            StreamName = streamName;
            MessageKey = messageKey;
            Message = message;
        }

        public OutboxMessage Add()
        {
            AddedAt = DateTime.Now;
            return this;
        }  
    }
}
