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
- `Username:` Redis cluster communication user. Default: none. Required: true.
- `Password:` Redis cluster communication password. Default: none. Required: true.
- `Group:` The consumer group name. Default: none. Required: true.
- `Consumer:` The consumer name. Default: none. Required: true.
- `ExponentialRetryDeltaBackOffMilliseconds:` Represents a retry policy that performs retries, using a randomized exponential
    back off scheme to determine the interval between retries. Default: 5000. Required: true.
- `ConnectRetry:` The number of times to repeat the initial connect cycle if no servers respond. Default: 2147483647. Required: true.
- `GeneralHandlerForAll:` .
