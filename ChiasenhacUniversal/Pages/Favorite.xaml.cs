using BackgroundAudioShare;
using BackgroundAudioShare.Messages;
using BackgroundAudioShare.Model;
using ChiasenhacUniversal.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Windows.ApplicationModel.Store;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ChiasenhacUniversal.Pages
{
    public sealed partial class Favorite : Page
    {
        private bool isMyBackgroundTaskRunning = false;
        private AutoResetEvent backgroundAudioTaskStarted;
        public List<SongDetail.MusicInfo> listFavoriteSongs = new List<SongDetail.MusicInfo>();
        public bool isListenedSong = false;

        private bool IsMyBackgroundTaskRunning
        {
            get
            {
                if (isMyBackgroundTaskRunning)
                    return true;

                string value = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.BackgroundTaskState) as string;
                if (value == null)
                {
                    return false;
                }
                else
                {
                    try
                    {
                        isMyBackgroundTaskRunning = EnumHelper.Parse<BackgroundTaskState>(value) == BackgroundTaskState.Running;
                    }
                    catch (ArgumentException)
                    {
                        isMyBackgroundTaskRunning = false;
                    }
                    return isMyBackgroundTaskRunning;
                }
            }
        }

        public Favorite()
        {
            this.InitializeComponent();

            try
            {
                var a = Convert.ToInt32(App.themeColor.Values["color"]);
                if (a == 0)
                {
                    this.RequestedTheme = ElementTheme.Light;
                    this.UpdateLayout();
                }
                else if (a == 1)
                {
                    this.RequestedTheme = ElementTheme.Dark;
                    this.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }


            backgroundAudioTaskStarted = new AutoResetEvent(false);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            bool isFileExist = await WriteAndReadFile.CheckFileExist("ListFavoriteSongs");

            if (isFileExist == true)
            {
                string loveSongData = await WriteAndReadFile.ReadFile("ListFavoriteSongs");
                listFavoriteSongs = JsonConvert.DeserializeObject<List<SongDetail.MusicInfo>>(loveSongData);

                cvListFavorite.Source = listFavoriteSongs;

                GetAllSong(listFavoriteSongs);
            }
            try
            {
                if (CurrentApp.LicenseInformation.ProductLicenses["Chiasenhac"].IsActive)
                {
                    //windowsphone.Visibility = Visibility.Collapsed;
                    //windows.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
            }
        }

        async void GetAllSong(List<SongDetail.MusicInfo> songs)
        {
            try
            {
                BXH.listAllSongs.Clear();
                foreach (var song in songs)
                {
                    string url = "http://chiasenhac.com/api/listen.php?code=duc_wp_2014&return=json&m=" + song.music_id + "&url=" + song.music_title_url;

                    HttpClient httpClient = new HttpClient();
                    string results = await httpClient.GetStringAsync(new Uri(url));

                    SongDetail.RootObject ro = JsonConvert.DeserializeObject<SongDetail.RootObject>(results);

                    BXH.listAllSongs.Add(ro.music_info);

                    for (int i = 0; i < BXH.listAllSongs.Count; i++)
                    {
                        if (BXH.listAllSongs[i].music_bitrate == "1000")
                        {
                            BXH.listAllSongs[i].file_lossless_url = BXH.listAllSongs[i].file_32_url.Replace(".m4a", " [FLAC Lossless].flac").Replace("/32/", "/flac/");
                        }
                    }
                }

                MessageService.SendMessageToBackground(new ChangedPlaylistMessage(BXH.listAllSongs));
            }
            catch (Exception)
            {
            }

        }

        private void grvFavoriteSongs_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var panel = (ItemsWrapGrid)grvFavoriteSongs.ItemsPanelRoot;

            if (e.NewSize.Width < 433)
            {
                panel.ItemWidth = e.NewSize.Width / 3;
            }
            else
            {
                panel.ItemWidth = e.NewSize.Width / 6;
            }
        }

        private async void grvFavoriteSongs_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                SongDetail.MusicInfo itemSelected = (SongDetail.MusicInfo)e.ClickedItem;

                for (int i = 0; i < BXH.listAllSongs.Count; i++)
                {
                    if (itemSelected.music_id == BXH.listAllSongs[i].music_id)
                    {
                        if (BXH.listAllSongs[i].music_img == "")
                        {
                            BXH.listAllSongs[i].music_img = "ms-appx:///Images/logo.png";
                        }

                        if (BXH.listAllSongs[i].music_bitrate == "1000")
                        {
                            BXH.listAllSongs[i].file_lossless_url = BXH.listAllSongs[i].file_32_url.Replace(".m4a", " [FLAC Lossless].flac").Replace("/32/", "/flac/");
                        }

                        Home.nowPlayingSong = BXH.listAllSongs[i];
                        break;
                    }
                }

                BXH.CheckListenedSong();

                var bg = IsMyBackgroundTaskRunning;
                bool a = isMyBackgroundTaskRunning;
                var b = BackgroundMediaPlayer.Current.CurrentState;

                if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
                {
                    MessageService.SendMessageToBackground(new TrackChangedMessage(Home.nowPlayingSong.music_id));
                }
                else if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
                {
                    MessageService.SendMessageToBackground(new TrackChangedMessage(Home.nowPlayingSong.music_id));
                }
                else if (isMyBackgroundTaskRunning == false || MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
                {
                    if (Home.nowPlayingSong.file_32_url != null)
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId32, Home.nowPlayingSong.file_32_url);
                    }
                    if (Home.nowPlayingSong.file_url != null)
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId128, Home.nowPlayingSong.file_url);
                    }
                    if (Home.nowPlayingSong.file_320_url != null)
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId320, Home.nowPlayingSong.file_320_url);
                    }
                    if (Home.nowPlayingSong.file_m4a_url != null)
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId500, Home.nowPlayingSong.file_m4a_url);
                    }

                    ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.music_id, Home.nowPlayingSong.music_id);

                    ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.Position, new TimeSpan().ToString());

                    StartBackgroundAudioTask();
                }
                else
                {
                    MessageService.SendMessageToBackground(new TrackChangedMessage(Home.nowPlayingSong.music_id));
                }

                if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
                {
                    BackgroundMediaPlayer.Current.Play();
                }

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var mainPage = (MainPage)(Window.Current.Content as Frame).Content;
                    if (mainPage.nowPlaying.Visibility == Visibility.Collapsed)
                    {
                        mainPage.nowPlaying.Visibility = Visibility.Visible;
                    }
                    mainPage.songTitle.Text = Home.nowPlayingSong.music_title;
                    mainPage.songArtist.Text = Home.nowPlayingSong.music_artist;
                    mainPage.songThumbnail.Source = new BitmapImage(new Uri(Home.nowPlayingSong.music_img, UriKind.RelativeOrAbsolute));
                    mainPage.sybPlay.Symbol = Symbol.Pause;
                });

                for (int i = 0; i < listFavoriteSongs.Count; i++)
                {
                    BXH.listNowPlaying.Add(new SongModel.Music(i, listFavoriteSongs[i].music_id, listFavoriteSongs[i].cat_id, listFavoriteSongs[i].cat_level, listFavoriteSongs[i].music_title, listFavoriteSongs[i].music_artist, listFavoriteSongs[i].music_title_url, listFavoriteSongs[i].music_downloads, listFavoriteSongs[i].music_bitrate, listFavoriteSongs[i].music_width, listFavoriteSongs[i].music_height, listFavoriteSongs[i].music_length, listFavoriteSongs[i].music_img));
                }

                WriteAndReadFile.SaveListNowPlaying(BXH.listAllSongs);
            }
            catch (Exception)
            {

            }
        }

        #region BackgroudTask
        private async void StartBackgroundAudioTask()
        {
            try
            {
                AddMediaPlayerEventHandlers();

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    MessageService.SendMessageToBackground(new UpdatePlaylistMessage(BXH.listAllSongs));
                    MessageService.SendMessageToBackground(new StartPlaybackMessage());
                });
            }
            catch { }
        }

        private void AddMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.MessageReceivedFromBackground += this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            BackgroundAudioTaskStartedMessage backgroundAudioTaskStartedMessage;
            if (MessageService.TryParseMessage(e.Data, out backgroundAudioTaskStartedMessage))
            {
                backgroundAudioTaskStarted.Set();
                return;
            }
        }
        #endregion
    }
}
