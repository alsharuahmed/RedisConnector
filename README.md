# Redis.Util.RedisConnector
High performance Redis client, Add messages, transform the received messages to local events, and manage poison messages.

## RedisConnector Context diagram
![RedisConnector Context diagram](https://user-images.githubusercontent.com/43896049/150760902-8b84bf13-8ca8-4de2-b20e-f215f4fadc45.jpg)


## RedisConnector Container diagram
![RedisConnector Container diagram](https://user-images.githubusercontent.com/43896049/150774685-69a0e026-738f-453e-a5d4-3f075f027a40.jpg)

## RedisConnector Component Diagram
![RedisConnector Component Diagram](https://user-images.githubusercontent.com/43896049/150780733-cedcc8c3-f318-49eb-83b2-c01da129dc39.jpg)

---

## Configuration
The configuration can easily be setup using the following variables:
- `RedisConfiguration`: The configuration section of RedisConnector that will be mapped internally.
   - `Username`: Redis cluster communication user. Default: none. Required: true.
   - `Password`: Redis cluster communication password. Default: none. Required: true.
   - `Group`: The consumer group name. Default: none. Required: true.
   - `Consumer`: The consumer name. Default: none. Required: true.
   - `ExponentialRetryDeltaBackOffMilliseconds`: Represents a retry policy that performs retries, using a randomized exponential
      back off scheme to determine the interval between retries. Default: 5000. Required: true.
   - `ConnectRetry`: The number of times to repeat the initial connect cycle if no servers respond. Default: 2147483647. Required: true.
   - `GeneralHandlerForAll`: Enable the general handler (if the client implemented it) to handle any events, although it might have been handled by the event handler. Default:         false. Required: No.
   - `AtomicHandlers` : When `GeneralHandlerForAll`=true, atomic handlers will be considered, therefore, if and only if both the specific and general handlers success the result       will be true otherwise false. Default: false. Required: No.
   - `EndPoints`: Array of objects (Server, Port) that points to the Redis cluster nodes.
      - `Server`: The ip address for a node.
      - `Port`: The port of the node.
   - `Streams`: Dictionary of <key, value> pairs that represents the streams that the application interested in.
   - `PoisonMessage`: A bunch of configuration values that will manage dealing with pending/poison messages.
      - `Consumer`: The consumer name that the poison messages ownership will be changed to. Default: PoisonMessageConsumer. Required: true.
      - `pending_check_interval_minutes`: Interval (in minutes) to read the pending messages and try to republish it. Default: 30 mins. Required: true.
      - `pending_retention_check_interval_minutes`: Interval (in minutes) to check the pending messages if marked as poison messages, therefore, change its ownership. Default:            30 mins. Required: true.
      - `pending_retention_hours`: Default: 30 mins. Required: true.
      - `pending_retention_minutes`:
      - `pending_retention_ms`:
