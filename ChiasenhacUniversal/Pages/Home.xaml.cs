using BackgroundAudioShare;
using BackgroundAudioShare.Messages;
using BackgroundAudioShare.Model;
using ChiasenhacUniversal.Model;
using ChiasenhacUniversal.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Foundation.Metadata;
using Windows.Graphics.Imaging;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace ChiasenhacUniversal.Pages
{
    public sealed partial class Home : Page
    {
        public static SongDetail.MusicInfo nowPlayingSong;
        private bool isMyBackgroundTaskRunning = false;
        private AutoResetEvent backgroundAudioTaskStarted;
        public SongDetail.RootObject ro;
        public List<SongDetail.MusicInfo> listListenedSongs = new List<SongDetail.MusicInfo>();

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

        public Home()
        {
            this.InitializeComponent();
            //if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            //{
            //    windows.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    windowsphone.Visibility = Visibility.Collapsed;
            //}

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
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Active.ToString());

            

            try
            {
                string url = "http://chiasenhac.com/api/home.php?code=duc_wp_2014&return=json";

                HttpClient httpClient = new HttpClient();
                HttpClientHandler handler = new HttpClientHandler();
                handler.UseCookies = true;
                //handler.Proxy = new IWebProxy()

                

                string results = await httpClient.GetStringAsync(new Uri(url));

                ListAlbumsModel.RootObject ro = JsonConvert.DeserializeObject<ListAlbumsModel.RootObject>(results);

                cvListAlbums.Source = ro.album.album_new;
            }
            catch
            {
                MessageDialog showsms = new MessageDialog("Lỗi kết nối! Hãy kiểm tra kết nối mạng của bạn!");
                await showsms.ShowAsync();
            }

            try
            {
                if (CurrentApp.LicenseInformation.ProductLicenses["Chiasenhac"].IsActive)
                {
                    //grads.Visibility = Visibility.Collapsed;
                    //windows.Visibility = Visibility.Collapsed;
                    //windowsphone.Visibility = Visibility.Collapsed;
                }
                else
                {
                    //grads.Visibility = Visibility.Visible;
                }
            }
            catch (Exception)
            {
            }
        }

        private void grvAlbums_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var panel = (ItemsWrapGrid)grvAlbums.ItemsPanelRoot;

            if(e.NewSize.Width < 433)
            {
                panel.ItemWidth = e.NewSize.Width / 3;
            }
            else
            {
                panel.ItemWidth = e.NewSize.Width / 5;
            }
        }

        private async void grvAlbums_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                grvAlbums.IsEnabled = false;
                prLoading.Visibility = Visibility.Visible;

                ListAlbumsModel.AlbumNew itemSelected = (ListAlbumsModel.AlbumNew)e.ClickedItem;
                string url = "http://chiasenhac.com/api/listen.php?code=duc_wp_2014&return=json&m=" + itemSelected.music_id + "&url=" + itemSelected.music_title_url;

                HttpClient httpClient = new HttpClient();
                string results = await httpClient.GetStringAsync(new Uri(url));

                ro = JsonConvert.DeserializeObject<SongDetail.RootObject>(results);

                if (ro.music_info.music_img == "")
                {
                    ro.music_info.music_img = "ms-appx:///Images/logo.png";
                }
                if (ro.music_info.music_bitrate == "1000")
                {
                    ro.music_info.file_lossless_url = ro.music_info.file_32_url.Replace(".m4a", " [FLAC Lossless].flac").Replace("/32/", "/flac/");
                }

                Home.nowPlayingSong = ro.music_info;

                BXH.CheckListenedSong();
                BXH.listNowPlaying.Clear();
                GetData();
            }
            catch (Exception)
            {
            }          
        }

        async void GetData()
        {
            try
            {
                string url = "http://chiasenhac.com/api/listen.php?code=duc_wp_2014&return=json&m=" + Home.nowPlayingSong.music_id + "&url=" + Home.nowPlayingSong.music_title_url;

                HttpClient httpClient = new HttpClient();
                string results = await httpClient.GetStringAsync(new Uri(url));

                SongDetail.RootObject ro = JsonConvert.DeserializeObject<SongDetail.RootObject>(results);

                if (ro.track_list != null)
                {
                    for (int i = 0; i < ro.track_list.Count; i++)
                    {
                        BXH.listNowPlaying.Add(new SongModel.Music(i, ro.track_list[i].music_id, ro.track_list[i].cat_id, ro.track_list[i].cat_level, ro.track_list[i].music_title, ro.track_list[i].music_artist, ro.track_list[i].music_title_url, "", "", "", "", "", ""));
                    }
                    GetAllSong(BXH.listNowPlaying);
                }
                else
                {
                    MessageDialog showsms = new MessageDialog("Lỗi dữ liệu album!");
                    await showsms.ShowAsync();
                    grvAlbums.IsEnabled = true;
                    prLoading.Visibility = Visibility.Collapsed;
                }
            }
            catch
            { }
        }

        async void GetAllSong(List<SongModel.Music> songs)
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
                }

                for (int i = 0; i < BXH.listAllSongs.Count; i++)
                {
                    if (BXH.listAllSongs[i].music_bitrate == "1000")
                    {
                        BXH.listAllSongs[i].file_lossless_url = BXH.listAllSongs[i].file_32_url.Replace(".m4a", " [FLAC Lossless].flac").Replace("/32/", "/flac/");
                    }
                }

                WriteAndReadFile.SaveListNowPlaying(BXH.listAllSongs);
                MessageService.SendMessageToBackground(new ChangedPlaylistMessage(BXH.listAllSongs));

                if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
                {
                    MessageService.SendMessageToBackground(new TrackChangedMessage(ro.music_info.music_id));
                }
                else if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
                {
                    MessageService.SendMessageToBackground(new TrackChangedMessage(ro.music_info.music_id));
                }
                else if (isMyBackgroundTaskRunning == false || MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
                {
                    if (ro.music_info.file_32_url != null)
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId32, ro.music_info.file_32_url);
                    }
                    if (ro.music_info.file_url != null)
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId128, ro.music_info.file_url);
                    }
                    if (ro.music_info.file_320_url != null)
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId320, ro.music_info.file_320_url);
                    }
                    if (ro.music_info.file_m4a_url != null)
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId500, ro.music_info.file_m4a_url);
                    }

                    ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.music_id, ro.music_info.music_id);

                    ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.Position, new TimeSpan().ToString());

                    StartBackgroundAudioTask();
                }
                else
                {
                    MessageService.SendMessageToBackground(new TrackChangedMessage(ro.music_info.music_id));
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
                mainPage.songTitle.Text = ro.music_info.music_title;
                mainPage.songArtist.Text = ro.music_info.music_artist;
                mainPage.songThumbnail.Source = new BitmapImage(new Uri(ro.music_info.music_img, UriKind.RelativeOrAbsolute));
                mainPage.sybPlay.Symbol = Symbol.Pause;

                prLoading.Visibility = Visibility.Collapsed;
                grvAlbums.IsEnabled = true;

                GetDominantColor();
            }
            catch (Exception)
            {

            }
        }
      
        private async void pvHome_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(pvHome.SelectedIndex == 1)
            {
                bool isFileExist = await WriteAndReadFile.CheckFileExist("ListListenedSongs");

                if (isFileExist == true)
                {
                    string listenedSongData = await WriteAndReadFile.ReadFile("ListListenedSongs");
                    MainPage.listListenedSongs = JsonConvert.DeserializeObject<List<SongDetail.MusicInfo>>(listenedSongData);

                    cvListListened.Source = MainPage.listListenedSongs;
                }
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

        private async void grvListened_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                SongDetail.MusicInfo itemSelected = (SongDetail.MusicInfo)e.ClickedItem;

                Home.nowPlayingSong = itemSelected;

                BXH.listAllSongs.Clear();
                BXH.listAllSongs.Add(Home.nowPlayingSong);

                MessageService.SendMessageToBackground(new ChangedPlaylistMessage(BXH.listAllSongs));

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

                WriteAndReadFile.SaveListNowPlaying(BXH.listAllSongs);
            }
            catch (Exception)
            {

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

        public async void GetDominantColor()
        {
            try
            {
                var mainPage = (MainPage)(Window.Current.Content as Frame).Content;
                var rass = RandomAccessStreamReference.CreateFromUri(new Uri(Home.nowPlayingSong.music_img, UriKind.RelativeOrAbsolute));
                IRandomAccessStream stream = await rass.OpenReadAsync();
                var decoder = await BitmapDecoder.CreateAsync(stream);

                //Create a transform to get a 1x1 image
                var myTransform = new BitmapTransform { ScaledHeight = 1, ScaledWidth = 1 };

                //Get the pixel provider
                var pixels = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Rgba8,
                    BitmapAlphaMode.Ignore,
                    myTransform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage);

                //Get the bytes of the 1x1 scaled image
                var bytes = pixels.DetachPixelData();

                //read the color 
                var myDominantColor = Color.FromArgb(255, bytes[0], bytes[1], bytes[2]);
                if (myDominantColor.ToString() != "#FFFFFFFF")
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        mainPage.griPlaying.Background = new SolidColorBrush(myDominantColor);
                    });
                }
                else
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        mainPage.griPlaying.Background = new SolidColorBrush(Color.FromArgb(255, 0, 171, 233));
                    });
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private async void SymbolIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            MessageDialog md = new MessageDialog("Loại bỏ quảng cáo vĩnh viễn khỏi ứng dụng của bạn và ủng hộ chúng tôi", "Chia Sẻ Nhạc");
            bool? result = null;
            md.Commands.Add(
               new UICommand("Chơi luôn", new UICommandInvokedHandler((cmd) => result = true)));
            md.Commands.Add(
               new UICommand("Không chơi", new UICommandInvokedHandler((cmd) => result = false)));

            await md.ShowAsync();
            if (result == true)
            {
                try
                {
                    if (!CurrentApp.LicenseInformation.ProductLicenses["Chiasenhac"].IsActive)
                    {
                        //grads.Visibility = Visibility.Collapsed;
                        await CurrentApp.RequestProductPurchaseAsync("Chiasenhac");
                        if (CurrentApp.LicenseInformation.ProductLicenses["Chiasenhac"].IsActive)
                        {
                            return;
                        }
                    }
                }
                catch
                {
                }
            }
            //grads.Visibility = Visibility.Collapsed;
            await Task.Delay(600000);
            //grads.Visibility = Visibility.Visible;
        }

        //private void AdControl_ErrorOccurred(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        //{
        //    grads.Visibility = Visibility.Collapsed;
        //}
    }
}
