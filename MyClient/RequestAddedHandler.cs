using Microsoft.Extensions.Options;
using RedisConnector;
using RedisConnector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ConsoleApp1
{ 
    public class RequestAddedHandler : BaseHandler<RedisMessage>
    {
        private readonly IRedisConnector _redisConnector;
        private readonly RedisConfiguration _redisConfig;

        public RequestAddedHandler(
            Publisher<RedisMessage> publisher,
            IRedisConnector redisConnector,
            IOptions<RedisConfiguration> _redisConfig
           ) : base(publisher)
        {
            this._redisConfig = _redisConfig.Value;
            this._redisConnector = redisConnector;
        }


        public override void Handle(object sender, IRedisMessage e)
        { 
            RedisMessage redisMessage = (RedisMessage)e;
            RequestAddedMessage requestAddedMessage = redisMessage.Message.Deserialize<RequestAddedMessage>();
             
            Console.WriteLine($"RequestAddedHandler: received a message of key {redisMessage.MessageKey}, " +
                $"message: {redisMessage.Message}, one of the extra prop value is {redisMessage.GetExtraProp("corrleationId")}." +
                $"Request id = {requestAddedMessage.RequestId}, , status = {requestAddedMessage.Status}.\r\n");


            #region Push a new message
            var newMessage = new RedisMessage(
                             streamName: _redisConfig.Streams.Values.ToList().First(),
                             messageKey: "RequestCanceled",
                             message: "\"Hello World from RequestAddedHandler!\"");

            newMessage.AddExtraProp(new List<NameValueExtraProp>() { new NameValueExtraProp("parentMessageId", redisMessage.MessageId) });

            var reuslt = _redisConnector.StreamAddAsync(newMessage).Result;
            #endregion

        }
    }
     
}
