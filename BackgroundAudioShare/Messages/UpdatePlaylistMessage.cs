using BackgroundAudioShare.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundAudioShare.Messages
{
    [DataContract]
    public class UpdatePlaylistMessage
    {
        public UpdatePlaylistMessage(List<SongDetail.MusicInfo> songs)
        {
            this.Songs = songs;
        }

        [DataMember]
        public List<SongDetail.MusicInfo> Songs;
    }
}
