using System;

namespace ConsoleApp1
{
    public class MyRequestAddedMessage
    {
        public Guid RequestId { get; set; }
        public string Status { get; set; }

        public MyRequestAddedMessage(Guid requestId, string status)
        {
            RequestId = requestId;
            Status = status;
        }
    }
}
