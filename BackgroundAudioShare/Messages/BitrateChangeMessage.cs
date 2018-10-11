using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundAudioShare.Messages
{
    [DataContract]
    public class BitrateChangeMessage
    {
        public BitrateChangeMessage() { }

        public BitrateChangeMessage(string bitrate)
        {
            this.Bitrate = bitrate;
            //this.QualityUrl = qualityUrl;
        }

        //[DataMember]
        //public Uri QualityUrl;
        [DataMember]
        public string Bitrate;
    }
}
