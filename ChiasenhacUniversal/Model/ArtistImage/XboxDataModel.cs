using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiasenhacUniversal.Model.ArtistImage
{
    public class XboxDataModel
    {
        public class OtherIds
        {
            public string music_amg { get; set; }
        }

        public class Item
        {
            public List<string> Genres { get; set; }
            public List<string> Subgenres { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
            public string Link { get; set; }
            public OtherIds OtherIds { get; set; }
            public string Source { get; set; }
            public string CompatibleSources { get; set; }
        }

        public class Artists
        {
            public List<Item> Items { get; set; }
            public int TotalItemCount { get; set; }
        }

        public class Artist2
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
            public string Link { get; set; }
            public string Source { get; set; }
            public string CompatibleSources { get; set; }
        }

        public class Artist
        {
            public string Role { get; set; }
            public Artist2 Artists { get; set; }
        }

        public class Item2
        {
            public string ReleaseDate { get; set; }
            public string Duration { get; set; }
            public int TrackCount { get; set; }
            public bool IsExplicit { get; set; }
            public string LabelName { get; set; }
            public List<string> Genres { get; set; }
            public List<string> Subgenres { get; set; }
            public string AlbumType { get; set; }
            public string Subtitle { get; set; }
            public List<Artist> Artists { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
            public string Link { get; set; }
            public string Source { get; set; }
            public string CompatibleSources { get; set; }
        }

        public class Albums
        {
            public List<Item2> Items { get; set; }
            public int TotalItemCount { get; set; }
        }

        public class Album
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
            public string Link { get; set; }
            public string Source { get; set; }
            public string CompatibleSources { get; set; }
        }

        public class Artist4
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
            public string Link { get; set; }
            public string Source { get; set; }
            public string CompatibleSources { get; set; }
        }

        public class Artist3
        {
            public string Role { get; set; }
            public Artist4 Artist { get; set; }
        }

        public class Item3
        {
            public string ReleaseDate { get; set; }
            public string Duration { get; set; }
            public int TrackNumber { get; set; }
            public bool IsExplicit { get; set; }
            public List<string> Genres { get; set; }
            public List<string> Subgenres { get; set; }
            public List<string> Rights { get; set; }
            public Album Album { get; set; }
            public List<Artist3> Artists { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
            public string Link { get; set; }
            public string Source { get; set; }
            public string CompatibleSources { get; set; }
        }

        public class Tracks
        {
            public List<Item3> Items { get; set; }
            public string ContinuationToken { get; set; }
            public int TotalItemCount { get; set; }
        }

        public class RootObject
        {
            public Artists Artists { get; set; }
            public Albums Albums { get; set; }
            public Tracks Tracks { get; set; }
        }
    }
}
