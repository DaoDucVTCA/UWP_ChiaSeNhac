using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiasenhacUniversal.Model
{
    public class SearchAlbumResultModel
    {
        public class AlbumList
        {
            public int id { get; set; }
            public string cover_img { get; set; }
            public string music_id { get; set; }
            public string cat_id { get; set; }
            public string cat_level { get; set; }
            public string music_title_url { get; set; }
            public string music_title { get; set; }
            public string music_artist { get; set; }
            public string music_composer { get; set; }
            public string music_album { get; set; }
            public string music_album_id { get; set; }
            public string music_year { get; set; }
            public string music_downloads { get; set; }
            public string music_bitrate { get; set; }
            public string music_length { get; set; }
            public string music_width { get; set; }
            public string music_height { get; set; }
            public string music_tracklist { get; set; }
        }

        public class RootObject
        {
            public List<AlbumList> album_list { get; set; }
        }
    }
}
