using BackgroundAudioShare;
using BackgroundAudioShare.Messages;
using BackgroundAudioShare.Model;
using ChiasenhacUniversal.Model;
using ChiasenhacUniversal.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    public sealed partial class AllMusic : Page
    {
        public TypeMusicModel.RootObject ro;
        private bool isMyBackgroundTaskRunning = false;
        private AutoResetEvent backgroundAudioTaskStarted;
        public List<SongDetail.MusicInfo> listHotSongs = new List<SongDetail.MusicInfo>();
        public string downloadLink32;
        public string downloadLink128;
        public string downloadLink320;
        public string downloadLinkm4a;
        public string downloadLink;
        DownloadOperation _download;
        public StorageFile destinationFile;
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

        public bool isListenedSong = false;

        public AllMusic()
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

            LoadData("beat-playback");

            backgroundAudioTaskStarted = new AutoResetEvent(false);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            prLoading.Visibility = Visibility.Visible;
            try
            {
                if (CurrentApp.LicenseInformation.ProductLicenses["Chiasenhac"].IsActive)
                {
                    //windows.Visibility = Visibility.Collapsed;
                    //windows1.Visibility = Visibility.Collapsed;
                    //windowsphone.Visibility = Visibility.Collapsed;
                    //windowsphone1.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
            }
        }

        public async void LoadData(string typeName)
        {
            try
            {
                var mainPage = (MainPage)(Window.Current.Content as Frame).Content;

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mainPage.nowPlayingInfo.IsTapEnabled = false;
                });

                prLoading.Visibility = Visibility.Visible;
                string url = "http://chiasenhac.com/api/category.php?code=duc_wp_2014&return=json&c=" + typeName;

                HttpClient httpClient = new HttpClient();
                string results = await httpClient.GetStringAsync(new Uri(url));

                ro = JsonConvert.DeserializeObject<TypeMusicModel.RootObject>(results);

                lvMusicNew.ItemsSource = ro.@new.music;
                lvMusicHot.ItemsSource = ro.hot.music;

                GetAllNewSongs(ro.@new.music);
            }
            catch { }
         
        }

        async void GetAllNewSongs(List<TypeMusicModel.Music2> songs)
        {
            try
            {
                var mainPage = (MainPage)(Window.Current.Content as Frame).Content;

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mainPage.nowPlayingInfo.IsTapEnabled = false;
                });

                lvMusicNew.IsEnabled = false;
                BXH.listAllSongs.Clear();

                foreach (var song in songs)
                {
                    string url = "http://chiasenhac.com/api/listen.php?code=duc_wp_2014&return=json&m=" + song.music_id + "&url=" + song.music_title_url;

                    HttpClient httpClient = new HttpClient();
                    string results = await httpClient.GetStringAsync(new Uri(url));

                    SongDetail.RootObject roNew = JsonConvert.DeserializeObject<SongDetail.RootObject>(results);

                    if (roNew.music_info.music_img == "")
                    {
                        roNew.music_info.music_img = "ms-appx:///Images/logo.png";
                    }

                    BXH.listAllSongs.Add(roNew.music_info);
                }

                MessageService.SendMessageToBackground(new ChangedPlaylistMessage(BXH.listAllSongs));
                lvMusicNew.IsEnabled = true;
                prLoading.Visibility = Visibility.Collapsed;

                mainPage.nowPlayingInfo.IsTapEnabled = true;
            }
            catch
            { }
        }

        async void GetAllHotSongs(List<TypeMusicModel.Music> songs)
        {
            try
            {
                lvMusicHot.IsEnabled = false;

                for (int i = 0; i < songs.Count; i++)
                {
                    string url = "http://chiasenhac.com/api/listen.php?code=duc_wp_2014&return=json&m=" + songs[i].music_id + "&url=" + songs[i].music_title_url;

                    HttpClient httpClient = new HttpClient();
                    string results = await httpClient.GetStringAsync(new Uri(url));

                    SongDetail.RootObject roHot = JsonConvert.DeserializeObject<SongDetail.RootObject>(results);

                    listHotSongs.Add(roHot.music_info);
                    //BXH.listAllSongs.Add(roHot.music_info);
                }

                BXH.listAllSongs = listHotSongs;
                MessageService.SendMessageToBackground(new ChangedPlaylistMessage(BXH.listAllSongs));
                lvMusicHot.IsEnabled = true;
                prLoading.Visibility = Visibility.Collapsed;
            }
            catch { }
        }

        private void griTitleMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            BXH.listAllSongs.Clear();

            MenuFlyoutItem item = sender as MenuFlyoutItem;

            switch (item.Text)
            {
                case "Beat, Playback":
                    tblTitle.Text = "Beat, Playback";
                    LoadData("beat-playback");
                    break;
                case "Việt Nam":
                    tblTitle.Text = "Việt Nam";
                    LoadData("vietnam");
                    break;
                case "Thúy Nga":
                    tblTitle.Text = "Thúy Nga";
                    LoadData("thuy-nga");
                    break;
                case "US - UK":
                    tblTitle.Text = "US - UK";
                    LoadData("us-uk");
                    break;
                case "Nhạc Hàn":
                    tblTitle.Text = "Nhạc Hàn";
                    LoadData("korea");
                    break;
                case "Nhạc Hoa":
                    tblTitle.Text = "Nhạc Hoa";
                    LoadData("chinese");
                    break;
                case "Khác":
                    tblTitle.Text = "Khác";
                    LoadData("other");
                    break;
            }

        }

        private void lvMusicNew_ItemClick(object sender, ItemClickEventArgs e)
        {
            TypeMusicModel.Music2 itemSelected = (TypeMusicModel.Music2)e.ClickedItem;

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

            if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
            {
                MessageService.SendMessageToBackground(new TrackChangedMessage(Home.nowPlayingSong.music_id));
            }
            else if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
            {
                MessageService.SendMessageToBackground(new TrackChangedMessage(Home.nowPlayingSong.music_id));
            }
            else if (!IsMyBackgroundTaskRunning || MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
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

            var mainPage = (MainPage)(Window.Current.Content as Frame).Content;

            if (mainPage.nowPlaying.Visibility == Visibility.Collapsed)
            {
                mainPage.nowPlaying.Visibility = Visibility.Visible;
            }
            mainPage.songTitle.Text = Home.nowPlayingSong.music_title;
            mainPage.songArtist.Text = Home.nowPlayingSong.music_artist;
            mainPage.songThumbnail.Source = new BitmapImage(new Uri(Home.nowPlayingSong.music_img, UriKind.RelativeOrAbsolute));
            mainPage.sybPlay.Symbol = Symbol.Pause;

            //for (int i = 0; i < ro.@new.music.Count; i++)
            //{
            //    BXH.listNowPlaying.Add(new SongModel.Music(ro.@new.music[i].id, ro.@new.music[i].music_id, ro.@new.music[i].cat_id, ro.@new.music[i].cat_level, ro.@new.music[i].music_title, ro.@new.music[i].music_artist, ro.@new.music[i].music_title_url, ro.@new.music[i].music_downloads, ro.@new.music[i].music_bitrate, ro.@new.music[i].music_width, ro.@new.music[i].music_height, ro.@new.music[i].music_length, ro.@new.music[i].thumbnail_url));
            //}

            WriteAndReadFile.SaveListNowPlaying(BXH.listAllSongs);
        }

        private void pvAllMusic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ro != null)
            {
                if (pvAllMusic.SelectedIndex == 0)
                {
                    prLoading.Visibility = Visibility.Visible;
                    GetAllNewSongs(ro.@new.music);
                }
                else if (pvAllMusic.SelectedIndex == 1)
                {
                    prLoading.Visibility = Visibility.Visible;
                    BXH.listAllSongs.Clear();
                    GetAllHotSongs(ro.hot.music);
                }
            }
            
        }

        #region BackgroudTask

        private async void StartBackgroundAudioTask()
        {
            AddMediaPlayerEventHandlers();

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MessageService.SendMessageToBackground(new UpdatePlaylistMessage(BXH.listAllSongs));
                MessageService.SendMessageToBackground(new StartPlaybackMessage());
            });

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
                backgroundAudioTaskStarted.Set();
                return;
            }
        }

        #endregion

        private void lvMusicHot_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                TypeMusicModel.Music itemSelected = (TypeMusicModel.Music)e.ClickedItem;

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

                if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
                {
                    MessageService.SendMessageToBackground(new TrackChangedMessage(Home.nowPlayingSong.music_id));
                }
                else if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
                {
                    MessageService.SendMessageToBackground(new TrackChangedMessage(Home.nowPlayingSong.music_id));
                }
                if (!IsMyBackgroundTaskRunning || MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
                {
                    if (Home.nowPlayingSong.file_32_url != "")
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId32, Home.nowPlayingSong.file_32_url);
                    }
                    if (Home.nowPlayingSong.file_url != "")
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId128, Home.nowPlayingSong.file_url);
                    }
                    if (Home.nowPlayingSong.file_320_url != "")
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId320, Home.nowPlayingSong.file_320_url);
                    }
                    if (Home.nowPlayingSong.file_m4a_url != "")
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId500, Home.nowPlayingSong.file_m4a_url);
                    }

                    ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.music_id, Home.nowPlayingSong.music_id);
                    ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.Position, new TimeSpan().ToString());

                    StartBackgroundAudioTask();
                }
                else
                {
                    MessageService.SendMessageToBackground(new UpdatePlaylistMessage(BXH.listAllSongs));
                    MessageService.SendMessageToBackground(new TrackChangedMessage(Home.nowPlayingSong.music_id));
                }

                if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
                {
                    BackgroundMediaPlayer.Current.Play();
                }

                var mainPage = (MainPage)(Window.Current.Content as Frame).Content;

                if (mainPage.nowPlaying.Visibility == Visibility.Collapsed)
                {
                    mainPage.nowPlaying.Visibility = Visibility.Visible;
                }
                mainPage.songTitle.Text = Home.nowPlayingSong.music_title;
                mainPage.songArtist.Text = Home.nowPlayingSong.music_artist;
                mainPage.songThumbnail.Source = new BitmapImage(new Uri(Home.nowPlayingSong.music_img, UriKind.RelativeOrAbsolute));

                for (int i = 0; i < ro.hot.music.Count; i++)
                {
                    BXH.listNowPlaying.Add(new SongModel.Music(ro.hot.music[i].id, ro.hot.music[i].music_id, ro.hot.music[i].cat_id, ro.hot.music[i].cat_level, ro.hot.music[i].music_title, ro.hot.music[i].music_artist, ro.hot.music[i].music_title_url, ro.hot.music[i].music_downloads, ro.hot.music[i].music_bitrate, ro.hot.music[i].music_width, ro.hot.music[i].music_height, ro.hot.music[i].music_length, ro.hot.music[i].thumbnail_url));
                }

                WriteAndReadFile.SaveListNowPlaying(BXH.listAllSongs);
            }
            catch { }
        }

        private void lvMusicNew_Holding(object sender, HoldingRoutedEventArgs e)
        {
            TypeMusicModel.Music2 itemSelected = (e.OriginalSource as FrameworkElement).DataContext as TypeMusicModel.Music2;
            BXH.fileName = itemSelected.music_title;

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

                    downloadLink32 = BXH.listAllSongs[i].file_32_url;
                    downloadLink128 = BXH.listAllSongs[i].file_url;
                    downloadLink320 = BXH.listAllSongs[i].file_320_url;
                    downloadLinkm4a = BXH.listAllSongs[i].file_m4a_url;
                    break;
                }
            }

            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void lvMusicNew_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            TypeMusicModel.Music2 itemSelected = (e.OriginalSource as FrameworkElement).DataContext as TypeMusicModel.Music2;
            BXH.fileName = itemSelected.music_title;

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

                    downloadLink32 = BXH.listAllSongs[i].file_32_url;
                    downloadLink128 = BXH.listAllSongs[i].file_url;
                    downloadLink320 = BXH.listAllSongs[i].file_320_url;
                    downloadLinkm4a = BXH.listAllSongs[i].file_m4a_url;
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
                    if (downloadLink128 != "")
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
                    if (downloadLink320 != "")
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
                    if (downloadLinkm4a != "")
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
                destinationFile = await KnownFolders.MusicLibrary.CreateFileAsync(BXH.fileName + ".m4a", CreationCollisionOption.GenerateUniqueName);
            }
            else if (downloadLink.Contains(".mp3"))
            {
                destinationFile = await KnownFolders.MusicLibrary.CreateFileAsync(BXH.fileName + ".mp3", CreationCollisionOption.GenerateUniqueName);
            }

            var downloader = new BackgroundDownloader();

            _download = downloader.CreateDownload(source, destinationFile);
            _download.Priority = BackgroundTransferPriority.Default;

            await HandleDownloadAsync();
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

        private void lvMusicHot_Holding(object sender, HoldingRoutedEventArgs e)
        {
            TypeMusicModel.Music itemSelected = (e.OriginalSource as FrameworkElement).DataContext as TypeMusicModel.Music;
            BXH.fileName = itemSelected.music_title;

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

                    downloadLink32 = BXH.listAllSongs[i].file_32_url;
                    downloadLink128 = BXH.listAllSongs[i].file_url;
                    downloadLink320 = BXH.listAllSongs[i].file_320_url;
                    downloadLinkm4a = BXH.listAllSongs[i].file_m4a_url;
                    break;
                }
            }

            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void lvMusicHot_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            TypeMusicModel.Music itemSelected = (e.OriginalSource as FrameworkElement).DataContext as TypeMusicModel.Music;
            BXH.fileName = itemSelected.music_title;

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

                    downloadLink32 = BXH.listAllSongs[i].file_32_url;
                    downloadLink128 = BXH.listAllSongs[i].file_url;
                    downloadLink320 = BXH.listAllSongs[i].file_320_url;
                    downloadLinkm4a = BXH.listAllSongs[i].file_m4a_url;
                    break;
                }
            }

            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }

}
