using System;

namespace RedisConnector
{  
    public class Publisher<T> where T : IRedisMessage
    {
        IRedisMessage _redisMessage { get; } 
        public event EventHandler<IRedisMessage> RedisMessageReceived;

        public Publisher(IRedisMessage redisMessage)
        {
            _redisMessage = redisMessage;
        }

        public void Publish()
        {
            OnRedisMessageReceived(_redisMessage);
        }


        protected virtual void OnRedisMessageReceived(IRedisMessage e)
        {
            EventHandler<IRedisMessage> handler = RedisMessageReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

}
