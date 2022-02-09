using RedisConnector.Core;

namespace RedisConnector
{
    public abstract class BaseHandler<T> : IHandler<T>  where T : IRedisMessage
    {
        public BaseHandler(Publisher<T> publisher)
        { 
            publisher.RedisMessageReceived += Handle;
        }

        public abstract void Handle(object sender, IRedisMessage e);
    }

    public class BaseHandler
    {
        public BaseHandler(Publisher<RedisMessage> publisher)
        {
            publisher.RedisMessageReceived += Handle;
        }

        public virtual void Handle(object sender, IRedisMessage e) { }
    }


}
