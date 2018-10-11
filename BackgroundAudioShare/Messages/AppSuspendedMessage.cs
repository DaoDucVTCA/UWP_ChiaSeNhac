using System;
using System.Runtime.Serialization;

namespace BackgroundAudioShare.Messages
{
    [DataContract]
    public class AppSuspendedMessage
    {
        public AppSuspendedMessage()
        {
            this.Timestamp = DateTime.Now;
        }

        public AppSuspendedMessage(DateTime timestamp)
        {
            this.Timestamp = timestamp;
        }

        [DataMember]
        public DateTime Timestamp;
    }
}
