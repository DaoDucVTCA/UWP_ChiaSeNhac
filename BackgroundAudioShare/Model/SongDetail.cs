using System.Collections.Generic;

namespace BackgroundAudioShare.Model
{
    public class SongDetail
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
            public string music_length { get; set; }       
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

            //public MusicInfo() { }

            //public MusicInfo(string _music_id, string _cat_id, string _cat_level, string _music_title_url, string _music_title, string _music_artist, string _music_production, string _music_album_id, string _music_year, string _music_listen, string _music_download, string _music_time, string _music_bitrate, string _music_lenght, string _music_32_filesize,string _music_filesize, string _music_320_filesize, string _music_m4a_filesize, string _music_lossless_filesize, string _music_width, string _music_height, string _music_username, string _music_lyric, string _music_img_height, string _music_img_width, string _music_img, string _video_thumbnail, string _video_preview, string _file_url, string _file_32_url, string _file_320_url, string _file_m4a_url, string _file_lossless_url, string _full_url, string _music_genre)
            //{
            //    music_id = _music_id;
            //    cat_id = _cat_id;
            //    cat_level = _cat_level;
            //    music_title_url = _music_title_url;
            //    music_title = _music_title;
            //    music_artist = _music_artist;
            //    music_production = _music_production;
            //    music_album_id = _music_album_id;
            //    music_year = _music_year;
            //    music_listen = _music_listen;
            //    music_downloads = _music_download;
            //    music_time = _music_time;
            //    music_bitrate = _music_bitrate;
            //    music_length = _music_lenght;
            //    music_32_filesize = _music_32_filesize;
            //    music_filesize = _music_filesize;
            //    music_320_filesize = _music_320_filesize;
            //    music_m4a_filesize = _music_m4a_filesize;
            //    music_lossless_filesize = _music_lossless_filesize;
            //    music_width = _music_width;
            //    music_height = _music_height;
            //    music_username = _music_username;
            //    music_lyric = _music_lyric;
            //    music_img_height = _music_img_height;
            //    music_img_width = _music_img_width;
            //    music_img = _music_img;
            //    video_thumbnail = _video_thumbnail;
            //    video_preview = video_preview;
            //    file_url = _file_url;
            //    file_32_url = _file_32_url;
            //    file_320_url = _file_320_url;
            //    file_m4a_url = _file_m4a_url;
            //    file_lossless_url = _file_lossless_url;
            //    full_url = _full_url;
            //    music_genre = _music_genre;
            //}
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

        public class TrackList
        {
            public string music_id { get; set; }
            public string cat_id { get; set; }
            public string cat_level { get; set; }
            public string cat_custom { get; set; }
            public string music_title_url { get; set; }
            public string music_title { get; set; }
            public string music_artist { get; set; }
            public string music_track_id { get; set; }
            public string music_shortlyric { get; set; }
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
            public List<TrackList> track_list { get; set; }
            public Related related { get; set; }
            public Recent recent { get; set; }
            public Artist artist { get; set; }
        }
    }
}
