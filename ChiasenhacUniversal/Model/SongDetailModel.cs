using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiasenhacUniversal.Model
{
    public class SongDetailModel
    {
        public class MusicInfo
        {
            public string music_id { get; set; }
            public string cat_id { get; set; }
            public string cat_level { get; set; }
            public string cat_sublevel { get; set; }
            public string cover_id { get; set; }
            public string music_title_url { get; set; }
            public string music_title { get; set; }
            public string music_artist { get; set; }
            public string music_composer { get; set; }
            public string music_album { get; set; }
            public string music_production { get; set; }
            public string music_album_id { get; set; }
            public string music_year { get; set; }
            public string music_listen { get; set; }
            public string music_downloads { get; set; }
            public string music_time { get; set; }
            public string music_bitrate { get; set; }

            private string _music_length;
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
            public string music_32_filesize { get; set; }
            public string music_filesize { get; set; }
            public string music_320_filesize { get; set; }
            public string music_m4a_filesize { get; set; }
            public string music_lossless_filesize { get; set; }
            public string music_width { get; set; }
            public string music_height { get; set; }
            public string music_username { get; set; }
            public string music_lyric { get; set; }
            public string music_img_height { get; set; }
            public string music_img_width { get; set; }
            public string music_img { get; set; }
            public string video_thumbnail { get; set; }
            public string video_preview { get; set; }
            public string file_url { get; set; }
            public string file_32_url { get; set; }
            public string file_320_url { get; set; }
            public string file_m4a_url { get; set; }
            public string file_lossless_url { get; set; }
            public string full_url { get; set; }
            public string music_genre { get; set; }
        }

        public class MusicList
        {
            public string music_id { get; set; }
            public string cat_id { get; set; }
            public string cat_level { get; set; }
            public string music_title_url { get; set; }
            public string music_title { get; set; }
            public string music_artist { get; set; }
            public string music_bitrate { get; set; }
            public string music_length { get; set; }
            public string music_width { get; set; }
            public string music_height { get; set; }
            public string music_thumbs_time { get; set; }
            public string music_downloads { get; set; }
            public string thumbnail_url { get; set; }
        }

        public class Related
        {
            public string music_total { get; set; }
            public List<MusicList> music_list { get; set; }
        }

        public class MusicList2
        {
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
            public string music_length { get; set; }
            public string music_downloads_today { get; set; }
            public string music_thumbs_time { get; set; }
            public string music_deleted { get; set; }
            public string thumbnail_url { get; set; }
        }

        public class Recent
        {
            public int music_total { get; set; }
            public List<MusicList2> music_list { get; set; }
        }

        public class MusicList3
        {
            public string music_id { get; set; }
            public string cat_id { get; set; }
            public string cat_level { get; set; }
            public string music_title_url { get; set; }
            public string music_title { get; set; }
            public string music_artist { get; set; }
            public string music_bitrate { get; set; }
            public string music_length { get; set; }
            public string music_width { get; set; }
            public string music_height { get; set; }
            public string music_thumbs_time { get; set; }
            public string music_downloads { get; set; }
            public string thumbnail_url { get; set; }
            public string cat_sublevel { get; set; }
            public string cat_custom { get; set; }
            public string cover_id { get; set; }
            public string music_download_time { get; set; }
            public string music_last_update_time { get; set; }
            public string music_title_search { get; set; }
            public string music_artist_search { get; set; }
            public string music_composer_search { get; set; }
            public string music_album_search { get; set; }
            public string music_composer { get; set; }
            public string music_album { get; set; }
            public string music_production { get; set; }
            public string music_album_id { get; set; }
            public string music_track_id { get; set; }
            public string music_year { get; set; }
            public string music_code_1 { get; set; }
            public string music_code_2 { get; set; }
            public string music_listen { get; set; }
            public string music_downloads_today { get; set; }
            public string music_downloads_today_0 { get; set; }
            public string music_downloads_this_week { get; set; }
            public string music_downloads_max_week { get; set; }
            public string music_time { get; set; }
            public string music_filename { get; set; }
            public string music_32_filesize { get; set; }
            public string music_filesize { get; set; }
            public string music_320_filesize { get; set; }
            public string music_m4a_filesize { get; set; }
            public string music_lossless_filesize { get; set; }
            public string music_user_id { get; set; }
            public string music_username { get; set; }
            public string music_spectrum { get; set; }
            public string music_lyric { get; set; }
            public string music_note { get; set; }
            public string music_deleted { get; set; }
            public string music_tracklist { get; set; }
            public string album_artist { get; set; }
        }

        public class Artist
        {
            public string music_total { get; set; }
            public List<MusicList3> music_list { get; set; }
        }

        public class RootObject
        {
            public MusicInfo music_info { get; set; }
            public Related related { get; set; }
            public Recent recent { get; set; }
            public Artist artist { get; set; }
        }
    }
}
