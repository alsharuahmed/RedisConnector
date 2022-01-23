using RedisConnector;
using System;
using System.Linq;

namespace ConsoleApp1
{
    public class GeneralEventHandler : BaseHandler
    {
        public GeneralEventHandler(Publisher<RedisMessage> publisher) : base(publisher)
        {
        }

        public override void Handle(object sender, IRedisMessage e)
        {
            RedisMessage redisMessage = (RedisMessage)e;
             
            Console.WriteLine($"GeneralEventHandler: received this message key {redisMessage.MessageKey}, message: {redisMessage.Message}.\r\n");
        }
    }
}