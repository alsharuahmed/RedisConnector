using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedisConnector.Core;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisConnector
{
    public sealed class RedisConnector : IRedisConnector
    {
        IServiceProvider _serviceProvider;
        Lazy<ConnectionMultiplexer> _lazyConnection;
        ConfigurationOptions _configurationOptions;
        readonly RedisConfiguration _redisConfig;
        readonly ILogger<RedisConnector> _logger;

        public IDatabase GetRedisDb()
        {
            IDatabase _redisDb;

            if (_lazyConnection == null) 
                InitConnection().Wait(); 

            _redisDb = _lazyConnection.Value.GetDatabase();
            return _redisDb;
        }


        RedisEventDispatcher _redisEventDispatcher;
        private IOutboxRepository _outboxRepository;

        RedisConnector()
        {
            _configurationOptions = new ConfigurationOptions();
        }

        public RedisConnector(
            IServiceProvider serviceProvider,
            IOptions<RedisConfiguration> redisConfig,
            ILogger<RedisConnector> logger
            ) : this()
        {
            this._serviceProvider = serviceProvider;
            this._logger = logger;
            this._redisConfig = redisConfig.Value;

            _redisEventDispatcher = RedisEventDispatcher.Object(_serviceProvider, _redisConfig, logger);
        }

        public RedisConnector(
           IServiceProvider serviceProvider,
           IOptions<RedisConfiguration> redisConfig,
           ILogger<RedisConnector> logger,
           IOutboxRepository outboxRepository
           ) : this(
               serviceProvider,
               redisConfig,
               logger
               )
        {
            this._outboxRepository = outboxRepository;
        }

        public void SetContext(object dbContext)
        {
            dbContext.Guard(nameof(dbContext));

            _outboxRepository.SetDbContext((DbContext)dbContext);
        }

        public async Task<string> StreamAddAsync(RedisMessage message, bool? enableOutbox = null, bool autoSave = false)
        {
            try
            {
                bool _enableOutbox = enableOutbox ?? _redisConfig.EnableOutbox;

                if (_enableOutbox)
                    return await AddToOutboxAsync(message, autoSave);

                return await AddToStreamAsync(message);
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> AddOutboxToStreamAsync(Guid outboxId, bool autoSave = true)
        {
            try
            {
                var outboxMessage = await _outboxRepository.GetByIdAsync(outboxId); 

                var result = await GetRedisDb().StreamAddAsync(outboxMessage.StreamName, outboxMessage.ToEntry());

                if (result.HasValue)
                {
                    await _outboxRepository.UpdateAsync(outboxMessage, autoSave);
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                throw new RedisException($"ReadConnector could not add the outbox message (id: {outboxId}) to the stream.", e);
            }
        }

        public async Task AddOutboxToStreamAsync(bool autoSave = false)
        {
            try
            {
                var outboxMessages = await _outboxRepository.GetUnprocessedMessages();

                for (int i = 0; i < outboxMessages.Count(); i++)
                {
                    try
                    {
                        var result = await AddToStreamAsync(outboxMessages.ElementAt(i).ToRedisMessage());
                        await _outboxRepository.UpdateAsync(outboxMessages.ElementAt(i), autoSave);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"ReadConnector could not add the outbox message (id: {outboxMessages.ElementAt(i).Id}) to the stream.", e);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                throw new RedisException($"ReadConnector could not add outbox messages to the stream.", e);
            }
        }

        private async Task<string> AddToStreamAsync(RedisMessage message)
        {
            try
            {
                return await GetRedisDb().StreamAddAsync(message.StreamName, message.ToEntry());
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                throw new RedisException("ReadConnector could not add the message", e);
            }
        }

        private async Task<string> AddToOutboxAsync(RedisMessage message, bool autoSave = false)
        {
            try
            {
                _outboxRepository.Guard(nameof(_outboxRepository));

                var outboxMessage = message.ToOutboxMessage();
                var result = await _outboxRepository.InsertAsync(outboxMessage, autoSave);
                return outboxMessage.Id.ToString();
            }
            catch (ArgumentException e)
            {
                _logger.LogCritical($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                throw new RedisException("ReadConnector OutboxRepository is null", e);
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                throw new RedisException("ReadConnector could not add the message to the outbox", e);
            }
        }

        public async Task ReadStreamAsync(bool alwaysOn)
        {
            DateTime dateTimeOfStartReading = DateTime.Now;

            while (true)
            {
                try
                {
#if NET5_0_OR_GREATER
                    List<RedisMessage> publishedMessages = new();
#elif NETCOREAPP3_1
                    List<RedisMessage> publishedMessages = new List<RedisMessage>();
#endif
                    bool pending_retention_check_interval_minutes_enabled = dateTimeOfStartReading.MinutesPassed(_redisConfig.PoisonMessage.pending_retention_check_interval_minutes);
                    bool pending_check_interval_minutes_enabled = dateTimeOfStartReading.MinutesPassed(_redisConfig.PoisonMessage.pending_check_interval_minutes);

                    if (pending_retention_check_interval_minutes_enabled)
                    {
                        await ManagePoisonMessageAsync();
                    }

                    Task<List<RedisMessage>> streamsReadPendingGroupAsyncTask = null;
                    Task<List<RedisMessage>> streamsReadGroupAsyncTask = StreamsReadGroupAsync();
                    List<Task> readTasks = new List<Task>() { streamsReadGroupAsyncTask };

                    if (pending_check_interval_minutes_enabled)
                    {
                        streamsReadPendingGroupAsyncTask = StreamsReadPendingGroupAsync();
                        readTasks.Add(streamsReadPendingGroupAsyncTask);
                    }


                    while (readTasks.Count > 0)
                    {
                        Task finishedTask = await Task.WhenAny(readTasks);

                        if (finishedTask == streamsReadGroupAsyncTask)
                            publishedMessages.AddRange(PublishMessages(streamsReadGroupAsyncTask.Result));
                        else if (finishedTask == streamsReadPendingGroupAsyncTask)
                            publishedMessages.AddRange(PublishMessages(streamsReadPendingGroupAsyncTask.Result));

                        readTasks.Remove(finishedTask);
                    }

                    AcknowledgeMsgs(publishedMessages);
                }
                catch (Exception e)
                {
                    _logger.LogCritical($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                }

                if (!alwaysOn)
                    return;
            }
        }

        private async Task ManagePoisonMessageAsync()
        {
            try
            {
                if (_redisConfig.PoisonMessage == null)
                    return;

                long pendingRetentionMs = GetPoisonRetentionInMs();

#if NET5_0_OR_GREATER
                List<RedisMessage> redisMessages = new();
#elif NETCOREAPP3_1
                List<RedisMessage> redisMessages = new List<RedisMessage>();
#endif


                foreach (var stream in _redisConfig.Streams)
                {
                    var pendingMessages = await GetRedisDb().StreamPendingMessagesAsync(stream.Value, _redisConfig.Group, int.MaxValue, _redisConfig.Consumer);

                    var poisonMessagesIds = pendingMessages
                        .Where(m =>
                            _redisConfig.PoisonMessage.pending_check_interval_minutes.FromMinutesToMilliseconds() * m.DeliveryCount >= pendingRetentionMs)
                        .Select(m =>
                            m.MessageId)
                        .ToArray();

                    if (poisonMessagesIds.Count() > 0)
                    {
                        var result = await GetRedisDb().StreamClaimIdsOnlyAsync(stream.Value, _redisConfig.Group, _redisConfig.PoisonMessage.Consumer, 0, poisonMessagesIds);
                        _logger.LogInformation($"Messages of the following ids have been claimed to Consumer:{_redisConfig.PoisonMessage.Consumer}, Group:{_redisConfig.Group}, ids:{{{string.Join(", ", result)}}}");
                    }
                }

            }
            catch (Exception e)
            {
                _logger.LogError($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                throw new InvalidOperationException("Invalid StreamsReadGroup operation", e); ;
            }
        }
         

        private async Task AcknowledgeMsgsAsync(List<RedisMessage> redisMessages)
        {
            try
            {
#if NET5_0_OR_GREATER
                List<Task> acknowledgeTasks = new();
#elif NETCOREAPP3_1
                List<Task> acknowledgeTasks = new List<Task>();
#endif

                redisMessages.ForEach(m => acknowledgeTasks.Add(GetRedisDb().StreamAcknowledgeAsync(m.StreamName, _redisConfig.Group, m.MessageId)));
                await Task.WhenAll(acknowledgeTasks);
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                throw new InvalidOperationException($"Invalid {nameof(AcknowledgeMsgsAsync)} operation", e);
            }
        }

        private void AcknowledgeMsgs(List<RedisMessage> redisMessages)
        {
            try
            {
                redisMessages.ForEach(m => GetRedisDb().StreamAcknowledge(m.StreamName, _redisConfig.Group, m.MessageId));
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                throw new InvalidOperationException($"Invalid {nameof(AcknowledgeMsgsAsync)} operation", e);
            }
        }

        private List<RedisMessage> PublishMessages(List<RedisMessage> redisMessages)
        {

#if NET5_0_OR_GREATER
            List<RedisMessage> dispatchedMessages = new();
#elif NETCOREAPP3_1
            List<RedisMessage> dispatchedMessages = new List<RedisMessage>();
#endif 

            foreach (var redisMessage in redisMessages)
            {
                var result = _redisEventDispatcher.Dispatch(redisMessage);
                if (result)
                    dispatchedMessages.Add(redisMessage);
            }

            return dispatchedMessages;
        }

        private long GetPoisonRetentionInMs()
            => long.Parse(_redisConfig.PoisonMessage.pending_retention_ms > 0 ? _redisConfig.PoisonMessage.pending_retention_ms.ToString() :
                _redisConfig.PoisonMessage.pending_retention_minutes > 0 ? _redisConfig.PoisonMessage.pending_retention_minutes.FromMinutesToMilliseconds().ToString() :
                _redisConfig.PoisonMessage.pending_retention_hours.FromHoursToMilliseconds().ToString());

        private async Task<List<RedisMessage>> StreamsReadGroupAsync()
        {
            try
            {
#if NET5_0_OR_GREATER
                List<RedisMessage> redisMessages = new();
#elif NETCOREAPP3_1
                List<RedisMessage> redisMessages = new List<RedisMessage>();
#endif

                foreach (var stream in _redisConfig.Streams)
                {
                    var newMessages = await GetRedisDb().StreamReadGroupAsync(
                                                            key: stream.Value,
                                                            groupName: _redisConfig.Group,
                                                            consumerName: _redisConfig.Consumer,
                                                            position: StreamPosition.NewMessages);

                    redisMessages.AddRange(ConvertToRedisMessages(stream.Value, newMessages));
                }

                return redisMessages;

            }
            catch (Exception e)
            {
                _logger.LogError($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                throw new InvalidOperationException("Invalid StreamsReadGroup operation", e); ;
            }
        }

        private async Task<List<RedisMessage>> StreamsReadPendingGroupAsync()
        {
            try
            {
#if NET5_0_OR_GREATER
                List<RedisMessage> redisMessages = new();
#elif NETCOREAPP3_1
                List<RedisMessage> redisMessages = new List<RedisMessage>();
#endif 

                foreach (var stream in _redisConfig.Streams)
                {
                    var pendingMessages = await GetRedisDb().StreamReadGroupAsync(
                                                           key: stream.Value,
                                                           groupName: _redisConfig.Group,
                                                           consumerName: _redisConfig.Consumer,
                                                           position: StreamPosition.Beginning);

                    redisMessages.AddRange(ConvertToRedisMessages(stream.Value, pendingMessages));
                }

                return redisMessages;

            }
            catch (Exception e)
            {
                _logger.LogError($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                throw new InvalidOperationException("Invalid StreamsReadGroup operation", e); ;
            }
        }

        private List<RedisMessage> ConvertToRedisMessages(string streamName, StreamEntry[] messages)
        {
#if NET5_0_OR_GREATER
            List<RedisMessage> redisMessages = new();
#elif NETCOREAPP3_1
            List<RedisMessage> redisMessages = new List<RedisMessage>();
#endif 

            foreach (var msg in messages)
            {
                try
                {
                    var redisMessage = new RedisMessage(
                                                          streamName: streamName,
                                                          messageKey: GetStreamEntryValue(msg, RedisMessageTemplate.MessageKey),
                                                          message: GetStreamEntryValue(msg, RedisMessageTemplate.Message));

                    redisMessage.SetAddedAt(GetStreamEntryValue(msg, RedisMessageTemplate.AddedAt));
                    redisMessage.AddExtraProp(GetStreamEntryExtraProp(msg));
                    redisMessage.SetMessageId(msg.Id);

                    redisMessages.Add(redisMessage);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Could not convert redis message. Message id {msg.Id}. " +
                        $"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                }
            }

            return redisMessages;
        }

        private string GetStreamEntryValue(StreamEntry streamEntry, string name)
            => streamEntry.Values.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase)).Value;

        private List<NameValueExtraProp> GetStreamEntryExtraProp(StreamEntry streamEntry)
            => streamEntry.Values.Where(e =>
                !string.Equals(e.Name, RedisMessageTemplate.MessageKey, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(e.Name, RedisMessageTemplate.Message, StringComparison.OrdinalIgnoreCase)).ToList().ToNameValueExtraProp();

        private async Task InitConnection()
        { 
            SetAuthentication();
            SetEndPoints();
            SetRetryPolicy();

            var connection = await Connect(_configurationOptions);
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() => connection);
             
            await CreateConsumerGroups();
        }


        private async Task<ConnectionMultiplexer> Connect(ConfigurationOptions configurationOptions)
        {
            ConnectionMultiplexer connection = null;

            try
            {
                connection = await ConnectionMultiplexer.ConnectAsync(configurationOptions);
                return connection;
            }
            catch (RedisConnectionException e)
            {

                _logger.LogCritical($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                throw;

            }
            catch (Exception e)
            {
                _logger.LogCritical($"ReadConnector could not initiate, throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                throw new RedisException("ReadConnector could not initiate", e);
            }
        }

        private void SetRetryPolicy()
        {
            _configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(_redisConfig.ExponentialRetryDeltaBackOffMilliseconds);
            _configurationOptions.ConnectRetry = _redisConfig.ConnectRetry;
        }

        private void SetEndPoints()
        {
            _redisConfig.EndPoints.ForEach(endPoint =>
                _configurationOptions.EndPoints.Add($"{endPoint.Server}:{endPoint.Port}"));
        }

        private void SetAuthentication()
        {
            _configurationOptions.Password = _redisConfig.Password;
            _configurationOptions.User = _redisConfig.Username;
        }

        private async Task CreateConsumerGroups()
        {
            foreach (var stream in _redisConfig.Streams)
            {
                try
                {
                    var result = await GetRedisDb().StreamCreateConsumerGroupAsync(stream.Value, _redisConfig.Group);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("BUSYGROUP") || e.InnerException.Message.Contains("BUSYGROUP"))
                        _logger.LogInformation($"Stream: {stream.Value}, Group: {_redisConfig.Group}, Message { e.Message }, InnerException: {e.InnerException?.Message}");
                    else
                    {
                        _logger.LogCritical($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace}, Message { e.Message }, InnerException: {e.InnerException}");
                        throw new RedisException("ReadConnector could not initiate", e);
                    }
                }

            }
        }


    }
}
