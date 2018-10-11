using System.Runtime.Serialization;

namespace BackgroundAudioShare.Messages
{
    [DataContract]
    public class TrackChangedMessage
    {
        public TrackChangedMessage()
        {
        }

        public TrackChangedMessage(string musicId)
        {
            this.MusicId = musicId;
        }

        [DataMember]
        public string MusicId;
    }
}
