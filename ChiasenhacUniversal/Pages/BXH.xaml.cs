using BackgroundAudioShare;
using BackgroundAudioShare.Messages;
using BackgroundAudioShare.Model;
using ChiasenhacUniversal.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Media.Playback;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace ChiasenhacUniversal.Pages
{
    public sealed partial class BXH : Page
    {
        public SongModel.RootObject ro;
        private bool isMyBackgroundTaskRunning = false;
        private AutoResetEvent backgroundAudioTaskStarted;
        public int categoryNumber;
        public static List<SongDetail.MusicInfo> listAllSongs = new List<SongDetail.MusicInfo>();
        public static List<SongModel.Music> listNowPlaying = new List<SongModel.Music>();
        public bool isListenedSong = false;
        public string downloadLink32;
        public string downloadLink128;
        public string downloadLink320;
        public string downloadLinkm4a;
        public string downloadLink;
        DownloadOperation _download;
        public StorageFile destinationFile;
        public static string fileName;

        string GetTrackId(SongDetail.MusicInfo item)
        {
            if (item == null)
                return null;
            var a = item.file_url as string;
            return a;
        }

        string GetMusicId(SongDetail.MusicInfo item)
        {
            if (item == null)
                return null;
            else
                return item.music_id;
        }

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

        public BXH()
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
            catch(Exception ex)
            {
                ex.ToString();
            }

            LoadData();

            backgroundAudioTaskStarted = new AutoResetEvent(false);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            prLoading.Visibility = Visibility.Visible;
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Active.ToString());
            try
            {
                if (CurrentApp.LicenseInformation.ProductLicenses["Chiasenhac"].IsActive)
                {
                    //windows.Visibility = Visibility.Collapsed;
                    //windowsphone.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
            }
        }

        public async void LoadData ()
        {
            try
            {
                var mainPage = (MainPage)(Window.Current.Content as Frame).Content;

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mainPage.nowPlayingInfo.IsTapEnabled = false;
                });

                string url = "http://chiasenhac.com/api/home.php?code=duc_wp_2014&return=json";

                HttpClient httpClient = new HttpClient();
                string results = await httpClient.GetStringAsync(new Uri(url));

                ro = JsonConvert.DeserializeObject<SongModel.RootObject>(results);

                lvVideo.ItemsSource = ro.category[0].music;

                prLoading.Visibility = Visibility.Collapsed;

                mainPage.nowPlayingInfo.IsTapEnabled = true;
            }
            catch
            { }
        }

       
        private void griTitleMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            prLoading.Visibility = Visibility.Visible;
            MenuFlyoutItem item = sender as MenuFlyoutItem;

            switch(item.Text)
            {
                case "BXH Video":
                    lvMusic.Visibility = Visibility.Collapsed;
                    lvVideo.Visibility = Visibility.Visible;
                    lvVideo.ItemsSource = ro.category[0].music;
                    listNowPlaying = ro.category[0].music;
                    GetAllSong(ro.category[0].music);
                    categoryNumber = 0;
                    tblBXHTitle.Text = "BXH Video";
                    break;
                case "BXH Beat, Playback":
                    lvMusic.Visibility = Visibility.Visible;
                    lvVideo.Visibility = Visibility.Collapsed;
                    lvMusic.ItemsSource = ro.category[1].music;
                    listNowPlaying = ro.category[1].music;
                    GetAllSong(ro.category[1].music);
                    categoryNumber = 1;
                    tblBXHTitle.Text = "BXH Beat, Playback";
                    break;
                case "BXH Nhạc Việt Nam":
                    tblBXHTitle.Text = "BXH Nhạc Việt Nam";
                    lvMusic.Visibility = Visibility.Visible;
                    lvVideo.Visibility = Visibility.Collapsed;
                    lvMusic.ItemsSource = ro.category[2].music;
                    listNowPlaying = ro.category[2].music;
                    GetAllSong(ro.category[2].music);
                    categoryNumber = 2;

                    break;
                case "BXH Nhạc US-UK":
                    tblBXHTitle.Text = "BXH Nhạc US-UK";
                    lvMusic.Visibility = Visibility.Visible;
                    lvVideo.Visibility = Visibility.Collapsed;
                    lvMusic.ItemsSource = ro.category[3].music;
                    listNowPlaying = ro.category[3].music;
                    GetAllSong(ro.category[3].music);
                    categoryNumber = 3;
                    break;
                case "BXH Nhạc Hàn":
                    tblBXHTitle.Text = "BXH Nhạc Hàn";
                    lvMusic.Visibility = Visibility.Visible;
                    lvVideo.Visibility = Visibility.Collapsed;
                    lvMusic.ItemsSource = ro.category[5].music;
                    listNowPlaying = ro.category[5].music;
                    GetAllSong(ro.category[5].music);
                    categoryNumber = 5;
                    break;
                case "BXH Nhạc Hoa":
                    tblBXHTitle.Text = "BXH Nhạc Hoa";
                    lvMusic.Visibility = Visibility.Visible;
                    lvVideo.Visibility = Visibility.Collapsed;
                    lvMusic.ItemsSource = ro.category[4].music;
                    listNowPlaying = ro.category[4].music;
                    GetAllSong(ro.category[4].music);
                    categoryNumber = 4;
                    break;
            }
        }

        async void GetAllSong(List<SongModel.Music> songs)
        {
            try
            {
                var mainPage = (MainPage)(Window.Current.Content as Frame).Content;

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mainPage.nowPlayingInfo.IsTapEnabled = false;
                });

                lvMusic.IsEnabled = false;
                listAllSongs.Clear();
                foreach (var song in songs)
                {
                    string url = "http://chiasenhac.com/api/listen.php?code=duc_wp_2014&return=json&m=" + song.music_id + "&url=" + song.music_title_url;

                    HttpClient httpClient = new HttpClient();
                    string results = await httpClient.GetStringAsync(new Uri(url));

                    SongDetail.RootObject ro = JsonConvert.DeserializeObject<SongDetail.RootObject>(results);

                    listAllSongs.Add(ro.music_info);
                }

                for (int i = 0; i < listAllSongs.Count; i++)
                {
                    if (listAllSongs[i].music_img == "")
                    {
                        listAllSongs[i].music_img = "ms-appx:///Images/logo.png";
                    }

                    if (listAllSongs[i].music_bitrate == "1000")
                    {
                        listAllSongs[i].file_lossless_url = listAllSongs[i].file_32_url.Replace(".m4a", "%20[FLAC Lossless].flac").Replace("/32/", "/flac/");
                    }
                }

                MessageService.SendMessageToBackground(new ChangedPlaylistMessage(BXH.listAllSongs));
                lvMusic.IsEnabled = true;
                prLoading.Visibility = Visibility.Collapsed;

                mainPage.nowPlayingInfo.IsTapEnabled = true;
            }
            catch
            {  }
        }

        private async void lvMusic_ItemClick(object sender, ItemClickEventArgs e)
        {
            SongModel.Music itemSelected = (SongModel.Music)e.ClickedItem;

            for(int i = 0; i < listAllSongs.Count; i++)
            {
                if(itemSelected.music_id == listAllSongs[i].music_id)
                {
                    if(listAllSongs[i].music_img == "")
                    {
                        listAllSongs[i].music_img = "ms-appx:///Images/logo.png";
                    }

                    if(listAllSongs[i].music_bitrate == "1000")
                    {
                        listAllSongs[i].file_lossless_url = listAllSongs[i].file_32_url.Replace(".m4a", " [FLAC Lossless].flac").Replace("/32/", "/flac/");
                    }

                    Home.nowPlayingSong = listAllSongs[i];
                    break;
                }
            }
            

            if(MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
            {
                MessageService.SendMessageToBackground(new TrackChangedMessage(Home.nowPlayingSong.music_id));
            }
            else if(MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
            {
                MessageService.SendMessageToBackground(new TrackChangedMessage(Home.nowPlayingSong.music_id));
            }
            else if (isMyBackgroundTaskRunning == false || MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
            {
                if(Home.nowPlayingSong.file_32_url != null)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId32, Home.nowPlayingSong.file_32_url);
                }
                if(Home.nowPlayingSong.file_url != null)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId128, Home.nowPlayingSong.file_url);
                }
                if(Home.nowPlayingSong.file_320_url != null)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId320, Home.nowPlayingSong.file_320_url);
                }
                if(Home.nowPlayingSong.file_m4a_url != null)
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


            CheckListenedSong();

            WriteAndReadFile.SaveListNowPlaying(listAllSongs);
        }

        #region BackgroudTask

        private async void StartBackgroundAudioTask()
        {
            try
            {
                AddMediaPlayerEventHandlers();

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    MessageService.SendMessageToBackground(new UpdatePlaylistMessage(listAllSongs));
                    MessageService.SendMessageToBackground(new StartPlaybackMessage());
                });
            }
            catch { }
        }

        private void AddMediaPlayerEventHandlers()
        {
            //BackgroundMediaPlayer.Current.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground += this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            BackgroundAudioTaskStartedMessage backgroundAudioTaskStartedMessage;
            if (MessageService.TryParseMessage(e.Data, out backgroundAudioTaskStartedMessage))
            {
                Debug.WriteLine("BackgroundAudioTask started");
                backgroundAudioTaskStarted.Set();
                return;
            }
        }
        #endregion

        private async void lvVideo_ItemClick(object sender, ItemClickEventArgs e)
        {
            var mainPage = (MainPage)(Window.Current.Content as Frame).Content;
            SongModel.Music itemSelected = (SongModel.Music)e.ClickedItem;
            mainPage.nowPlaying.Visibility = Visibility.Collapsed;
            try
            {
                string url = "http://chiasenhac.com/api/listen.php?code=duc_wp_2014&return=json&m=" + itemSelected.music_id + "&url=" + itemSelected.music_title_url;

                HttpClient httpClient = new HttpClient();
                string results = await httpClient.GetStringAsync(new Uri(url));

                SongDetail.RootObject ro = JsonConvert.DeserializeObject<SongDetail.RootObject>(results);

                Frame.Navigate(typeof(PlayVideo), ro.music_info);
            }
            catch
            { }
                       
        }

        public static async void CheckListenedSong()
        {
            bool isFileExist = await WriteAndReadFile.CheckFileExist("ListListenedSongs");

            if (isFileExist == true)
            {
                try
                {
                    string listenedSongData = await WriteAndReadFile.ReadFile("ListListenedSongs");
                    MainPage.listListenedSongs = JsonConvert.DeserializeObject<List<SongDetail.MusicInfo>>(listenedSongData);

                    if (Home.nowPlayingSong.music_id != MainPage.listListenedSongs[0].music_id)
                    {
                        MainPage.listListenedSongs.Add(Home.nowPlayingSong);
                        MainPage.listListenedSongs.Reverse();
                        await WriteAndReadFile.WriteFile("ListListenedSongs", JsonConvert.SerializeObject(MainPage.listListenedSongs));
                    }

                    if (MainPage.listListenedSongs.Count > 9)
                    {
                        MainPage.listListenedSongs.RemoveAt(MainPage.listListenedSongs.Count - 1);
                        await WriteAndReadFile.WriteFile("ListListenedSongs", JsonConvert.SerializeObject(MainPage.listListenedSongs));
                    }
                }
                catch { }
            }
            else
            {
                string dataToSave;
                MainPage.listListenedSongs.Add(Home.nowPlayingSong);
                MainPage.listListenedSongs.Reverse();
                dataToSave = JsonConvert.SerializeObject(MainPage.listListenedSongs);

                await WriteAndReadFile.WriteFile("ListListenedSongs", dataToSave);
            }
        }

        private void lvMusic_Holding(object sender, HoldingRoutedEventArgs e)
        {
            SongModel.Music itemSelected = (e.OriginalSource as FrameworkElement).DataContext as SongModel.Music;
            fileName = itemSelected.music_title;

            for (int i = 0; i < listAllSongs.Count; i++)
            {
                if (itemSelected.music_id == listAllSongs[i].music_id)
                {
                    if (listAllSongs[i].music_img == "")
                    {
                        listAllSongs[i].music_img = "ms-appx:///Images/logo.png";
                    }

                    if (listAllSongs[i].music_bitrate == "1000")
                    {
                        listAllSongs[i].file_lossless_url = listAllSongs[i].file_32_url.Replace(".m4a", " [FLAC Lossless].flac").Replace("/32/", "/flac/");
                    }

                    downloadLink32 = listAllSongs[i].file_32_url;
                    downloadLink128 = listAllSongs[i].file_url;
                    downloadLink320 = listAllSongs[i].file_320_url;
                    downloadLinkm4a = listAllSongs[i].file_m4a_url;
                    break;
                }
            }

            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private async void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;

            switch (item.Text)
            {
                case "Download 32kbs":
                    downloadLink = downloadLink32;
                    break;
                case "Download 128kbs":
                    if(downloadLink128 != "")
                    {
                        downloadLink = downloadLink128;
                    }
                    else
                    {
                        MessageDialog showsms = new MessageDialog("Bài hát hiện chưa hỗ trợ chất lượng này!");
                        await showsms.ShowAsync();
                    }
                    break;
                case "Download 320kbs":
                    if(downloadLink320 != "")
                    {
                        downloadLink = downloadLink320;
                    }
                    else
                    {
                        MessageDialog showsms = new MessageDialog("Bài hát hiện chưa hỗ trợ chất lượng này!");
                        await showsms.ShowAsync();
                    }
                    break;
                case "Download m4a":
                    if(downloadLinkm4a != "")
                    {
                        downloadLink = downloadLinkm4a;
                    }
                    else
                    {
                        MessageDialog showsms = new MessageDialog("Bài hát hiện chưa hỗ trợ chất lượng này!");
                        await showsms.ShowAsync();
                    }
                    break;
            }

            IReadOnlyList<DownloadOperation> downloads = null;

            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            }
            catch (Exception)
            {

            }

            Uri source = new Uri(downloadLink, UriKind.RelativeOrAbsolute);

            if (downloadLink.Contains(".m4a"))
            {
                destinationFile = await KnownFolders.MusicLibrary.CreateFileAsync(fileName + ".m4a", CreationCollisionOption.GenerateUniqueName);
            }
            else if (downloadLink.Contains(".mp3"))
            {
                destinationFile = await KnownFolders.MusicLibrary.CreateFileAsync(fileName + ".mp3", CreationCollisionOption.GenerateUniqueName);
            }

            var downloader = new BackgroundDownloader();

            _download = downloader.CreateDownload(source, destinationFile);
            _download.Priority = BackgroundTransferPriority.Default;

            await HandleDownloadAsync();
        }

        private void lvMusic_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SongModel.Music itemSelected = (e.OriginalSource as FrameworkElement).DataContext as SongModel.Music;
            fileName = itemSelected.music_title;
            for (int i = 0; i < listAllSongs.Count; i++)
            {
                if (itemSelected.music_id == listAllSongs[i].music_id)
                {
                    if (listAllSongs[i].music_img == "")
                    {
                        listAllSongs[i].music_img = "ms-appx:///Images/logo.png";
                    }

                    if (listAllSongs[i].music_bitrate == "1000")
                    {
                        listAllSongs[i].file_lossless_url = listAllSongs[i].file_32_url.Replace(".m4a", " [FLAC Lossless].flac").Replace("/32/", "/flac/");
                    }

                    downloadLink32 = listAllSongs[i].file_32_url;
                    downloadLink128 = listAllSongs[i].file_url;
                    downloadLink320 = listAllSongs[i].file_320_url;
                    downloadLinkm4a = listAllSongs[i].file_m4a_url;
                    break;
                }
            }

            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        #region Download

        public async Task HandleDownloadAsync()
        {
            Progress<DownloadOperation> progessDownload = new Progress<DownloadOperation>(UpdateDownloadProgress);
            try
            {
                await _download.StartAsync().AsTask(progessDownload);

            }
            finally
            {
                //ProgressBar.Value = 0;

            }
        }

        private void UpdateDownloadProgress(DownloadOperation download)
        {
            var a = _download.Progress.TotalBytesToReceive;

            var b = _download.Progress.BytesReceived;

        }

        #endregion
    }
}
