using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundAudioShare.Messages
{
    [DataContract]
    public class QualityChangeMessage
    {
        public QualityChangeMessage() { }

        public QualityChangeMessage(string quality)
        {
            this.Quality = quality;
            //this.QualityUrl = qualityUrl;
        }

        //[DataMember]
        //public Uri QualityUrl;
        [DataMember]
        public string Quality;
    }
}
