using System;
using System.Collections.Generic;

namespace ChiasenhacUniversal.Model
{
    public class SearchResultModel
    {
        public class MusicList
        {
            public int id { get; set; }
            public string thumbnail { get; set; }
            public string music_id { get; set; }
            public string cat_id { get; set; }
            public string cat_level { get; set; }
            public string music_title_url { get; set; }
            public string music_title { get; set; }
            public string music_artist { get; set; }
            public string music_composer { get; set; }
            public string music_album { get; set; }
            public string music_year { get; set; }
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
            public string music_width { get; set; }
            public string music_height { get; set; }
        }

        public class RootObject
        {
            public List<MusicList> music_list { get; set; }
        }
    }
}
