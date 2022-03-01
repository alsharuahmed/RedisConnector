using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyClient;
using MyClient.Entity;
using RedisConnector;
using RedisConnector.Core;
using RedisConnector.Outbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class MyClientContextFactory : IDesignTimeDbContextFactory<MyClientContext>
    {
        public MyClientContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MyClientContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=MyClient;User ID=sa;Password=NoNeed@Thanx3;MultipleActiveResultSets=true");

            return new MyClientContext(optionsBuilder.Options);
        }
    }

    public interface IBlogRepository
    {
        public Task AddAsync(MyClientContext myClientContext);
    }

    public class BlogRepository : IBlogRepository
    {
        private readonly DbContext _myClientContext;
        private readonly RedisConfiguration _redisConfiguration;
        private readonly IRedisConnector _redisConnector;

        public BlogRepository(
            MyClientContext myClientContext,
            IOptions<RedisConfiguration> redisConfiguration,
            IRedisConnector redisConnector)
        {
            this._myClientContext = myClientContext;
            this._redisConfiguration = redisConfiguration.Value;
            this._redisConnector = redisConnector; 
        }

        public async Task AddAsync(MyClientContext myClientContext)
        {
            var blog = await _myClientContext.Set<Blog>().AddAsync(new Blog("title", "url"));

            #region Push Messages
            var guidKey = Guid.NewGuid();
            var extraProp = new List<NameValueExtraProp>()
                {
                    new NameValueExtraProp("corrleationId", new Random().Next().ToString()),
                    new NameValueExtraProp("parentMessageId", new Random().Next().ToString()),
                };

            var requestAddedMessage = new RedisMessage(
                         streamName: _redisConfiguration.Streams.Values.ToList().First(),
                         messageKey: "RequestAdded",
                         message: new RequestAddedMessage(guidKey, "Added").Serialize());
            requestAddedMessage.AddExtraProp(extraProp);

            var requestCanceledMessage = new RedisMessage(
                       streamName: _redisConfiguration.Streams.Values.ToList().First(),
                       messageKey: "RequestCanceled",
                       message: "\"Hello World!\"");

            var otherMessage = new RedisMessage(
                      streamName: _redisConfiguration.Streams.Values.ToList().First(),
                      messageKey: "OtherEvent",
                      message: "\"Hello World!\"");
            #endregion

            _redisConnector.SetContext(myClientContext);
            var requestAddedMessageId = await _redisConnector.StreamAddAsync(requestAddedMessage);
           
            List<Task> addTasks = new List<Task>()
            {
                _redisConnector.StreamAddAsync(requestCanceledMessage),
                _redisConnector.StreamAddAsync(otherMessage)
            }; 
            await Task.WhenAll(addTasks);

            var result = _myClientContext.SaveChanges();

            //Try to send the message from the outbox to stream.
            await _redisConnector.AddOutboxToStreamAsync(Guid.Parse(requestAddedMessageId));
        }
    }

    internal class Program
    { 
        static async Task Main(string[] args)
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            serviceCollection.AddDbContext<MyClientContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Default")));

            serviceCollection.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });
             

            serviceCollection.Configure<RedisConfiguration>(configuration.GetSection("RedisConfiguration"));
            serviceCollection.AddTransient<IBlogRepository, BlogRepository>();
            serviceCollection.RegisterRedisConnector();
            serviceCollection.RegisterRedisConnectorOutbox(); 

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            var myClientContext = serviceProvider.GetRequiredService<MyClientContext>();
            var myRepository = serviceProvider.GetRequiredService<IBlogRepository>();

            await myRepository.AddAsync(myClientContext);



            var _redisConnector = serviceProvider.GetRequiredService<IRedisConnector>();
            
            //Should be in a background job that run as interval
            await _redisConnector.AddOutboxToStreamAsync();

            //Should be in a background job to be run once time; ReadStreamAsync method runs for ever.
            await _redisConnector.ReadStreamAsync();
        }
    }
}
