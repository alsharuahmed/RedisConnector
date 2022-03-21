using Microsoft.Extensions.Options;
using RedisConnector;
using RedisConnector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ConsoleApp1
{ 
    public class MyRequestAddedHandler : BaseHandler<RedisMessage>
    {
        private readonly IRedisConnector _redisConnector;
        private readonly RedisConfiguration _redisConfig;

        public MyRequestAddedHandler(
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
            MyRequestAddedMessage myRequestAddedMessage = redisMessage.Message.Deserialize<MyRequestAddedMessage>();

            Console.WriteLine($"MyRequestAddedHandler: received a message of key {redisMessage.MessageKey}, " +
                $"message: {redisMessage.Message}, one of the extra prop value is {redisMessage.GetExtraProp("corrleationId")}." +
                $"Request id = {myRequestAddedMessage.RequestId}, , status = {myRequestAddedMessage.Status}.\r\n"); 
        }
    }
     
}
