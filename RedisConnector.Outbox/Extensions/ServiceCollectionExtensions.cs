using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RedisConnector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisConnector.Outbox
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterRedisConnectorOutbox(this IServiceCollection services)
        {
            services.AddTransient<IOutboxRepository, OutboxRepository>();
        }
    }
}
