namespace RedisConnector
{
    public interface IHandler<T> where T : IRedisMessage
    {
        void Handle(object sender, IRedisMessage e);
    }

}
