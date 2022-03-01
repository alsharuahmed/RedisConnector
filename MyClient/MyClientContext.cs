using Microsoft.EntityFrameworkCore;
using MyClient.Entity;
using MyClient.EntityConfiguration;
using RedisConnector;
using RedisConnector.Core;

namespace MyClient
{
    public class MyClientContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        //public DbSet<OutboxMessage> OutboxMessages { get; set; }

        public MyClientContext(DbContextOptions<MyClientContext> options) : base(options)
        {
        }
         

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new BlogConfig());

            // RedisConnector
            builder.ConfigureRedisConnector();
        }
    }
}
