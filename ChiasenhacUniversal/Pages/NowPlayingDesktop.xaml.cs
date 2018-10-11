﻿using BackgroundAudioShare;
using BackgroundAudioShare.Messages;
using BackgroundAudioShare.Model;
using ChiasenhacUniversal.Model.ArtistImage;
using ChiasenhacUniversal.ViewModel;
using MixRadio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Xml.Dom;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Playback;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;


namespace ChiasenhacUniversal.Pages
{
    public sealed partial class NowPlayingDesktop : Page
    {
        DispatcherTimer timer;
        private bool isMyBackgroundTaskRunning = false;
        private const string TrackIdKey = "trackid";
        private AutoResetEvent backgroundAudioTaskStarted;
        public string downloadLink;
        public StorageFile destinationFile;
        DownloadOperation _download;
        public List<SongDetail.MusicInfo> listFavoriteSongs = new List<SongDetail.MusicInfo>();
        public bool isFavoriteSong = false;
        public int removeIndex;
        public bool isListenedSong = false;
        public bool isRepeat = false;
        public bool isShuffle = false;
        string artist;
        int indexImage;
        StorageFile file;
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

        public NowPlayingDesktop()
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

            BackgroundMediaPlayer.MessageReceivedFromBackground += this.BackgroundMediaPlayer_MessageReceivedFromBackground;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            backgroundAudioTaskStarted = new AutoResetEvent(false);
        }

        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            TrackChangedMessage trackChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out trackChangedMessage))
            {
                try
                {
                    int index = BXH.listAllSongs.FindIndex(item => GetMusicId(item) == (string)trackChangedMessage.MusicId);


                    Home.nowPlayingSong = BXH.listAllSongs[index];

                    BXH.CheckListenedSong();
                    CheckFavoriteSong();
                    
                    string song_lenght;
                    int length = int.Parse(Home.nowPlayingSong.music_length);
                    int minutes = length / 60;
                    int seconds = length % 60;

                    if (seconds < 10)
                    {
                        song_lenght = minutes + ":" + "0" + seconds;
                    }
                    else
                    {
                        song_lenght = minutes + ":" + seconds;
                    }

                    BXH.CheckListenedSong();
                    NowPlayingMobile.UpdateLiveTile();

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ruSongTitle.Text = BXH.listAllSongs[index].music_title;
                        ruSongArtist.Text = BXH.listAllSongs[index].music_artist;
                        ruSongAlbum.Text = Home.nowPlayingSong.music_album;
                        ruNowPlayingLenght.Text = song_lenght;
                        if (BXH.listAllSongs[index].music_lyric != null)
                        {
                            tblLyric.Text = BXH.listAllSongs[index].music_lyric;
                        }
                        imgThumbnailSong.Source = new BitmapImage(new Uri(BXH.listAllSongs[index].music_img, UriKind.RelativeOrAbsolute));
                        if (Home.nowPlayingSong.music_artist.Contains(';'))
                        {
                            Home.nowPlayingSong.music_artist = Home.nowPlayingSong.music_artist.Substring(0, Home.nowPlayingSong.music_artist.IndexOf(';'));
                        }
                        else if (Home.nowPlayingSong.music_artist.Contains(','))
                        {
                            Home.nowPlayingSong.music_artist = Home.nowPlayingSong.music_artist.Substring(0, Home.nowPlayingSong.music_artist.IndexOf(','));
                        }

                        SearchArtistImageXbox(Home.nowPlayingSong.music_artist);
                        
                    });                    
                }
                catch { }

                
            }

            BackgroundAudioTaskStartedMessage backgroundAudioTaskStartedMessage;
            if (MessageService.TryParseMessage(e.Data, out backgroundAudioTaskStartedMessage))
            {
                backgroundAudioTaskStarted.Set();
                return;
            }
        }

        async void timer_Tick(object sender, object e)
        {
            sliderTime.Value = BackgroundMediaPlayer.Current.Position.TotalSeconds;

            if (BackgroundMediaPlayer.Current.Position.TotalMinutes < 10)
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ruNowPlayingTime.Text = string.Format("{0:d1}:{1:d2}", BackgroundMediaPlayer.Current.Position.Minutes, BackgroundMediaPlayer.Current.Position.Seconds);
                });
            }
            else
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ruNowPlayingTime.Text = string.Format("{0:d2}:{1:d2}", BackgroundMediaPlayer.Current.Position.Minutes, BackgroundMediaPlayer.Current.Position.Seconds);
                });

            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(Home.nowPlayingSong != null)
            {
                var mainPage = (MainPage)(Window.Current.Content as Frame).Content;
                mainPage.nowPlaying.Visibility = Visibility.Collapsed;

                bool isFileExist = await WriteAndReadFile.CheckFileExist("ListNowPlaying");

                if (isFileExist == true)
                {
                    string listenedSongData = await WriteAndReadFile.ReadFile("ListNowPlaying");
                    BXH.listAllSongs = JsonConvert.DeserializeObject<List<SongDetail.MusicInfo>>(listenedSongData);
                }

                if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
                {
                    sbiPlay.Symbol = Symbol.Pause;
                }
                else if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Paused)
                {
                    sbiPlay.Symbol = Symbol.Play;
                }
                else if (MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.music_id, Home.nowPlayingSong.music_id);
                    StartBackgroundAudioTask();

                    sbiPlay.Symbol = Symbol.Pause;
                }

                if (Home.nowPlayingSong.music_lyric != null)
                {
                    tblLyric.Text = Home.nowPlayingSong.music_lyric;
                }

                CheckFavoriteSong();

                if (Home.nowPlayingSong.music_length.Contains(':'))
                {
                    string song_lenght = Home.nowPlayingSong.music_length;
                    string[] st = song_lenght.Split(':');
                    int minutes = int.Parse(st[0]);
                    int second = int.Parse(st[1]);
                    sliderTime.Maximum = (minutes * 60) + second;
                    ruNowPlayingLenght.Text = song_lenght;
                }
                else
                {
                    string song_lenght;
                    int length = int.Parse(Home.nowPlayingSong.music_length);
                    int minutes = length / 60;
                    int seconds = length % 60;

                    if (seconds < 10)
                    {
                        song_lenght = minutes + ":" + "0" + seconds;
                    }
                    else
                    {
                        song_lenght = minutes + ":" + seconds;
                    }

                    sliderTime.Maximum = (minutes * 60) + seconds;
                    ruNowPlayingLenght.Text = song_lenght;
                }

                if (Home.nowPlayingSong != null)
                {
                    if (Home.nowPlayingSong.music_img == "")
                    {
                        BitmapImage bi = new BitmapImage(new Uri("ms-appx:///Images/logo.png", UriKind.RelativeOrAbsolute));
                        imgThumbnailSong.Source = bi;
                    }
                    else
                    {
                        BitmapImage bi = new BitmapImage(new Uri(Home.nowPlayingSong.music_img, UriKind.RelativeOrAbsolute));
                        imgThumbnailSong.Source = bi;
                    }

                    ruSongTitle.Text = Home.nowPlayingSong.music_title;
                    ruSongArtist.Text = Home.nowPlayingSong.music_artist;
                    ruSongAlbum.Text = Home.nowPlayingSong.music_album;

                    if (Home.nowPlayingSong.music_img == "")
                    {
                        BitmapImage bi = new BitmapImage(new Uri("ms-appx:///Images/logo.png", UriKind.RelativeOrAbsolute));
                        ibBackground.ImageSource = bi;
                    }
                    else
                    {
                        BitmapImage bi = new BitmapImage(new Uri(Home.nowPlayingSong.music_img, UriKind.RelativeOrAbsolute));
                        ibBackground.ImageSource = bi;
                    }

                    if (Home.nowPlayingSong.music_artist.Contains(';'))
                    {
                        Home.nowPlayingSong.music_artist = Home.nowPlayingSong.music_artist.Substring(0, Home.nowPlayingSong.music_artist.IndexOf(';'));
                    }
                    else if (Home.nowPlayingSong.music_artist.Contains(','))
                    {
                        Home.nowPlayingSong.music_artist = Home.nowPlayingSong.music_artist.Substring(0, Home.nowPlayingSong.music_artist.IndexOf(','));
                    }

                    SearchArtistImageXbox(Home.nowPlayingSong.music_artist);
                }

                for (int i = 0; i < BXH.listAllSongs.Count; i++)
                {
                    BXH.listNowPlaying.Add(new SongModel.Music(i, BXH.listAllSongs[i].music_id, BXH.listAllSongs[i].cat_id, BXH.listAllSongs[i].cat_level, BXH.listAllSongs[i].music_title, BXH.listAllSongs[i].music_artist, BXH.listAllSongs[i].music_title_url, BXH.listAllSongs[i].music_downloads, BXH.listAllSongs[i].music_bitrate, BXH.listAllSongs[i].music_width, BXH.listAllSongs[i].music_height, BXH.listAllSongs[i].music_length, BXH.listAllSongs[i].music_img));
                }

                if (BXH.listNowPlaying != null)
                {
                    lvSongs.ItemsSource = BXH.listAllSongs;
                }

                var b = Convert.ToInt32(App.bitrate.Values["bitrate"]);
                if (b == 0)
                {
                    tblQuality.Text = "128kbs";
                }
                else if (b == 32)
                {
                    tblQuality.Text = "32kbs";
                }
                else if (b == 128)
                {
                    tblQuality.Text = "128kbs";
                }
                else if (b == 320)
                {
                    tblQuality.Text = "320kbs";
                }
                else if (b == 500)
                {
                    tblQuality.Text = "m4a";
                }
                else if (b == 1000)
                {
                    tblQuality.Text = "flac";
                }
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            var mainPage = (MainPage)(Window.Current.Content as Frame).Content;
            mainPage.nowPlaying.Visibility = Visibility.Visible;
        }

        #region Search artist image

        public async void SearchArtistImageXbox(string artist)
        {
            try
            {
                var client = new HttpClient();

                // Define the data needed to request an authorization token.
                var service = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
                var clientId = "MusicWP";
                var clientSecret = "bUCLxUmaoyLya8JQNNvzfEBANaQurp9sz4fJH0yd2ew=";
                var scope = "http://music.xboxlive.com";
                var grantType = "client_credentials";

                // Create the request data.
                var requestData = new Dictionary<string, string>();
                requestData["client_id"] = clientId;
                requestData["client_secret"] = clientSecret;
                requestData["scope"] = scope;
                requestData["grant_type"] = grantType;

                // Post the request and retrieve the response.
                var response = await client.PostAsync(new Uri(service), new HttpFormUrlEncodedContent(requestData));
                var responseString = await response.Content.ReadAsStringAsync();
                var token = Regex.Match(responseString, ".*\"access_token\":\"(.*?)\".*", RegexOptions.IgnoreCase).Groups[1].Value;

                string url = "https://music.xboxlive.com/1/content/music/search?q=" + artist + "&accessToken=Bearer+" + WebUtility.UrlEncode(token);

                System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
                string responseData = await httpClient.GetStringAsync(new Uri(url));

                XboxDataModel.RootObject xdm = JsonConvert.DeserializeObject<XboxDataModel.RootObject>(responseData);
                
                if (xdm.Artists != null)
                {
                    for(int i = 0; i < xdm.Artists.Items.Count; i++)
                    {
                        try
                        {
                            HttpClient httpClientImage = new HttpClient();
                            string results = await httpClientImage.GetStringAsync(new Uri(xdm.Artists.Items[i].ImageUrl, UriKind.RelativeOrAbsolute));

                            if(results != null)
                            {
                                indexImage = i;
                                break;
                            }
                        }
                        catch { }
                    }

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ibBackground.ImageSource = new BitmapImage(new Uri(xdm.Artists.Items[indexImage].ImageUrl, UriKind.RelativeOrAbsolute));
                    });
                }
                else
                {
                    SearchArtistImageMixRadio(Home.nowPlayingSong.music_artist);
                }
            }
            catch
            {
                SearchArtistImageMixRadio(Home.nowPlayingSong.music_artist);
            }
        }

        async void SearchArtistImageMixRadio(string artist)
        {
            MusicClient client = new MusicClient("80a15866afcfe2e6497103420ef9385a");
            var genres = await client.GetGenresAsync();

            var listArtist = await client.SearchArtistsAsync(artist);

            if (listArtist.Count != 0)
            {
                try
                {
                    if (listArtist.Result[0].Thumb640Uri != null)
                    {
                        if (Home.nowPlayingSong.music_artist == "Pink")
                        {
                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                BitmapImage bmP = new BitmapImage(listArtist.Result[1].Thumb640Uri);
                                ibBackground.ImageSource = bmP;
                            });

                        }
                        else
                        {
                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                BitmapImage bm = new BitmapImage(listArtist.Result[0].Thumb640Uri);
                                ibBackground.ImageSource = bm;
                            });

                        }
                    }
                    else
                    {
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            BitmapImage bm = new BitmapImage(listArtist.Result[0].Thumb320Uri);
                            ibBackground.ImageSource = bm;
                        });
                    }
                }
                catch
                {
                    ibBackground.ImageSource = new BitmapImage(new Uri(Home.nowPlayingSong.music_img, UriKind.RelativeOrAbsolute));
                }
            }
        }

        #endregion

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            var mainPage = (MainPage)(Window.Current.Content as Frame).Content;

            switch (item.Text)
            {
                case "32kbs":
                    tblQuality.Text = "32kbs";
                    MessageService.SendMessageToBackground(new QualityChangeMessage("32kbs"));
                    break;
                case "128kbs":
                    tblQuality.Text = "128kbs";
                    MessageService.SendMessageToBackground(new QualityChangeMessage("128kbs"));
                    break;
                case "320kbs":
                    if(Home.nowPlayingSong.file_320_url == "")
                    {
                        griNoti.Visibility = Visibility.Visible;
                        await Task.Delay(2000);
                        griNoti.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MessageService.SendMessageToBackground(new StartPlaybackMessage());
                        tblQuality.Text = "320kbs";
                        MessageService.SendMessageToBackground(new QualityChangeMessage("320kbs"));
                    }
                    
                    break;
                case "m4a":
                    if (Home.nowPlayingSong.file_m4a_url == "")
                    {
                        griNoti.Visibility = Visibility.Visible;
                        await Task.Delay(2000);
                        griNoti.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MessageService.SendMessageToBackground(new StartPlaybackMessage());
                        tblQuality.Text = "m4a";
                        MessageService.SendMessageToBackground(new QualityChangeMessage("m4a"));
                    }
                    
                    break;
                case "flac":
                    if (Home.nowPlayingSong.file_lossless_url == "")
                    {
                        griNoti.Visibility = Visibility.Visible;
                        await Task.Delay(2000);
                        griNoti.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MessageService.SendMessageToBackground(new StartPlaybackMessage());
                        tblQuality.Text = "flac";
                        MessageService.SendMessageToBackground(new QualityChangeMessage("flac"));
                    }
                                        
                    break;
            }
        }

        private void griBitrate_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void vbPlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
            {
                BackgroundMediaPlayer.Current.Pause();
                sbiPlay.Symbol = Symbol.Play;
            }
            else if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
            {
                BackgroundMediaPlayer.Current.Play();
                sbiPlay.Symbol = Symbol.Pause;
            }
        }

        private void vbNext_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MessageService.SendMessageToBackground(new SkipNextMessage());            
        }

        private void vbPrevious_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MessageService.SendMessageToBackground(new SkipPreviousMessage());
        }

        private void vbLyric_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (griListSong.Visibility == Visibility.Visible)
            {
                griListSong.Visibility = Visibility.Collapsed;
            }

            if (svLyric.Visibility == Visibility.Collapsed)
            {
                svLyric.Visibility = Visibility.Visible;
            }
            else
            {
                svLyric.Visibility = Visibility.Collapsed;
            }
        }

        private void vbListSongs_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (svLyric.Visibility == Visibility.Visible)
            {
                svLyric.Visibility = Visibility.Collapsed;
            }

            if (griListSong.Visibility == Visibility.Collapsed)
            {
                griListSong.Visibility = Visibility.Visible;
            }
            else
            {
                griListSong.Visibility = Visibility.Collapsed;
            }
        }

        private  void lvSongs_ItemClick(object sender, ItemClickEventArgs e)
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

            if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
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

            mainPage.songTitle.Text = Home.nowPlayingSong.music_title;
            mainPage.songArtist.Text = Home.nowPlayingSong.music_artist;
            mainPage.songThumbnail.Source = new BitmapImage(new Uri(Home.nowPlayingSong.music_img, UriKind.RelativeOrAbsolute));
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
            BackgroundMediaPlayer.Current.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground += this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            var currentState = sender.CurrentState; // cache outside of completion or you might get a different value
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateTransportControls(currentState);
            });
        }

        private void UpdateTransportControls(MediaPlayerState currentState)
        {
            if (currentState == MediaPlayerState.Playing)
            {
                sbiPlay.Symbol = Symbol.Pause;
            }
            else
            {
                sbiPlay.Symbol = Symbol.Play;
            }
        }

        #endregion

        private async void vbShare_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(griMain);
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

            file = await ApplicationData.Current.LocalFolder.CreateFileAsync("JetsoftPaint-Picture.png", CreationCollisionOption.OpenIfExists);

            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);

                encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Ignore,
                (uint)renderTargetBitmap.PixelWidth,
                (uint)renderTargetBitmap.PixelHeight,
                DisplayInformation.GetForCurrentView().LogicalDpi,
                DisplayInformation.GetForCurrentView().LogicalDpi,
                pixelBuffer.ToArray());
                await encoder.FlushAsync();
            }

            DataTransferManager dtManager = DataTransferManager.GetForCurrentView();
            dtManager.DataRequested += dtManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void dtManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataPackage requestData = e.Request.Data;
            e.Request.Data.Properties.Title = "Chia sẻ từ ứng dụng Chiasenhac Universal";
            e.Request.Data.Properties.ApplicationName = "Chiasenhac_Universal";
            //e.Request.Data.Properties.Description = "Từ Chia sẻ nhạc Windows 10 Universal App.";
            e.Request.Data.Properties.ApplicationName = "Chiasenhac Universal";
            List<IStorageItem> imageItems = new List<IStorageItem>();
            imageItems.Add(this.file);
            requestData.SetStorageItems(imageItems);
            RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromFile(file);
            requestData.Properties.Thumbnail = imageStreamRef;
            requestData.SetBitmap(imageStreamRef);
            e.Request.Data.SetWebLink(new Uri(Home.nowPlayingSong.full_url, UriKind.RelativeOrAbsolute));
        }

        public void vbDownload_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private async void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {        
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            BXH.fileName = Home.nowPlayingSong.music_title;
            switch (item.Text)
            {
                case "32kbs":
                    if(Home.nowPlayingSong.file_32_url == "")
                    {
                        griNoti.Visibility = Visibility.Visible;
                        await Task.Delay(2000);
                        griNoti.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        downloadLink = Home.nowPlayingSong.file_32_url;
                    }                   
                    break;
                case "128kbs":
                    if(Home.nowPlayingSong.file_url == "")
                    {
                        griNoti.Visibility = Visibility.Visible;
                        await Task.Delay(2000);
                        griNoti.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        downloadLink = Home.nowPlayingSong.file_url;
                    }
                    break;
                case "320kbs":
                    if(Home.nowPlayingSong.file_320_url == "")
                    {
                        griNoti.Visibility = Visibility.Visible;
                        await Task.Delay(2000);
                        griNoti.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        downloadLink = Home.nowPlayingSong.file_320_url;
                    }                   
                    break;
                case "m4a":
                    if(Home.nowPlayingSong.file_m4a_url == "")
                    {
                        griNoti.Visibility = Visibility.Visible;
                        await Task.Delay(2000);
                        griNoti.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        downloadLink = Home.nowPlayingSong.file_m4a_url;
                    }
                    break;
                case "flac":
                    if (Home.nowPlayingSong.file_lossless_url == "")
                    {
                        griNoti.Visibility = Visibility.Visible;
                        await Task.Delay(2000);
                        griNoti.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        downloadLink = Home.nowPlayingSong.file_lossless_url;
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

            if(downloadLink.Contains(".m4a"))
            {
                destinationFile = await KnownFolders.MusicLibrary.CreateFileAsync(Home.nowPlayingSong.music_title + ".m4a", CreationCollisionOption.GenerateUniqueName);
            }
            else if(downloadLink.Contains(".mp3"))
            {
                destinationFile = await KnownFolders.MusicLibrary.CreateFileAsync(Home.nowPlayingSong.music_title + ".mp3", CreationCollisionOption.GenerateUniqueName);
            }
            //else if (downloadLink.Contains(".flac"))
            //{
            //    try
            //    {
            //        destinationFile = await Windows.Storage.DownloadsFolder.CreateFileAsync(Home.nowPlayingSong.music_title + ".flac", CreationCollisionOption.GenerateUniqueName);
            //    }
            //    catch(Exception ex)
            //    {
            //        ex.ToString();
            //    }
            //}

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

        private void sliderTime_Tapped(object sender, TappedRoutedEventArgs e)
        {
            timer.Start();
            int value = (int)sliderTime.Value;
            TimeSpan ts = new TimeSpan(0, 0, value);
            BackgroundMediaPlayer.Current.Position = ts;
        }

        private async void vbFavorite_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                bool isFileExist = await WriteAndReadFile.CheckFileExist("ListFavoriteSongs");

                if(isFileExist == true)
                {
                    string loveSongData = await WriteAndReadFile.ReadFile("ListFavoriteSongs");
                    listFavoriteSongs = JsonConvert.DeserializeObject<List<SongDetail.MusicInfo>>(loveSongData);

                    for (int i = 0; i < listFavoriteSongs.Count; i++)
                    {
                        if (listFavoriteSongs[i].music_id == Home.nowPlayingSong.music_id)
                        {
                            isFavoriteSong = true;
                            removeIndex = i;
                            break;
                        }
                        else
                        {
                            isFavoriteSong = false;
                            break;
                        }
                    }

                    if (isFavoriteSong == true)
                    {
                        listFavoriteSongs.RemoveAt(removeIndex);
                        await WriteAndReadFile.WriteFile("ListFavoriteSongs", JsonConvert.SerializeObject(listFavoriteSongs));
                        sbiFavorite.Symbol = Symbol.OutlineStar;
                    }
                    else
                    {
                        listFavoriteSongs.Add(Home.nowPlayingSong);
                        listFavoriteSongs.Reverse();

                        await WriteAndReadFile.WriteFile("ListFavoriteSongs", JsonConvert.SerializeObject(listFavoriteSongs));

                        sbiFavorite.Symbol = Symbol.Favorite;
                    }
                }
                else
                {
                    string dataToSave;

                    listFavoriteSongs.Add(Home.nowPlayingSong);
                    dataToSave = JsonConvert.SerializeObject(listFavoriteSongs);

                    await WriteAndReadFile.WriteFile("ListFavoriteSongs", dataToSave);

                    sbiFavorite.Symbol = Symbol.Favorite;
                }
                
            }
            catch { }
        }

        public async void CheckFavoriteSong()
        {
            bool isFileExist = await WriteAndReadFile.CheckFileExist("ListFavoriteSongs");
            if (isFileExist == true)
            {
                string loveSongData = await WriteAndReadFile.ReadFile("ListFavoriteSongs");
                listFavoriteSongs = JsonConvert.DeserializeObject<List<SongDetail.MusicInfo>>(loveSongData);

                for (int i = 0; i < listFavoriteSongs.Count; i++)
                {
                    if (listFavoriteSongs[i].music_id == Home.nowPlayingSong.music_id)
                    {
                        isFavoriteSong = true;
                        break;
                    }
                    else
                    {
                        isFavoriteSong = false;
                    }
                }

                if(isFavoriteSong == true)
                {
                    
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        sbiFavorite.Symbol = Symbol.Favorite;
                    });
                }
                else
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        sbiFavorite.Symbol = Symbol.OutlineStar;
                    });
                }
            }
        }

        private void vbRepeat_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(isRepeat == false)
            {
                MessageService.SendMessageToBackground(new RepeatMessage(true));
                isRepeat = true;
                sbiRepeat.Symbol = Symbol.RepeatOne;
            }
            else
            {
                MessageService.SendMessageToBackground(new RepeatMessage(false));
                isRepeat = false;
                sbiRepeat.Symbol = Symbol.RepeatAll;
            }

        }

        private void vbShuffle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (isShuffle == false)
            {
                MessageService.SendMessageToBackground(new ShuffleMessage(true));
                isShuffle = true;
                sbiShuffle.Symbol = Symbol.Switch;
            }
            else
            {
                MessageService.SendMessageToBackground(new ShuffleMessage(false));
                isShuffle = false;
                sbiShuffle.Symbol = Symbol.Shuffle;
            }
        }

        private void ibBackground_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            BitmapImage bi = new BitmapImage(new Uri("ms-appx:///Images/logo.png", UriKind.RelativeOrAbsolute));
            ibBackground.ImageSource = bi;
        }

        private void tblSongTitle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            
        }

        private void hylArtist_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            Search.cat = "artist";
            
            if (ruSongArtist.Text.Contains(';'))
            {
                artist = Home.nowPlayingSong.music_artist.Substring(0, Home.nowPlayingSong.music_artist.IndexOf(';'));
            }
            else if (Home.nowPlayingSong.music_artist.Contains(','))
            {
                artist = Home.nowPlayingSong.music_artist.Substring(0, Home.nowPlayingSong.music_artist.IndexOf(','));
            }
            else
            {
                artist = ruSongArtist.Text;
            }

            Frame.Navigate(typeof(Search), artist);
        }

        private void hylAlbum_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            Search.cat = "album";
            Frame.Navigate(typeof(Search), ruSongAlbum.Text);
        }

        private void hylTitle_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            Search.cat = "";
            Frame.Navigate(typeof(Search), ruSongTitle.Text);
        }
    }
}