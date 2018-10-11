using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiasenhacUniversal.Model
{
    public class ListAlbumsModel
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
            public string music_bitrate { get; set; }
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
