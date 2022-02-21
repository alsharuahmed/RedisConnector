using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RedisConnector.Core;
using RedisConnector.Outbox;
using System;

namespace RedisConnector
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterRedisConnector(this IServiceCollection services , DbContext dbContext)
        {
            dbContext.Guard(); 

            services.AddSingleton<IRedisConnector, RedisConnector>(); 
            services.RegisterRedisConnectorOutbox(dbContext);
        }

        public static void RegisterRedisConnector(this IServiceCollection services)
        {
            //GuardOutbox(services);

            services.AddSingleton<IRedisConnector, RedisConnector>();
            services.RegisterRedisConnectorOutbox();
        }

        private static void GuardOutbox(IServiceCollection services)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            RedisConfiguration redisConfig = serviceProvider.GetService<IOptions<RedisConfiguration>>().Value;

            if (redisConfig.EnableOutbox)
                throw new InvalidOperationException("dbContext must be set before use outbox feature.");
        }
    }
}
