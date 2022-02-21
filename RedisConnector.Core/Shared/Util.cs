using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RedisConnector.Core;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedisConnector.Core
{
    public static class Util
    {
        #region Extensions
        public static bool MinutesPassed(this DateTime startTime, double minutes)
            => ((int)DateTime.Now.Subtract(startTime).TotalSeconds) % TimeSpan.FromMinutes(minutes).TotalSeconds == 0;

        public static T Deserialize<T>(this string value) where T : class => JsonConvert.DeserializeObject<T>(value);

        public static string Serialize(this object obj) => JsonConvert.SerializeObject(obj);

        internal static NameValueEntry ToNameValueEntry(this NameValueExtraProp nameValueExtraProp)
            => new NameValueEntry(nameValueExtraProp.Name, nameValueExtraProp.Value);

        internal static List<NameValueEntry> ToNameValueEntry(this List<NameValueExtraProp> list)
        {
#if NET5_0_OR_GREATER
            List<NameValueEntry> result = new();
#elif NETCOREAPP1_0_OR_GREATER
            List<NameValueEntry> result = new List<NameValueEntry>();
#endif  
            result.AddRange(list.Select(i => i.ToNameValueEntry()));
            return result;
        }

        public static NameValueExtraProp ToNameValueExtraProp(this NameValueEntry nameValueEntry)
            => new NameValueExtraProp(nameValueEntry.Name, nameValueEntry.Value);

        public static List<NameValueExtraProp> ToNameValueExtraProp(this List<NameValueEntry> list)
        {
#if NET5_0_OR_GREATER
            List<NameValueExtraProp> result = new();
#elif NETCOREAPP3_1
            List<NameValueExtraProp> result = new List<NameValueExtraProp>();
#endif 
            result.AddRange(list.Select(i => i.ToNameValueExtraProp()));
            return result;
        }

        public static double FromMinutesToMilliseconds(this double minutes)
            => TimeSpan.FromMinutes(minutes).TotalMilliseconds;

        public static double FromHoursToMilliseconds(this int hours)
            => TimeSpan.FromHours(hours).TotalMilliseconds;

        public static OutboxMessage ToOutboxMessage(this RedisMessage redisMessage)
        {
            return new OutboxMessage(
                redisMessage.StreamName,
                redisMessage.MessageKey,
                redisMessage.Message);
        }

        public static RedisMessage ToRedisMessage(this OutboxMessage outboxMessage)
        {
            return new RedisMessage(
                outboxMessage.StreamName,
                outboxMessage.MessageKey,
                outboxMessage.Message);
        }

        #region Guard
        public static void Guard(this object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
        }

        public static void Guard(this ModelBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
        }

        public static void Guard(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));
        }
        #endregion

        #endregion
    }
}
