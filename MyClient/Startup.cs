using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyClient;
using RedisConnector;
using System.Configuration;

namespace ConsoleApp1
{
    //public class Startup
    //{

    //    public void ConfigureStartupServices(MyClientContext myClientContext)
    //    {
    //        var configuration = new ConfigurationBuilder()
    //            .AddJsonFile("appsettings.json", optional: false)
    //            .Build();

    //        servicesCollection.AddDbContext<MyClientContext>(options =>
    //            options.UseSqlServer(
    //                configuration.GetConnectionString("Default")));

    //        servicesCollection.AddLogging(builder =>
    //        {
    //            builder.AddConsole();
    //            builder.AddDebug();
    //        });

    //        // _serviceCollection.AddLogging();

    //        servicesCollection.Configure<RedisConfiguration>(configuration.GetSection("RedisConfiguration")); 
    //    }
    //}
}
