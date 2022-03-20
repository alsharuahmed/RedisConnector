using System;
using System.Threading.Tasks;

namespace RedisConnector.Core
{
    //
    // Summary:
    //     Describes functionality that is common to both standalone redis servers and redis
    //     clusters
    public interface IRedisConnector
    {
        // Summary:
        //     Adds an entry using the specified values to the given stream key. If key does
        //     not exist, a new key holding a stream is created. The command returns the ID
        //     of the newly created stream entry.
        //
        // Parameters:
        //   RedisMessage:
        //     The message of type RedisMessage. 
        // Returns:
        //     The ID of the newly created message.  
        Task<string> StreamAddAsync(RedisMessage redisMessage, bool? enableOutbox = null, bool autoSave = false);
        Task ReadStreamAsync(bool alwaysOn = false); 
        Task<string> AddOutboxToStreamAsync(Guid outboxId, bool autoSave = true);
        Task AddOutboxToStreamAsync(bool autoSave = true);
        void SetContext(object dbContext);
    }
}
