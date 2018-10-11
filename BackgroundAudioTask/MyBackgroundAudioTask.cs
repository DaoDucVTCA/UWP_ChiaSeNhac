using BackgroundAudioShare;
using BackgroundAudioShare.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Playback;
using BackgroundAudioShare.Model;
using Windows.Media.Core;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.Storage;

namespace BackgroundAudioTask
{
    public sealed class MyBackgroundAudioTask : IBackgroundTask
    {
        private SystemMediaTransportControls smtc;
        private AppState foregroundAppState = AppState.Unknown;
        private BackgroundTaskDeferral deferral;
        private ManualResetEvent backgroundTaskStarted = new ManualResetEvent(false);
        private bool playbackStartedPreviously = false;
        private static MediaPlaybackList playbackList;
        private const string musicId = "music_id";
        private const string TrackIdKey = "trackid128";
        private const string TitleKey = "title";
        private const string ArtistKey = "artist";
        private const string MusicImageKey = "musicImage";
        private const string ThumbnailKey = "thumbnail";
        private int nowPlayingIndex;
        private int songIndex;
        private bool isRepeat;
        private MediaPlaybackItem item;
        DispatcherTimer timer;
        private string imageUlr;
        List<SongDetail.MusicInfo> nowPlayingList = new List<SongDetail.MusicInfo>();
        int bitrate;
        MediaSource source;

        string GetMusicId(MediaPlaybackItem item)
        {
            if (item == null)
                return null;
            else
                return item.Source.CustomProperties[musicId] as string;
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                smtc = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
                smtc.ButtonPressed += smtc_ButtonPressed;
                smtc.PropertyChanged += smtc_PropertyChanged;
                smtc.IsEnabled = true;
                smtc.IsPauseEnabled = true;
                smtc.IsPlayEnabled = true;
                smtc.IsNextEnabled = true;
                smtc.IsPreviousEnabled = true;

                // Read persisted state of foreground app
                var value = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.AppState);
                if (value == null)
                    foregroundAppState = AppState.Unknown;
                else
                    foregroundAppState = EnumHelper.Parse<AppState>(value.ToString());

                // Add handlers for MediaPlayer
                BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;

                // Initialize message channel 
                BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;

                if (foregroundAppState != AppState.Suspended)
                    MessageService.SendMessageToForeground(new BackgroundAudioTaskStartedMessage());

                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Running.ToString());

                deferral = taskInstance.GetDeferral();

                // Mark the background task as started to unblock SMTC Play operation (see related WaitOne on this signal)
                backgroundTaskStarted.Set();

