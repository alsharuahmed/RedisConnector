using System;

namespace ConsoleApp1
{
    public class RequestAddedMessage
    {
        public Guid RequestId { get; set; }
        public string Status { get; set; }

        public RequestAddedMessage(Guid requestId, string status)
        {
            RequestId = requestId;
            Status = status;
        }
    }
}
