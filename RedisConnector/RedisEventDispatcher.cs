using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedisConnector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RedisConnector
{

    internal sealed class RedisEventDispatcher
    {
        private static IServiceProvider _serviceProvider;
        private static ILogger<RedisConnector> _logger;
        private static RedisConfiguration _redisConfig;
        private List<Type> _allTypesInThisAssembly = new List<Type>();

        private static RedisEventDispatcher _object = null;
        public static RedisEventDispatcher Object(
            IServiceProvider serviceProvider,
            RedisConfiguration redisConfig,
            ILogger<RedisConnector> logger)
        {
            if (_object == null)
            {
                _object = new RedisEventDispatcher(serviceProvider, redisConfig, logger);
            }
            return _object;
        }

        RedisEventDispatcher(
            IServiceProvider serviceProvider,
            RedisConfiguration redisConfig,
            ILogger<RedisConnector> logger)
        {
            _serviceProvider = serviceProvider;
            _redisConfig = redisConfig;
            _logger = logger;

            LoadAssembly();
        }

        private void LoadAssembly()
        {
            try
            {
                var assemblyNames = Assembly
                                .GetEntryAssembly()
                                .GetReferencedAssemblies().ToList();
                assemblyNames.Add(Assembly.GetEntryAssembly().GetName());



                foreach (var assembly in assemblyNames)
                {
                    _allTypesInThisAssembly.AddRange(Assembly.Load(assembly).GetTypes().Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    (t.GetInterface(typeof(IHandler<>).Name.ToString()) != null ||
                    t.IsSubclassOf(typeof(BaseHandler)))));
                }
            }
            catch (Exception e)
            { 
                _logger.LogCritical($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                throw;
            }
        }

        RedisEventDispatcher()
        {
        }

        public bool Dispatch(RedisMessage redisMessage)
        {
            Type generalBaseEventHandler = null;
#if NET5_0_OR_GREATER
               List<Type> handlers = new(); 
#elif NETCOREAPP3_1
            List<Type> handlers = new List<Type>();
#endif



            PublishStatus generalEventDispatched, exactEventDispatched;
            generalEventDispatched = exactEventDispatched = PublishStatus.None;

            try
            {
                SegregateTypes(ref generalBaseEventHandler, ref handlers);

                exactEventDispatched = PublishExactEvent(redisMessage, handlers);

                if(exactEventDispatched == PublishStatus.NoHandler || _redisConfig.GeneralHandlerForAll)
                    generalEventDispatched = PublishGeneralEvent(redisMessage, generalBaseEventHandler);
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                throw;
            }

            if(_redisConfig.GeneralHandlerForAll && _redisConfig.AtomicHandlers)
                return (generalEventDispatched == PublishStatus.Published && exactEventDispatched == PublishStatus.Published);

            return (generalEventDispatched == PublishStatus.Published || exactEventDispatched == PublishStatus.Published);
        }

        private PublishStatus PublishExactEvent(RedisMessage redisMessage, List<Type> handlers)
        {
            try
            {
                var handlerType = handlers.FirstOrDefault(e => e.Name.ToLower().Equals($"{redisMessage.MessageKey.ToLower()}handler"));

                if (handlerType == null)
                    return PublishStatus.NoHandler;


                Type publisherType = typeof(Publisher<>).MakeGenericType(typeof(RedisMessage));
                object publisher = Activator.CreateInstance(
                    publisherType,
                    redisMessage);

                ActivatorUtilities.CreateInstance(
                    _serviceProvider,
                    handlerType,
                    publisher);


                MethodInfo method = publisherType.GetMethod("Publish");
                method.Invoke(publisher, null);
                return PublishStatus.Published;
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                return PublishStatus.HandlerException;
            }
        }

        private PublishStatus PublishGeneralEvent(RedisMessage redisMessage, Type generalBaseEventHandler)
        {
            if (generalBaseEventHandler == null)
                return PublishStatus.NoHandler;

            try
            {
                Publisher<RedisMessage> publisherOfGeneral = new Publisher<RedisMessage>(redisMessage);

                ActivatorUtilities.CreateInstance(
                    _serviceProvider,
                    generalBaseEventHandler,
                    publisherOfGeneral);

                publisherOfGeneral.Publish();

                return PublishStatus.Published;
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Throw the following exception: {e.Message}, stacktrace: {e.StackTrace} InnerException: {e.InnerException}");
                return PublishStatus.HandlerException;
            }

        }

        private void SegregateTypes(ref Type generalBaseEventHandler, ref List<Type> handlers)
        {
            foreach (Type type in _allTypesInThisAssembly.Where(t => t != null))
            {
                if (type.GetInterface(typeof(IHandler<>).Name.ToString()) != null)
                {
                    handlers.Add(type);
                }
                else if (type.IsSubclassOf(typeof(BaseHandler)))
                {
                    generalBaseEventHandler = type;
                }
            }
        }
    }
}