                // Associate a cancellation and completed handlers with the background task.
                taskInstance.Task.Completed += TaskCompleted;
                taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            }
            catch (Exception)
            {

            }
            
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            try
            {
                // immediately set not running
                backgroundTaskStarted.Reset();

                // save state
                //ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId, GetCurrentTrackId() == null ? null : GetCurrentTrackId().ToString());
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.Position, BackgroundMediaPlayer.Current.Position.ToString());
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Canceled.ToString());
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, Enum.GetName(typeof(AppState), foregroundAppState));

                // unsubscribe from list changes
                if (playbackList != null)
                {
                    //playbackList.CurrentItemChanged -= PlaybackList_CurrentItemChanged;
                    playbackList = null;
                }

                // unsubscribe event handlers
                BackgroundMediaPlayer.MessageReceivedFromForeground -= BackgroundMediaPlayer_MessageReceivedFromForeground;
                smtc.ButtonPressed -= smtc_ButtonPressed;
                smtc.PropertyChanged -= smtc_PropertyChanged;

                BackgroundMediaPlayer.Shutdown(); 
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            deferral.Complete(); 
        }

        private void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            try
            {
                item = args.NewItem;

                if (item == null)
                {
                    smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    smtc.DisplayUpdater.MusicProperties.Title = string.Empty;
                    smtc.DisplayUpdater.Update();
                    return;
                }
                else
                {
                    smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
                    smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
                    smtc.DisplayUpdater.MusicProperties.Title = item.Source.CustomProperties[TitleKey] as string;
                    smtc.DisplayUpdater.MusicProperties.Artist = item.Source.CustomProperties[ArtistKey] as string;
                    imageUlr = item.Source.CustomProperties[MusicImageKey] as string;
                }

                smtc.DisplayUpdater.Update();

                // Get the current track
                string currentMusicId = null;
                if (item != null)
                {
                    currentMusicId = item.Source.CustomProperties[musicId].ToString();
                }

                if (foregroundAppState == AppState.Active)
                    MessageService.SendMessageToForeground(new TrackChangedMessage(currentMusicId));
                else
                    ApplicationSettingsHelper.SaveSettingsValue(TrackIdKey, currentMusicId == null ? null : currentMusicId);
            }
            catch
            { }

        }

        private void TaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            deferral.Complete();
        }

        public async void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            StartPlaybackMessage startPlaybackMessage;
            if (MessageService.TryParseMessage(e.Data, out startPlaybackMessage))
            {
                StartPlayback();
                return;
            }

            UpdatePlaylistMessage updatePlaylistMessage;
            if (MessageService.TryParseMessage(e.Data, out updatePlaylistMessage))
            {
                CreatePlaybackList(updatePlaylistMessage.Songs);
                nowPlayingList = updatePlaylistMessage.Songs;
                return;
            }

            TrackChangedMessage trackChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out trackChangedMessage))
            {
                
                try
                {
                    songIndex = playbackList.Items.ToList().FindIndex(item => GetMusicId(item).ToString() == (string)trackChangedMessage.MusicId);

                    Task.Delay(1000);
                    smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    playbackList.MoveTo((uint)songIndex);

                    BackgroundMediaPlayer.Current.Play();
                    return;

                }
                catch
                {
                    songIndex = playbackList.Items.ToList().FindIndex(item => GetMusicId(item).ToString() == (string)trackChangedMessage.MusicId);
                    PlaySongs();
                }
            }

            SkipNextMessage skipNextMessage;
            if (MessageService.TryParseMessage(e.Data, out skipNextMessage))
            {
                
                SkipToNext();
                return;
            }

            SkipPreviousMessage skipPreviousMessage;
            if (MessageService.TryParseMessage(e.Data, out skipPreviousMessage))
            {
                // User has chosen to skip track from app context.
                Debug.WriteLine("Skipping to previous");
                SkipToPrevious();
                return;
            }

            QualityChangeMessage qualityChangeMessage;
            if(MessageService.TryParseMessage(e.Data, out qualityChangeMessage))
            {
                try
                {
                    switch(qualityChangeMessage.Quality)
                    {
                        case "32kbs":
                            bitrate = 32;
                            CreatePlaybackList(nowPlayingList);
                            break;
                        case "128kbs":
                            bitrate = 128;
                            CreatePlaybackList(nowPlayingList);
                            break;
                        case "320kbs":
                            bitrate = 320;
                            CreatePlaybackList(nowPlayingList);
                            break;
                        case "m4a":
                            bitrate = 500;
                            CreatePlaybackList(nowPlayingList);
                            break;
                        case "flac":
                            bitrate = 1000;
                            CreatePlaybackList(nowPlayingList);
                            break;
                    }

                    smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                    await Task.Delay(100);
                    playbackList.MoveTo((uint)nowPlayingIndex);
                   
                        
                    // TODO: Work around playlist bug that doesn't continue playing after a switch; remove later
                    BackgroundMediaPlayer.Current.Play();
                    //string a = BackgroundMediaPlayer.Current.
                    return;
                }
                catch(Exception ex)
                {
                    ex.ToString();
                }
            }

            BitrateChangeMessage bitrateChangeMessage;
            if (MessageService.TryParseMessage(e.Data, out bitrateChangeMessage))
            {
                try
                {
                    switch (bitrateChangeMessage.Bitrate)
                    {
                        case "32kbs":
                            bitrate = 32;
                            break;
                        case "128kbs":
                            bitrate = 128;
                            break;
                        case "320kbs":
                            bitrate = 320;
                            break;
                        case "m4a":
                            bitrate = 500;
                            break;
                        case "flac":
                            bitrate = 1000;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }
            ChangedPlaylistMessage changedPlaylist;
            if(MessageService.TryParseMessage(e.Data, out changedPlaylist))
            {
                ChangePlaylist(changedPlaylist.Songs);
                nowPlayingList = changedPlaylist.Songs;
            }

            RepeatMessage repeatMessage;
            if(MessageService.TryParseMessage(e.Data, out repeatMessage))
            {
                if(repeatMessage.isRepeat == true)
                {
                    BackgroundMediaPlayer.Current.IsLoopingEnabled = true;
                }
                else
                {
                    BackgroundMediaPlayer.Current.IsLoopingEnabled = false;
                }
            }

            ShuffleMessage shuffleMessage;
            if (MessageService.TryParseMessage(e.Data, out shuffleMessage))
            {
                if (shuffleMessage.isShuffle == true)
                {
                    playbackList.ShuffleEnabled = true;
                }
                else
                {
                    playbackList.ShuffleEnabled = false;
                }
            }
        }

        public async void PlaySongs()
        {
            try
            {
                BackgroundMediaPlayer.Current.Source = playbackList;
                playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
                await Task.Delay(1000);
                smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                playbackList.MoveTo((uint)songIndex);

                BackgroundMediaPlayer.Current.Play();
            }
            catch (Exception)
            {

            }
        }

        private void SkipToPrevious()
        {
            smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
            playbackList.MovePrevious();
            nowPlayingIndex = (int)playbackList.CurrentItemIndex;
            BackgroundMediaPlayer.Current.Play();
        }

        private void SkipToNext()
        {
            try
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Changing;

                playbackList.MoveNext();
                nowPlayingIndex = (int)playbackList.CurrentItemIndex;
                BackgroundMediaPlayer.Current.Play();
            }
            catch(Exception ex)
            {
                ex.ToString();
            }
        }


        void ChangePlaylist(List<SongDetail.MusicInfo> songs)
        {
            
            try
            {
                playbackList = new MediaPlaybackList();


                foreach (var song in songs)
                {
                    switch(bitrate)
                    {
                        case 0:
                            source = MediaSource.CreateFromUri(new Uri(song.file_url, UriKind.RelativeOrAbsolute));
                            break;
                        case 32:
                            source = MediaSource.CreateFromUri(new Uri(song.file_32_url, UriKind.RelativeOrAbsolute));
                            break;
                        case 128:
                            source = MediaSource.CreateFromUri(new Uri(song.file_url, UriKind.RelativeOrAbsolute));
                            break;
                        case 320:
                            if(song.file_320_url != "")
                            {
                                source = MediaSource.CreateFromUri(new Uri(song.file_320_url, UriKind.RelativeOrAbsolute));
                            }
                            else
                            {
                                source = MediaSource.CreateFromUri(new Uri(song.file_url, UriKind.RelativeOrAbsolute));
                            }
                            break;
                        case 500:
                            if (song.file_m4a_url != "")
                            {
                                source = MediaSource.CreateFromUri(new Uri(song.file_m4a_url, UriKind.RelativeOrAbsolute));
                            }
                            else
                            {
                                source = MediaSource.CreateFromUri(new Uri(song.file_url, UriKind.RelativeOrAbsolute));
                            }
                            break;
                        case 1000:
                            if (song.file_lossless_url != "")
                            {
                                source = MediaSource.CreateFromUri(new Uri(song.file_lossless_url, UriKind.RelativeOrAbsolute));
                            }
                            else
                            {
                                source = MediaSource.CreateFromUri(new Uri(song.file_url, UriKind.RelativeOrAbsolute));
                            }
                            break;
                    }

                    source.CustomProperties[musicId] = song.music_id;
                    source.CustomProperties[TitleKey] = song.music_title;
                    source.CustomProperties[ArtistKey] = song.music_artist;
                    source.CustomProperties[MusicImageKey] = song.music_img;
                    playbackList.Items.Add(new MediaPlaybackItem(source));

                }

                playbackList.AutoRepeatEnabled = true;

                BackgroundMediaPlayer.Current.AutoPlay = false;
            }
            catch (Exception)
            {

            }

        }

        void CreatePlaybackList(List<SongDetail.MusicInfo> songs)
        {
            try
            {
                playbackList = new MediaPlaybackList();


                foreach (var song in songs)
                {
                    switch (bitrate)
                    {
                        case 0:
                            source = MediaSource.CreateFromUri(new Uri(song.file_url, UriKind.RelativeOrAbsolute));
                            break;
                        case 32:
                            source = MediaSource.CreateFromUri(new Uri(song.file_32_url, UriKind.RelativeOrAbsolute));
                            break;
                        case 128:
                            source = MediaSource.CreateFromUri(new Uri(song.file_url, UriKind.RelativeOrAbsolute));
                            break;
                        case 320:
                            if (song.file_320_url != "")
                            {
                                source = MediaSource.CreateFromUri(new Uri(song.file_320_url, UriKind.RelativeOrAbsolute));
                            }
                            else
                            {
                                source = MediaSource.CreateFromUri(new Uri(song.file_url, UriKind.RelativeOrAbsolute));
                            }
                            break;
                        case 500:
                            if (song.file_m4a_url != "")
                            {
                                source = MediaSource.CreateFromUri(new Uri(song.file_m4a_url, UriKind.RelativeOrAbsolute));
                            }
                            else
                            {
                                source = MediaSource.CreateFromUri(new Uri(song.file_url, UriKind.RelativeOrAbsolute));
                            }
                            break;
                        case 1000:
                            if (song.file_lossless_url != "")
                            {
                                source = MediaSource.CreateFromUri(new Uri(song.file_lossless_url, UriKind.RelativeOrAbsolute));
                            }
                            else
                            {
                                source = MediaSource.CreateFromUri(new Uri(song.file_url, UriKind.RelativeOrAbsolute));
                            }
                            break;
                    }
                    source.CustomProperties[musicId] = song.music_id;
                    source.CustomProperties[TitleKey] = song.music_title;
                    source.CustomProperties[ArtistKey] = song.music_artist;
                    source.CustomProperties[MusicImageKey] = song.music_img;
                    playbackList.Items.Add(new MediaPlaybackItem(source));
                }

                playbackList.AutoRepeatEnabled = true;
                BackgroundMediaPlayer.Current.AutoPlay = false;

                // Assign the list to the player
                BackgroundMediaPlayer.Current.Source = playbackList;

                playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
            }
            catch (Exception)
            {

            }

        }

        private void StartPlayback()
        {
            try
            {
                if (!playbackStartedPreviously)
                {
                    playbackStartedPreviously = true;

                    //var currentTrackId = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.TrackId128);
                    var currentTrackPosition = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.Position);
                    var currentMusicid = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.music_id);

                    if (currentMusicid != null)
                    {
                        var a = playbackList.Items.Count;
                        var index = playbackList.Items.ToList().FindIndex(item =>
                            GetMusicId(item).ToString() == (string)currentMusicid);
                        nowPlayingIndex = index;
                        TypedEventHandler<MediaPlaybackList, CurrentMediaPlaybackItemChangedEventArgs> handler = null;

                        handler = (MediaPlaybackList list, CurrentMediaPlaybackItemChangedEventArgs args) =>
                        {
                            if (args.NewItem == playbackList.Items[index])
                            {
                                // Unsubscribe because this only had to run once for this item
                                playbackList.CurrentItemChanged -= handler;

                                //Set position
                                try
                                {
                                    var position = TimeSpan.Parse((string)currentTrackPosition);
                                    //Debug.WriteLine("StartPlayback: Setting Position " + position);
                                    BackgroundMediaPlayer.Current.Position = position;
                                }
                                catch { }

                                // Begin playing
                                BackgroundMediaPlayer.Current.Play();
                            }
                        };

                        playbackList.CurrentItemChanged += handler;

                        playbackList.MoveTo((uint)index);
                    }
                    else
                    {
                        // Begin playing
                        BackgroundMediaPlayer.Current.Play();
                    }
                }
                else
                {
                    // Begin playing
                    BackgroundMediaPlayer.Current.Play();
                }
            }
            catch(Exception ex)
            {
                ex.ToString();
            }
        }

        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (sender.CurrentState == MediaPlayerState.Paused)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
            else if (sender.CurrentState == MediaPlayerState.Closed)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
            }
            else if (sender.CurrentState == MediaPlayerState.Opening)
            {
                BackgroundMediaPlayer.Current.Play();
            }
        }

        private void smtc_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            //throw new NotImplementedException();
        }

        private void smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    bool result = backgroundTaskStarted.WaitOne(5000);
                    
                    BackgroundMediaPlayer.Current.Play();

                    StartPlayback();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    try
                    {
                        BackgroundMediaPlayer.Current.Pause();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                    break;
                case SystemMediaTransportControlsButton.Next:
                    SkipToNext();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Debug.WriteLine("UVC previous button pressed");
                    SkipToPrevious();
                    break;
            }
        }
    }
}
