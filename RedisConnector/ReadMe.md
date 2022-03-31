# Redis.Util.RedisConnector
High-performance Redis client, Add messages, transform the received messages to local events, and manage poison messages.

## RedisConnector Context diagram
![RedisConnector Context diagram](https://user-images.githubusercontent.com/43896049/150760902-8b84bf13-8ca8-4de2-b20e-f215f4fadc45.jpg)

## RedisConnector Container diagram
![RedisConnector Container diagram (1)](https://user-images.githubusercontent.com/43896049/156996620-873fea70-6eff-4846-b7c4-e04f81d147fa.jpg)

## RedisConnector Component Diagram
![RedisConnector Component Diagram (1)](https://user-images.githubusercontent.com/43896049/156997443-15068271-adf9-4bca-8e18-a8c5b399dc5b.jpg)

---

## Configuration
The configuration can easily be set up using the following variables:
- `RedisConfiguration`: The configuration section of RedisConnector that will be mapped internally.
   - `Username`: Redis cluster communication user. Default: none. Required: true.
   - `Password`: Redis cluster communication password. Default: none. Required: true.
   - `Group`: The consumer group name. Default: none. Required: true.
   - `Consumer`: The consumer name. Default: none. Required: true.
   - `ExponentialRetryDeltaBackOffMilliseconds`: Represents a retry policy that performs retries, using a randomized exponential
      back-off scheme to determine the interval between retries. Default: 5000. Required: true.
   - `ConnectRetry`: The number of times to repeat the initial connect cycle if no servers respond. Default: 2147483647. Required: true.
   - `GeneralHandlerForAll`: Enable the general handler (if the client implemented it) to handle any events, although it might have been handled by the event handler. Default:         false. Required: No.
   - `AtomicHandlers` : When `GeneralHandlerForAll`=true, atomic handlers will be considered, therefore, if you set `AtomicHandlers`=true means either all handlers (specific, general) get succeed to acknowledge the event or (for false value) any handler success will get the event acknowledged. Default: false. Required: No.
   - `EnableOutbox` : Enable outbox pattern where messages will persist in the app database before they get published to Redis. Default: false. Required: No. (Hint: enable this feature will require a database migration).
   - `EndPoints`: Array of objects (Server, Port) that points to the Redis cluster nodes.
      - `Server`: The ip address for a node.
      - `Port`: The port of the node.
   - `Streams`: Dictionary of <key, value> pairs that represent the streams that the application is interested in.
   - `PoisonMessage`: A bunch of configuration values that will manage to deal with pending/poison messages.
      - `Consumer`: The consumer name that the poison messages ownership will be changed to. Default: PoisonMessageConsumer. Required: true.
      - `pending_check_interval_minutes`: Interval (in minutes) to read the pending messages and try to republish them. Default: 30 mins. Required: true.
      - `pending_retention_check_interval_minutes`: Interval (in minutes) to check the pending messages if marked as poison messages, therefore, change its ownership. Default:            5 mins. Required: true.
      - `pending_retention_hours`: The number of hours to keep a message before marking it poisons, secondary to pending_retention_minutes property. If not set, the value in pending_retention_hours is used Default: 168 hours. Required: false.
      - `pending_retention_minutes`: The number of minutes to keep a message before marking it poisons, secondary to pending_retention_ms property. If not set, the value in pending_retention_minutes is used Default: 0. Required: false.
      - `pending_retention_ms`: The number of milliseconds to keep a message before marking them as poison. Default: 0. Required: false.
---

## Intervals Clarification
![Intervals](https://user-images.githubusercontent.com/43896049/159156043-97f7bae7-42f5-46e5-bb4e-abddd303e69a.jpg)
