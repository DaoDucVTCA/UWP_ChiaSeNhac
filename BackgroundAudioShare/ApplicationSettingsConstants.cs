using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundAudioShare
{
    public static class ApplicationSettingsConstants
    {
        // Data keys
        public const string music_id = "music_id";
        public const string TrackId32 = "trackid32";
        public const string TrackId128 = "trackid128";
        public const string TrackId320 = "trackid320";
        public const string TrackId500 = "trackidm4a";
        public const string Position = "position";
        public const string BackgroundTaskState = "backgroundtaskstate"; // Started, Running, Cancelled
        public const string AppState = "appstate"; // Suspended, Resumed
        public const string AppSuspendedTimestamp = "appsuspendedtimestamp";
        public const string AppResumedTimestamp = "appresumedtimestamp";

    }
}
