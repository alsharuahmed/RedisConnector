using System;

namespace RedisConnector
{
    public sealed class PoisonMessage
    {
        string _consumer; 
        public string Consumer
        {
            get => _consumer;
            set
            {
                if(!String.IsNullOrWhiteSpace(value))
                    _consumer = value;
            }
        } 


        double _pendingCheckIntervalMinutes;
        public double pending_check_interval_minutes
        {
            get => _pendingCheckIntervalMinutes;
            set
            {
                if (value >= 0)
                    _pendingCheckIntervalMinutes = value;
            }
        }


        double _pendingRetentionCheckIntervalMinutes;
        public double pending_retention_check_interval_minutes
        {
            get => _pendingRetentionCheckIntervalMinutes;
            set
            {
                if (value >= 0)
                    _pendingRetentionCheckIntervalMinutes = value;
            }
        }
         
        int _pendingRetentionHours; 
        public int pending_retention_hours
        { 
            get => _pendingRetentionHours;
            set
            {
                if(value >= 0)
                    _pendingRetentionHours = value;
            }
        } 
        public double pending_retention_minutes { get; set; }
        public double pending_retention_ms { get; set; }

        public PoisonMessage()
        {
            _consumer = "PoisonMessageConsumer";
            _pendingCheckIntervalMinutes = 30;
            _pendingRetentionCheckIntervalMinutes = 5;
            _pendingRetentionHours = 168;
        }
    }
}
