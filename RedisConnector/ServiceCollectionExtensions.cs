using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedisConnector.Core;
using RedisConnector.Outbox;
using System;

namespace RedisConnector
{
    public static class ServiceCollectionExtensions
    { 
        public static void RegisterRedisConnector(this IServiceCollection services)
        {
            services.AddSingleton<IRedisConnector, RedisConnector>();
        } 
    }
}
