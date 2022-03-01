using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedisConnector.Core
{
    public class OutboxMessage
    {
        public Guid Id { get; private set; }
        public string StreamName { get; } 
        public string MessageKey { get; } 
        public string Message { get; private set; }
        public DateTime AddedAt { get; private set; }
        public bool Processed { get; private set; }
        public DateTime? ProcessedAt { get; private set; }

        public OutboxMessage()
        {
        }
         
        public OutboxMessage(
           string streamName,
           string messageKey,
           string message)
        {
            streamName.Guard();
            messageKey.Guard();
            message.Guard(nameof(message));

            StreamName = streamName;
            MessageKey = messageKey;
            Message = message;
        }

        public OutboxMessage Add()
        {
            Id = Guid.NewGuid();
            AddedAt = DateTime.Now;
            return this;
        }

        public OutboxMessage Update()
        {
            Processed = true;
            ProcessedAt = DateTime.Now;
            return this;
        }

        public NameValueEntry[] ToEntry()
        {
            return new List<NameValueEntry>()
            {
                new NameValueEntry(RedisMessageTemplate.MessageKey, MessageKey),
                new NameValueEntry(RedisMessageTemplate.Message, Message)
            }.ToArray();
        }
    }
}
