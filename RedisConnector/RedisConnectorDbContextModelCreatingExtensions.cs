using Microsoft.EntityFrameworkCore; 
using RedisConnector.Core;
using RedisConnector.Outbox;

namespace RedisConnector
{
    public static class RedisConnectorDbContextModelCreatingExtensions
    {
        public static void ConfigureRedisConnector(this ModelBuilder builder)
        {
            builder.Guard();

            builder.ConfigureRedisOutbox();
        }
    }
}
