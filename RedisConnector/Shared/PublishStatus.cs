using System;
using System.Collections.Generic;
using System.Text;

namespace RedisConnector
{
    internal enum PublishStatus
    {
        None,
        NoHandler,
        HandlerException,
        Published
    }
}
