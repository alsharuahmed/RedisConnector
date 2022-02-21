using Microsoft.EntityFrameworkCore; 
using RedisConnector.Core;

namespace RedisConnector.Outbox
{
    public static class OutboxMessageDbContextModelCreatingExtensions
    {
        public static void ConfigureRedisOutbox(this ModelBuilder builder)
        {
            builder.Guard();  

            builder.ApplyConfiguration(new OutboxMessageEntityConfig()); 
        }
    }
}
