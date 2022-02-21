using RedisConnector;
using RedisConnector.Core;
using System;
using System.Linq;

namespace ConsoleApp1
{
    public class GeneralEventHandler : BaseHandler
    {
        private readonly IRedisConnector _redisConnector;

        public GeneralEventHandler(
            Publisher<RedisMessage> publisher,
            IRedisConnector redisConnector) : base(publisher)
        {
        }

        public override void Handle(object sender, IRedisMessage e)
        {
            RedisMessage redisMessage = (RedisMessage)e;
             
            Console.WriteLine($"GeneralEventHandler: received this message key {redisMessage.MessageKey}, message: {redisMessage.Message}.\r\n");
        }
    }
}