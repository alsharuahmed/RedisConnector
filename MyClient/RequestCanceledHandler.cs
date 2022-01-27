using RedisConnector;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class RequestCanceledHandler : BaseHandler<RedisMessage>
    {
        public RequestCanceledHandler(Publisher<RedisMessage> publisher) : base(publisher)
        {
        }

        public override void Handle(object sender, IRedisMessage e)
        {
            RedisMessage redisMessage = (RedisMessage)e;

            Console.WriteLine($"RequestCanceledHandler: received a message of key {redisMessage.MessageKey}, " +
               $"message: {redisMessage.Message}, one of the extra prop value is {redisMessage.GetExtraProp("corrleationId")}." +
               $"Request id = {redisMessage.GetExtraProp("RequestId")}, , status = {redisMessage.GetExtraProp("Status")}.\r\n"); 
        }
    }
}
