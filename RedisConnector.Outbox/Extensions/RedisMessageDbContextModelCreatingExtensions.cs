using Microsoft.EntityFrameworkCore; 
using RedisConnector.Core;

namespace RedisConnector.Outbox
{
    public static class OutboxMessageDbContextModelCreatingExtensions
    {
        public static void ConfigureRedisOutbox(this ModelBuilder builder)
        {
            builder.Guard(nameof(builder));  

            builder.ApplyConfiguration(new OutboxMessageEntityConfig()); 
        }
    }
}
