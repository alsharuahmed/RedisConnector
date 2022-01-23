using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedisConnector
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
        public Task<string> StreamAddAsync(RedisMessage redisMessage);
        public Task ReadStreamAsync(); 
    }
}
