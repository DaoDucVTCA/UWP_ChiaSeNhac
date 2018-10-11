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
using Windows.Media.Playback;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ChiasenhacUniversal.Pages
{
    public sealed partial class Search : Page
    {
        public List<SearchResultModel.MusicList> listSearchResult = new List<SearchResultModel.MusicList>();
        public List<SearchAlbumResultModel.AlbumList> listAlbumResult = new List<SearchAlbumResultModel.AlbumList>();
        private bool isMyBackgroundTaskRunning = false;
        public AutoResetEvent backgroundAudioTaskStarted;
        public static string cat;
        public int page = 1;
        public string searchKey;
        public string downloadLink32;
        public string downloadLink128;
        public string downloadLink320;
        public string downloadLinkm4a;
        public string downloadLink;
        DownloadOperation _download;
        public StorageFile destinationFile;
        SearchResultModel.RootObject searchResultList;
        SearchAlbumResultModel.RootObject searchAlbumResult = new SearchAlbumResultModel.RootObject();
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
        public SongDetail.RootObject ro;

        public Search()
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
            try
            {
                prLoading.Visibility = Visibility.Visible;

                lvResult.ItemsSource = null;
                grvAlbums.ItemsSource = null;

                searchKey = e.Parameter as string;

                if (searchKey != null)
                {
                    
                    if(cat == null)
                    {
                        cat = "";
                    }

                    switch(cat)
                    {
                        case "album":
                            tblSearchType.Text = "Album";
                            lvResult.Visibility = Visibility.Collapsed;
                            grvAlbums.Visibility = Visibility.Visible;

                            string urlAlbum = "http://search.chiasenhac.com/api/search.php?code=duc_wp_2014&return=json&s=" + searchKey + "&mode=album" + "&page=" + page;
                            HttpClient httpClientAlbum = new HttpClient();
                            string responseStringAlbum = await httpClientAlbum.GetStringAsync(new Uri(urlAlbum));
                            searchAlbumResult = JsonConvert.DeserializeObject<SearchAlbumResultModel.RootObject>(responseStringAlbum);

                            for (int i = 0; i < searchAlbumResult.album_list.Count; i++)
                            {
                                if (searchAlbumResult.album_list[i].cover_img == "")
                                {
                                    searchAlbumResult.album_list[i].cover_img = "ms-appx:///Images/logo.png";
                                }
                            }

                            listAlbumResult = searchAlbumResult.album_list;

                            grvAlbums.ItemsSource = listAlbumResult;
                            break;

                        case "artist":
                            SearchArtist(1);
                            break;

                        case "video":
                            SearchVideo(1);
                            break;

                        case "":
                            SearchAll(1);
                            break;

                        case "composer":
                            SearchComposer(1);
                            break;
                    }
                    
                }
                prLoading.Visibility = Visibility.Collapsed;
            }
            catch
            { }
        }

        private async void lvResult_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                SearchResultModel.MusicList itemSelected = (SearchResultModel.MusicList)e.ClickedItem;

                if (itemSelected.cat_id != "1")
                {
                    string url = "http://chiasenhac.com/api/listen.php?code=duc_wp_2014&return=json&m=" + itemSelected.music_id + "&url=" + itemSelected.music_title_url;

                    HttpClient httpClient = new HttpClient();
                    string results = await httpClient.GetStringAsync(new Uri(url));

                    SongDetail.RootObject ro = JsonConvert.DeserializeObject<SongDetail.RootObject>(results);

                    if (ro.music_info.music_img == "")
                    {
                        ro.music_info.music_img = "ms-appx:///Images/logo.png";
                    }

                    Home.nowPlayingSong = ro.music_info;
                    BXH.listAllSongs.Clear();
                    BXH.listAllSongs.Add(ro.music_info);

                    MessageService.SendMessageToBackground(new ChangedPlaylistMessage(BXH.listAllSongs));

                    BXH.CheckListenedSong();
                    NowPlayingMobile.UpdateLiveTile();

                    if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
                    {
                        MessageService.SendMessageToBackground(new TrackChangedMessage(ro.music_info.music_id));
                    }
                    else if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
                    {
                        MessageService.SendMessageToBackground(new TrackChangedMessage(ro.music_info.music_id));
                    }
                    else if (!IsMyBackgroundTaskRunning || MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
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

                        ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.Position, new TimeSpan().ToString());

                        StartBackgroundAudioTask();
                    }
                    else
                    {
                        //MessageService.SendMessageToBackground(new UpdatePlaylistMessage(BXH.listAllSongs));
                        MessageService.SendMessageToBackground(new TrackChangedMessage(ro.music_info.music_id));
                    }

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var mainPage = (MainPage)(Window.Current.Content as Frame).Content;

                        if (mainPage.nowPlaying.Visibility == Visibility.Collapsed)
                        {
                            mainPage.nowPlaying.Visibility = Visibility.Visible;
                        }
                        mainPage.songTitle.Text = ro.music_info.music_title;
                        mainPage.songArtist.Text = ro.music_info.music_artist;
                        mainPage.songThumbnail.Source = new BitmapImage(new Uri(ro.music_info.music_img, UriKind.RelativeOrAbsolute));
                        mainPage.sybPlay.Symbol = Symbol.Pause;
                    });

                    WriteAndReadFile.SaveListNowPlaying(BXH.listAllSongs);
                }
                else
                {
                    if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
                    {
                        BackgroundMediaPlayer.Current.Pause();
                    }

                    string url = "http://chiasenhac.com/api/listen.php?code=duc_wp_2014&return=json&m=" + itemSelected.music_id + "&url=" + itemSelected.music_title_url;

                    HttpClient httpClient = new HttpClient();
                    string results = await httpClient.GetStringAsync(new Uri(url));

                    SongDetail.RootObject ro = JsonConvert.DeserializeObject<SongDetail.RootObject>(results);

                    if (ro.music_info.music_width == "1920" && ro.music_info.file_lossless_url == "")
                    {
                        ro.music_info.file_lossless_url = ro.music_info.file_m4a_url.Replace("/m4a/", "/flac/").Replace(".mp4", " [MP4 HD 1080p].mp4");
                    }

                    if (ro.music_info != null)
                    {
                        Frame.Navigate(typeof(PlayVideo), ro.music_info);
                    }
                }
            }
            catch
            { }
            
        }

        public async void SearchAll(int page)
        {
            tblSearchType.Text = "Tất cả";
            lvResult.Visibility = Visibility.Visible;
            grvAlbums.Visibility = Visibility.Collapsed;

            string urlAll = "http://search.chiasenhac.com/api/search.php?code=duc_wp_2014&return=json&s=" + searchKey + "&page=" + page;
            HttpClient httpClientAll = new HttpClient();
            string responseStringAll = await httpClientAll.GetStringAsync(new Uri(urlAll));
            searchResultList = JsonConvert.DeserializeObject<SearchResultModel.RootObject>(responseStringAll);

            for (int i = 0; i < searchResultList.music_list.Count; i++)
            {
                if (searchResultList.music_list[i].thumbnail == "")
                {
                    searchResultList.music_list[i].thumbnail = "ms-appx:///Images/Mp3.png";
                }
            }

            listSearchResult = searchResultList.music_list;

            lvResult.ItemsSource = listSearchResult;
        }

        public async void SearchAlbum(int page)
        {
            tblSearchType.Text = "Album";
            lvResult.Visibility = Visibility.Collapsed;
            grvAlbums.Visibility = Visibility.Visible;

            string urlAlbum = "http://search.chiasenhac.com/api/search.php?code=duc_wp_2014&return=json&s=" + searchKey + "&mode=album" + "&page=" + page;
            HttpClient httpClientAlbum = new HttpClient();
            string responseStringAlbum = await httpClientAlbum.GetStringAsync(new Uri(urlAlbum));
            searchAlbumResult = JsonConvert.DeserializeObject<SearchAlbumResultModel.RootObject>(responseStringAlbum);

            for (int i = 0; i < searchAlbumResult.album_list.Count; i++)
            {
                if (searchAlbumResult.album_list[i].cover_img == "")
                {
                    searchAlbumResult.album_list[i].cover_img = "ms-appx:///Images/logo.png";
                }
            }

            listAlbumResult = searchAlbumResult.album_list;

            grvAlbums.ItemsSource = listAlbumResult;
        }

        public async void SearchArtist(int page)
        {
            tblSearchType.Text = "Ca sĩ";
            lvResult.Visibility = Visibility.Visible;
            grvAlbums.Visibility = Visibility.Collapsed;
            string urlArtist = "http://search.chiasenhac.com/api/search.php?code=duc_wp_2014&return=json&s=" + searchKey + "&mode=artist" + "&page=" + page;
            HttpClient httpClientArtist = new HttpClient();
            string responseStringArtist = await httpClientArtist.GetStringAsync(new Uri(urlArtist));

            searchResultList = JsonConvert.DeserializeObject<SearchResultModel.RootObject>(responseStringArtist);

            for (int i = 0; i < searchResultList.music_list.Count; i++)
            {
                if (searchResultList.music_list[i].thumbnail == "")
                {
                    searchResultList.music_list[i].thumbnail = "ms-appx:///Images/Mp3.png";
                }
            }

            listSearchResult = searchResultList.music_list;

            lvResult.ItemsSource = listSearchResult;
        }

        public async void SearchVideo(int page)
        {
            try
            {
                tblSearchType.Text = "Video";
                lvResult.Visibility = Visibility.Visible;
                grvAlbums.Visibility = Visibility.Collapsed;
                string urlVideo = "http://search.chiasenhac.com/api/search.php?code=duc_wp_2014&return=json&s=" + searchKey + "&cat=video" + "&page=" + page;
                HttpClient httpClientVideo = new HttpClient();
                string responseStringVideo = await httpClientVideo.GetStringAsync(new Uri(urlVideo));
                searchResultList = JsonConvert.DeserializeObject<SearchResultModel.RootObject>(responseStringVideo);

                for (int i = 0; i < searchResultList.music_list.Count; i++)
                {
                    if (searchResultList.music_list[i].thumbnail == "")
                    {
                        searchResultList.music_list[i].thumbnail = "ms-appx:///Images/Mp3.png";
                    }
                }

                listSearchResult = searchResultList.music_list;

                lvResult.ItemsSource = listSearchResult;
            }
            catch
            { }
        }

        public async void SearchComposer(int page)
        {
            tblSearchType.Text = "Sáng tác";
            lvResult.Visibility = Visibility.Visible;
            grvAlbums.Visibility = Visibility.Collapsed;

            string urlComposer = "http://search.chiasenhac.com/api/search.php?code=duc_wp_2014&return=json&s=" + searchKey + "&mode=composer" + "&page=" + page;
            HttpClient httpClientComposer = new HttpClient();
            string responseStringComposer = await httpClientComposer.GetStringAsync(new Uri(urlComposer));
            searchResultList = JsonConvert.DeserializeObject<SearchResultModel.RootObject>(responseStringComposer);

            for (int i = 0; i < searchResultList.music_list.Count; i++)
            {
                if (searchResultList.music_list[i].thumbnail == "")
                {
                    searchResultList.music_list[i].thumbnail = "ms-appx:///Images/Mp3.png";
                }
            }

            listSearchResult = searchResultList.music_list;

            lvResult.ItemsSource = listSearchResult;
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

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;

            switch (item.Text)
            {
                case "Tất cả":
                    cat = "";
                    tblSearchType.Text = "Tất cả";
                    break;
                case "Ca sĩ":
                    cat = "artist";
                    tblSearchType.Text = "Ca sĩ";
                    break;
                case "Video":
                    cat = "video";
                    tblSearchType.Text = "Video";
                    break;
                case "Album":
                    cat = "album";
                    tblSearchType.Text = "Album";
                    break;
                case "Sáng tác":
                    cat = "composer";
                    tblSearchType.Text = "Sáng tác";
                    break;
            }
        }

        private void griSearchType_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void lvResult_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ScrollViewer sv = GetScrollViewer(this.lvResult);

                sv.ViewChanged += sv_ViewChanged;
            }
            catch(Exception ex)
            {
                switch (cat)
                {
                    case "album":
                        SearchAlbum(1);
                        break;

                    case "artist":
                        SearchArtist(1);
                        break;

                    case "video":
                        SearchVideo(1);
                        break;

                    case "":
                        SearchAll(1);
                        break;

                    case "composer":
                        SearchComposer(1);
                        break;
                }
            }
        }

        private async void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {            
           try
            {
                ScrollViewer sv = GetScrollViewer(this.lvResult);
                //double a = sv.ScrollableHeight - sv.ViewportHeight;
                var verticalOffsetValue = sv.VerticalOffset;

                if (verticalOffsetValue == sv.ScrollableHeight)
                {
                    prLoading.Visibility = Visibility.Visible;
                    page++;

                    switch (cat)
                    {
                        case "artist":
                            string urlArtist = "http://search.chiasenhac.com/api/search.php?code=duc_wp_2014&return=json&s=" + searchKey + "&mode=artist" + "&page=" + page;
                            HttpClient httpClientArtist = new HttpClient();
                            string responseStringArtist = await httpClientArtist.GetStringAsync(new Uri(urlArtist));

                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                SearchResultModel.RootObject searchResultListArtistMore = JsonConvert.DeserializeObject<SearchResultModel.RootObject>(responseStringArtist);

                                for (int i = 0; i < searchResultListArtistMore.music_list.Count; i++)
                                {
                                    if (searchResultListArtistMore.music_list[i].thumbnail == "")
                                    {
                                        searchResultListArtistMore.music_list[i].thumbnail = "ms-appx:///Images/Mp3.png";
                                    }
                                }

                                List<SearchResultModel.MusicList> listSearchResultArtistMore = searchResultListArtistMore.music_list;

                                for (int i = 0; i < listSearchResultArtistMore.Count; i++)
                                {
                                    listSearchResult.Add(listSearchResultArtistMore[i]);
                                }
                                lvResult.ItemsSource = null;
                                lvResult.ItemsSource = listSearchResult;

                                lvResult.ScrollIntoView(lvResult.Items[listSearchResult.Count - 50]);
                                prLoading.Visibility = Visibility.Collapsed;
                            });
                            break;

                        case "video":
                            string urlVideo = "http://search.chiasenhac.com/api/search.php?code=duc_wp_2014&return=json&s=" + searchKey + "&cat=video" + "&page=" + page;
                            HttpClient httpClientVideo = new HttpClient();
                            string responseStringVideo = await httpClientVideo.GetStringAsync(new Uri(urlVideo));

                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                SearchResultModel.RootObject searchResultListVideoMore = JsonConvert.DeserializeObject<SearchResultModel.RootObject>(responseStringVideo);

                                for (int i = 0; i < searchResultListVideoMore.music_list.Count; i++)
                                {
                                    if (searchResultListVideoMore.music_list[i].thumbnail == "")
                                    {
                                        searchResultListVideoMore.music_list[i].thumbnail = "ms-appx:///Images/Mp3.png";
                                    }
                                }

                                List<SearchResultModel.MusicList> listSearchResultArtistMore = searchResultListVideoMore.music_list;

                                for (int i = 0; i < listSearchResultArtistMore.Count; i++)
                                {
                                    listSearchResult.Add(listSearchResultArtistMore[i]);
                                }
                                lvResult.ItemsSource = null;
                                lvResult.ItemsSource = listSearchResult;

                                lvResult.ScrollIntoView(lvResult.Items[listSearchResult.Count - 50]);
                                prLoading.Visibility = Visibility.Collapsed;
                            });
                            break;

                        case "":
                            string urlAll = "http://search.chiasenhac.com/api/search.php?code=duc_wp_2014&return=json&s=" + searchKey + "&page=" + page;
                            HttpClient httpClientAll = new HttpClient();
                            string responseStringAll = await httpClientAll.GetStringAsync(new Uri(urlAll));

                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                SearchResultModel.RootObject searchResultListAllMore = JsonConvert.DeserializeObject<SearchResultModel.RootObject>(responseStringAll);

                                for (int i = 0; i < searchResultListAllMore.music_list.Count; i++)
                                {
                                    if (searchResultListAllMore.music_list[i].thumbnail == "")
                                    {
                                        searchResultListAllMore.music_list[i].thumbnail = "ms-appx:///Images/Mp3.png";
                                    }
                                }

                                List<SearchResultModel.MusicList> listSearchResultArtistMore = searchResultListAllMore.music_list;

                                for (int i = 0; i < listSearchResultArtistMore.Count; i++)
                                {
                                    listSearchResult.Add(listSearchResultArtistMore[i]);
                                }
                                lvResult.ItemsSource = null;
                                lvResult.ItemsSource = listSearchResult;

                                lvResult.ScrollIntoView(lvResult.Items[listSearchResult.Count - 50]);
                                prLoading.Visibility = Visibility.Collapsed;
                            });
                            break;

                        case "composer":
                            string urlComposer = "http://search.chiasenhac.com/api/search.php?code=duc_wp_2014&return=json&s=" + searchKey + "&mode=composer" + "&page=" + page;
                            HttpClient httpClientComposer = new HttpClient();
                            string responseStringComposer = await httpClientComposer.GetStringAsync(new Uri(urlComposer));

                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                SearchResultModel.RootObject searchResultListComposerMore = JsonConvert.DeserializeObject<SearchResultModel.RootObject>(responseStringComposer);

                                for (int i = 0; i < searchResultListComposerMore.music_list.Count; i++)
                                {
                                    if (searchResultListComposerMore.music_list[i].thumbnail == "")
                                    {
                                        searchResultListComposerMore.music_list[i].thumbnail = "ms-appx:///Images/Mp3.png";
                                    }
                                }

                                List<SearchResultModel.MusicList> listSearchResultArtistMore = searchResultListComposerMore.music_list;

                                for (int i = 0; i < listSearchResultArtistMore.Count; i++)
                                {
                                    listSearchResult.Add(listSearchResultArtistMore[i]);
                                }
                                lvResult.ItemsSource = null;
                                lvResult.ItemsSource = listSearchResult;

                                lvResult.ScrollIntoView(lvResult.Items[listSearchResult.Count - 50]);
                                prLoading.Visibility = Visibility.Collapsed;
                            });

                            break;
                    }

                    prLoading.Visibility = Visibility.Collapsed;
                }

            }
            catch
            { }

        }

        public ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer) return depObj as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
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

        private async void lvResult_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            SearchResultModel.MusicList itemSelected = (e.OriginalSource as FrameworkElement).DataContext as SearchResultModel.MusicList;
            BXH.fileName = itemSelected.music_title;

            if (itemSelected.cat_id != "1")
            {
                try
                {
                    string url = "http://chiasenhac.com/api/listen.php?code=duc_wp_2014&return=json&m=" + itemSelected.music_id + "&url=" + itemSelected.music_title_url;

                    HttpClient httpClient = new HttpClient();
                    string results = await httpClient.GetStringAsync(new Uri(url));

                    SongDetail.RootObject ro = JsonConvert.DeserializeObject<SongDetail.RootObject>(results);

                    if (ro.music_info.file_32_url != "")
                    {
                        downloadLink32 = ro.music_info.file_32_url;
                    }
                    if (ro.music_info.file_url != "")
                    {
                        downloadLink128 = ro.music_info.file_url;
                    }
                    if (ro.music_info.file_320_url != "")
                    {
                        downloadLink320 = ro.music_info.file_320_url;
                    }
                    if (ro.music_info.file_m4a_url != "")
                    {
                        downloadLinkm4a = ro.music_info.file_m4a_url;
                    }
                }
                catch (Exception)
                {

                }
            }

            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private async void lvResult_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            SearchResultModel.MusicList itemSelected = (e.OriginalSource as FrameworkElement).DataContext as SearchResultModel.MusicList;
            BXH.fileName = itemSelected.music_title;

            if (itemSelected.cat_id != "1")
            {
                string url = "http://chiasenhac.com/api/listen.php?code=duc_wp_2014&return=json&m=" + itemSelected.music_id + "&url=" + itemSelected.music_title_url;

                HttpClient httpClient = new HttpClient();
                string results = await httpClient.GetStringAsync(new Uri(url));

                SongDetail.RootObject ro = JsonConvert.DeserializeObject<SongDetail.RootObject>(results);

                if (ro.music_info.file_32_url != "")
                {
                    downloadLink32 = ro.music_info.file_32_url;
                }
                if (ro.music_info.file_url != "")
                {
                    downloadLink128 = ro.music_info.file_url;
                }
                if (ro.music_info.file_320_url != "")
                {
                    downloadLink320 = ro.music_info.file_320_url;
                }
                if (ro.music_info.file_m4a_url != "")
                {
                    downloadLinkm4a = ro.music_info.file_m4a_url;
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

        private void grvAlbums_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var panel = (ItemsWrapGrid)grvAlbums.ItemsPanelRoot;

            if (e.NewSize.Width < 433)
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

                SearchAlbumResultModel.AlbumList itemSelected = (SearchAlbumResultModel.AlbumList)e.ClickedItem;

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
            }
            catch (Exception)
            {

            }
        }
    }
}
