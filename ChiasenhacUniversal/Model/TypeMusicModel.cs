using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiasenhacUniversal.Model
{
    public class TypeMusicModel
    {
        public class Music
        {
            public int id { get; set; }
            public string music_id { get; set; }
            public string cat_id { get; set; }
            public string cat_level { get; set; }
            public string music_title_url { get; set; }
            public string music_title { get; set; }
            public string music_artist { get; set; }
            public string music_downloads { get; set; }
            public string music_listen { get; set; }

            public string _music_bitrate;
            public string music_bitrate
            {
                get { return _music_bitrate; }
                set { _music_bitrate = ParseBitrate(value); }
            }

            public string ParseBitrate(string bitrate)
            {
                if (bitrate == "1000")
                    return "lossless";
                else if (bitrate != "500" || bitrate != "320" || bitrate != "128")
                    return bitrate + "kbps";
                else
                    return bitrate;
            }

            public string music_width { get; set; }
            public string music_height { get; set; }

            private string _music_length;
            public string music_length
            {
                get { return _music_length; }
                set { _music_length = ParseLength(value); }
            }
            public string artist_face_url { get; set; }
            public string thumbnail_url { get; set; }

            public String ParseLength(string musicLength)
            {
                int length = int.Parse(musicLength);
                int minutes = length / 60;
                int seconds = length % 60;
                if (seconds < 10)
                {
                    return minutes + ":" + "0" + seconds;
                }
                return minutes + ":" + seconds;
            }
        }

        public class Hot
        {
            public int hot_total { get; set; }
            public List<Music> music { get; set; }
        }

        public class Music2
        {
            public int id { get; set; }
            public string music_id { get; set; }
            public string cat_id { get; set; }
            public string cat_level { get; set; }
            public string music_title { get; set; }
            public string music_artist { get; set; }
            public string music_title_url { get; set; }
            public string music_downloads { get; set; }

            public string _music_bitrate;
            public string music_bitrate
            {
                get { return _music_bitrate; }
                set { _music_bitrate = ParseBitrate(value); }
            }

            public string ParseBitrate(string bitrate)
            {
                if (bitrate == "1000")
                    return "lossless";
                else if (bitrate != "500" || bitrate != "320" || bitrate != "128")
                    return bitrate + "kbps";
                else
                    return bitrate;
            }

            public string music_width { get; set; }
            public string music_height { get; set; }

            private string _music_length;
            public string music_length
            {
                get { return _music_length; }
                set { _music_length = ParseLength(value); }
            }
            public string thumbnail_url { get; set; }

            public String ParseLength(string musicLength)
            {
                int length = int.Parse(musicLength);
                int minutes = length / 60;
                int seconds = length % 60;
                if (seconds < 10)
                {
                    return minutes + ":" + "0" + seconds;
                }
                return minutes + ":" + seconds;
            }
        }

        public class New
        {
            public int new_total { get; set; }
            public List<Music2> music { get; set; }
        }

        public class AlbumNew
        {
            public string music_id { get; set; }
            public string cat_id { get; set; }
            public string cat_level { get; set; }
            public string cat_sublevel { get; set; }
            public string music_title_url { get; set; }
            public string music_artist { get; set; }
            public string music_album { get; set; }
            public string music_album_id { get; set; }
            public string music_year { get; set; }
            public string music_bitrate { get; set; }
            public string music_length { get; set; }
            public string music_tracklist { get; set; }
            public string cover_img { get; set; }
        }

        public class AlbumOld
        {
            public string music_id { get; set; }
            public string cat_id { get; set; }
            public string cat_level { get; set; }
            public string cat_sublevel { get; set; }
            public string music_title_url { get; set; }
            public string music_artist { get; set; }
            public string music_album { get; set; }
            public string music_album_id { get; set; }
            public string music_year { get; set; }
            public string music_bitrate { get; set; }
            public string music_length { get; set; }
            public string music_tracklist { get; set; }
            public string cover_img { get; set; }
        }

        public class Album
        {
            public int album_total { get; set; }
            public List<AlbumNew> album_new { get; set; }
            public int old_album_total { get; set; }
            public List<AlbumOld> album_old { get; set; }
        }

        public class RootObject
        {
            public int cat_id { get; set; }
            public int cat_level { get; set; }
            public string cat_url { get; set; }
            public Hot hot { get; set; }
            public New @new { get; set; }
            public Album album { get; set; }
        }
    }
}
