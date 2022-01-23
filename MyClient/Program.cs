using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedisConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            Startup startup = new Startup();
            startup.ConfigureServices(services);
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            RedisConfiguration redisConfig = serviceProvider.GetService<IOptions<RedisConfiguration>>().Value;
            var redisConnector = serviceProvider.GetService<IRedisConnector>();


            #region Push Messages
            var guidKey = Guid.NewGuid();
            var extraProp = new List<NameValueExtraProp>()
                {
                    new NameValueExtraProp("corrleationId", new Random().Next().ToString()),
                    new NameValueExtraProp("parentMessageId", new Random().Next().ToString()),
                };

            var requestAddedMessage = new RedisMessage(
                         streamName: redisConfig.Streams.Values.ToList().First(),
                         messageKey: "RequestAdded",
                         message: new RequestAddedMessage(guidKey, "Added").Serialize());
            requestAddedMessage.AddExtraProp(extraProp);

            var requestCanceledMessage = new RedisMessage(
                       streamName: redisConfig.Streams.Values.ToList().First(),
                       messageKey: "RequestCanceled",
                       message: "\"Hello World!\"");

            var fuckMessage = new RedisMessage(
                      streamName: redisConfig.Streams.Values.ToList().First(),
                      messageKey: "OtherEvent",
                      message: "\"Hello World!\"");


            #endregion


            List<Task> addTasks = new List<Task>()
            {
                redisConnector.StreamAddAsync(requestAddedMessage),
                redisConnector.StreamAddAsync(requestCanceledMessage),
                redisConnector.StreamAddAsync(fuckMessage)
            };
             
            await Task.WhenAll(addTasks); 
             

            await redisConnector.ReadStreamAsync();
        }
    }

    public class Startup
    {

        public Startup()
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            services.Configure<RedisConfiguration>(configuration.GetSection("RedisConfiguration"));

            services.AddLogging();

            services.AddSingleton<IRedisConnector, RedisConnector.RedisConnector>();

        }
    }
}
