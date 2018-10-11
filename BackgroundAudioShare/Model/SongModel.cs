using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundAudioShare.Model
{
    public class SongModel
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

            public string _music_length;
            public string music_length
            {
                get { return _music_length; }
                set { _music_length = ParseLength(value); }
            }

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
            public string artist_face_url { get; set; }
            public string thumbnail_url { get; set; }

            public Music() { }

            public Music(int _id, string _music_id, string _cat_id, string _cat_level, string _music_title, string _music_artist, string _music_title_url, string _music_download, string _music_bitrate, string _music_width, string _music_height, string _music_length_, string _thumbnail_url)
            {
                id = _id;
                music_id = _music_id;
                cat_id = _cat_id;
                cat_level = _cat_level;
                music_title = _music_title;
                music_artist = _music_artist;
                music_title_url = _music_title_url;
                music_downloads = _music_download;
                music_bitrate = _music_bitrate;
                music_width = _music_width;
                music_height = _music_height;
                _music_length = _music_length_;
                thumbnail_url = _thumbnail_url;
            }


        }

        public class Category
        {
            public string cat_id { get; set; }
            public string title { get; set; }
            public string cat_url { get; set; }
            public List<Music> music { get; set; }
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
            public string cover_img { get; set; }
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
            public string cover_img { get; set; }
            public string music_tracklist { get; set; }
        }

        public class Album
        {
            public int album_total { get; set; }
            public List<AlbumOld> album_old { get; set; }
            public List<AlbumNew> album_new { get; set; }
        }

        public class RootObject
        {
            public List<Category> category { get; set; }
            public Album album { get; set; }
        }
    }
}
