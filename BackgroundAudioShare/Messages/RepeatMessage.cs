using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundAudioShare.Messages
{
    [DataContract]
    public class RepeatMessage
    {
        public RepeatMessage(bool repeat)
        {
            this.isRepeat = repeat;
        }

        [DataMember]
        public bool isRepeat;
    }
}
