using BackgroundAudioShare;
using BackgroundAudioShare.Messages;
using BackgroundAudioShare.Model;
using BackgroundAudioTask;
using ChiasenhacUniversal.Controls;
using ChiasenhacUniversal.Model;
using ChiasenhacUniversal.Pages;
using ChiasenhacUniversal.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Imaging;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace ChiasenhacUniversal
{
    public sealed partial class MainPage : Page
    {
        public static MainPage Current = null;
        private bool isMyBackgroundTaskRunning = false;
        public event TypedEventHandler<MainPage, Rect> TogglePaneButtonRectChanged;
        public Frame AppFrame { get { return frame; } }
        public TextBlock songTitle {get {return tblSongTitle; }}
        public TextBlock songArtist { get { return tblSongArtist; } }
        public Image songThumbnail { get { return imgSongThumbnail; } }
        public Grid nowPlaying { get { return griNowPlaying; } }
        public ToggleButton menuButton { get { return TogglePaneButton; } }
        public SplitView spvMenu { get { return RootSplitView; } }
        public NavMenuListView ucMenu { get { return NavMenuList; } }
        public SymbolIcon sybPlay { get { return sbiPlay; } }
        public object DisplayInfomation { get; private set; }
        public Grid nowPlayingInfo { get { return griNowPlayingInfo; } }
        public Grid griDownloaded { get { return griMess; } }
        public Grid griPlaying { get { return griNowPlaying; } }
        public static List<SongDetail.MusicInfo> listListenedSongs = new List<SongDetail.MusicInfo>();
        
        ApplicationDataContainer _roamingData = ApplicationData.Current.RoamingSettings;
        ApplicationDataContainer _localData = ApplicationData.Current.LocalSettings;
        ApplicationDataContainer countData = ApplicationData.Current.LocalSettings;
        int countNumber;
        
        #region MenuItem
        private List<NavMenuItem> navlist = new List<NavMenuItem>(new[]
            {
                new NavMenuItem()
                {
                    Symbol = Symbol.Find,
                    Label = "Tìm kiếm",
                    DestPage = typeof(Search)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Home,
                    Label = "Trang chủ",
                    DestPage = typeof(Home)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Bookmarks,
                    Label = "Bảng xếp hạng",
                    DestPage = typeof(BXH)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.MusicInfo,
                    Label = "Chuyên mục",
                    DestPage = typeof(AllMusic)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Video,
                    Label = "Video",
                    DestPage = typeof(Videos)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Favorite,
                    Label = "Yêu thích",
                    DestPage = typeof(Favorite)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Play,
                    Label = "Now playing",
                    DestPage = typeof(NowPlayingDesktop)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Download,
                    Label = "Quản lý download",
                    DestPage = typeof(DownloadManager)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Setting,
                    Label = "Cài đặt",
                    DestPage = typeof(Setting)
                },
            });
        #endregion

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

        string GetMusicId(SongDetail.MusicInfo item)
        {
            if (item == null)
                return null;
            else
                return item.music_id;
        }

        public Rect TogglePaneButtonRect
        {
            get;
            private set;
        }
        
        public MainPage()
        {
            this.InitializeComponent();

            var v = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            var allProperties = v.GetType().GetRuntimeProperties();
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            

            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

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

            this.Loaded += (sender, args) =>
            {
                Current = this;

                this.TogglePaneButton.Focus(FocusState.Programmatic);
            };

            BackgroundMediaPlayer.MessageReceivedFromBackground += this.BackgroundMediaPlayer_MessageReceivedFromBackground;

            NavMenuList.ItemsSource = navlist;

            var item = navlist[1];

            if (item != null)
            {
                if (item.DestPage != null &&
                    item.DestPage != this.AppFrame.CurrentSourcePageType)
                {
                    if (item.Label == "Trang chủ" && ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                    {
                        AppFrame.Navigate(typeof(HomeMobile));
                    }
                    else
                    {
                        this.AppFrame.Navigate(item.DestPage, item.Arguments);
                    }

                }
            }

            SystemNavigationManager.GetForCurrentView().BackRequested += SystemNavigationManager_BackRequested;

        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            bool handled = e.Handled;
            this.BackRequested(ref handled);
            e.Handled = handled;
        }

        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            TrackChangedMessage trackChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out trackChangedMessage))
            {
                try
                {
                    int index = BXH.listAllSongs.FindIndex(item =>
                            GetMusicId(item) == (string)trackChangedMessage.MusicId);
                    Home.nowPlayingSong = BXH.listAllSongs[index];

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
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        songTitle.Text = Home.nowPlayingSong.music_title;
                        songArtist.Text = Home.nowPlayingSong.music_artist;
                        if(Home.nowPlayingSong.music_img == "")
                        {
                            songThumbnail.Source = new BitmapImage(new Uri("ms-appx:///Images/logo.png", UriKind.RelativeOrAbsolute));
                        }
                        else
                        {
                            songThumbnail.Source = new BitmapImage(new Uri(Home.nowPlayingSong.music_img, UriKind.RelativeOrAbsolute));
                        }

                        GetDominantColor();
                    });

                    BXH.CheckListenedSong();
                    NowPlayingMobile.UpdateLiveTile();
                }
                catch { }

            }
        }

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            bool handled = e.Handled;
            this.BackRequested(ref handled);
            e.Handled = handled;
        }

        private void NavMenuList_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (!args.InRecycleQueue && args.Item != null && args.Item is NavMenuItem)
            {
                args.ItemContainer.SetValue(AutomationProperties.NameProperty, ((NavMenuItem)args.Item).Label);
            }
            else
            {
                args.ItemContainer.ClearValue(AutomationProperties.NameProperty);
            }
        }

        private void Page_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            FocusNavigationDirection direction = FocusNavigationDirection.None;
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Left:
                case Windows.System.VirtualKey.GamepadDPadLeft:
                case Windows.System.VirtualKey.GamepadLeftThumbstickLeft:
                case Windows.System.VirtualKey.NavigationLeft:
                    direction = FocusNavigationDirection.Left;
                    break;
                case Windows.System.VirtualKey.Right:
                case Windows.System.VirtualKey.GamepadDPadRight:
                case Windows.System.VirtualKey.GamepadLeftThumbstickRight:
                case Windows.System.VirtualKey.NavigationRight:
                    direction = FocusNavigationDirection.Right;
                    break;

                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.GamepadDPadUp:
                case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
                case Windows.System.VirtualKey.NavigationUp:
                    direction = FocusNavigationDirection.Up;
                    break;

                case Windows.System.VirtualKey.Down:
                case Windows.System.VirtualKey.GamepadDPadDown:
                case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
                case Windows.System.VirtualKey.NavigationDown:
                    direction = FocusNavigationDirection.Down;
                    break;
            }

            if (direction != FocusNavigationDirection.None)
            {
                var control = FocusManager.FindNextFocusableElement(direction) as Control;
                if (control != null)
                {
                    control.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                    e.Handled = true;
                }
            }
        }

        private void frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                var item = (from p in this.navlist where p.DestPage == e.SourcePageType select p).SingleOrDefault();
                if (item == null && this.AppFrame.BackStackDepth > 0)
                {
                    foreach (var entry in this.AppFrame.BackStack.Reverse())
                    {
                        item = (from p in this.navlist where p.DestPage == entry.SourcePageType select p).SingleOrDefault();
                        if (item != null)
                            break;
                    }
                }

                var container = (ListViewItem)NavMenuList.ContainerFromItem(item);

                if (container != null) container.IsTabStop = false;
                NavMenuList.SetSelectedItem(container);
                if (container != null) container.IsTabStop = true;
            }
        }

        private void frame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (e.Content is Page && e.Content != null)
            {
                var control = (Page)e.Content;
                control.Loaded += Page_Loaded;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((Page)sender).Focus(FocusState.Programmatic);
            ((Page)sender).Loaded -= Page_Loaded;
            this.CheckTogglePaneButtonSizeChanged();

            if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
            {
                sbiPlay.Symbol = Symbol.Pause;
            }
            else if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Paused)
            {
                sbiPlay.Symbol = Symbol.Play;
            }
        }

        private void TogglePaneButton_Unchecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.CheckTogglePaneButtonSizeChanged();
        }

        private void CheckTogglePaneButtonSizeChanged()
        {
            if (this.RootSplitView.DisplayMode == SplitViewDisplayMode.Inline ||
                this.RootSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                var transform = this.TogglePaneButton.TransformToVisual(this);
                var rect = transform.TransformBounds(new Rect(0, 0, this.TogglePaneButton.ActualWidth, this.TogglePaneButton.ActualHeight));
                this.TogglePaneButtonRect = rect;
            }
            else
            {
                this.TogglePaneButtonRect = new Rect();
            }

            var handler = this.TogglePaneButtonRectChanged;
            if (handler != null)
            {
                // handler(this, this.TogglePaneButtonRect);
                handler.DynamicInvoke(this, this.TogglePaneButtonRect);
            }
        }

        private void NavMenuList_ItemInvoked(object sender, ListViewItem listViewItem)
        {
            var item = (NavMenuItem)((NavMenuListView)sender).ItemFromContainer(listViewItem);

            if (item != null)
            {
                if (item.DestPage != null && item.DestPage != this.AppFrame.CurrentSourcePageType)
                {
                    if(item.Label == "Now playing" && ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                    {
                        AppFrame.Navigate(typeof(NowPlayingMobile));
                    }
                    else if(item.Label == "Trang chủ" && ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                    {
                        AppFrame.Navigate(typeof(HomeMobile));
                    }
                    else
                    {
                        AppFrame.Navigate(item.DestPage, item.Arguments);
                    }
                    
                }
            }
        }

        private void griNowPlaying_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                AppFrame.Navigate(typeof(NowPlayingMobile));
            } 
            else
            {
                AppFrame.Navigate(typeof(NowPlayingDesktop));
                RootSplitView.IsPaneOpen = !RootSplitView.IsPaneOpen;
            }          
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                await StatusBar.GetForCurrentView().HideAsync();
            }

            bool isFileExist = await WriteAndReadFile.CheckFileExist("ListNowPlaying");

            if (isFileExist == true)
            {
                try
                {
                    string listenedSongData = await WriteAndReadFile.ReadFile("ListListenedSongs");
                    listListenedSongs = JsonConvert.DeserializeObject<List<SongDetail.MusicInfo>>(listenedSongData);

                    string listNowPlaying = await WriteAndReadFile.ReadFile("ListNowPlaying");
                    Home.nowPlayingSong = listListenedSongs[0];

                    MessageService.SendMessageToBackground(new ChangedPlaylistMessage(JsonConvert.DeserializeObject<List<SongDetail.MusicInfo>>(listNowPlaying)));

                    BXH.listAllSongs = JsonConvert.DeserializeObject<List<SongDetail.MusicInfo>>(listNowPlaying);
                    songTitle.Text = Home.nowPlayingSong.music_title;
                    songArtist.Text = Home.nowPlayingSong.music_artist;
                    if (Home.nowPlayingSong.music_img == "")
                    {
                        Home.nowPlayingSong.music_img = "ms-appx:///Images/logo.png";
                    }
                    songThumbnail.Source = new BitmapImage(new Uri(Home.nowPlayingSong.music_img, UriKind.RelativeOrAbsolute));
                }
                catch(Exception ex)
                {
                    ex.ToString();
                }
            }
            else
            {
                griNowPlaying.Visibility = Visibility.Collapsed;
            }

            GetDominantColor();
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Active.ToString());

            try
            {
                bool a = (bool)_localData.Values["AppRatingDone"];
                if(a != true)
                {
                    if (Convert.ToInt32(countData.Values["Number"]) != null)
                    {
                        countNumber = Convert.ToInt32(countData.Values["Number"]) + 1;
                    }

                    countData.Values["Number"] = countNumber;


                    if (Convert.ToInt32(countData.Values["Number"]) % 8 == 0)
                    {
                        await AskForAppRating();
                    }
                }
                else
                {

                }
                
            }
            catch(Exception ex)
            {
                if (Convert.ToInt32(countData.Values["Number"]) != null)
                {
                    countNumber = Convert.ToInt32(countData.Values["Number"]) + 1;
                }

                countData.Values["Number"] = countNumber;


                if (Convert.ToInt32(countData.Values["Number"]) % 8 == 0)
                {
                    await AskForAppRating();
                }
            }
        }

        private async Task AskForAppRating()
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:review?PFN=" + Windows.ApplicationModel.Package.Current.Id.FamilyName));
            SaveAppRatingSetting();
        }

        private void SaveAppRatingSetting()
        {
            // for this setting, it's either saved or not, so we save true as the default value
            try
            {
                _localData.Values["AppRatingDone"] = true;
                _roamingData.Values["AppRatingDone"] = true;
            }
            catch { }

        }

        #region Foreground App Lifecycle Handlers

        void ForegroundApp_Resuming(object sender, object e)
        {
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Active.ToString());

            // Verify the task is running
            if (IsMyBackgroundTaskRunning)
            {
                // If yes, it's safe to reconnect to media play handlers
                AddMediaPlayerEventHandlers();

                // Send message to background task that app is resumed so it can start sending notifications again
                MessageService.SendMessageToBackground(new AppResumedMessage());

                //UpdateTransportControls(BackgroundMediaPlayer.Current.CurrentState);

                //var trackId = GetCurrentTrackIdAfterAppResume();
                //txtCurrentTrack.Text = trackId == null ? string.Empty : playlistView.GetSongById(trackId).Title;
                //txtCurrentState.Text = BackgroundMediaPlayer.Current.CurrentState.ToString();
            }
            else
            {
                //playButton.Content = ">";     // Change to play button
                //txtCurrentTrack.Text = string.Empty;
                //txtCurrentState.Text = "Background Task Not Running";
            }
        }

        void ForegroundApp_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // Only if the background task is already running would we do these, otherwise
            // it would trigger starting up the background task when trying to suspend.
            if (IsMyBackgroundTaskRunning)
            {
                // Stop handling player events immediately
                //RemoveMediaPlayerEventHandlers();

                // Tell the background task the foreground is suspended
                MessageService.SendMessageToBackground(new AppSuspendedMessage());
            }

            // Persist that the foreground app is suspended
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Suspended.ToString());

            deferral.Complete();
        }

        #endregion

        #region Background Task

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
            BackgroundMediaPlayer.Current.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground += this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            var currentState = sender.CurrentState; // cache outside of completion or you might get a different value
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {

                // Update controls
                //UpdateTransportControls(currentState);
            });
        }

        #endregion

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(e.NewSize.Width < 433)
            {
                TogglePaneButton.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            bool ignored = false;
            this.BackRequested(ref ignored);
        }

        private void BackRequested(ref bool handled)
        {
            if (this.AppFrame == null)
                return;

            // Check to see if this is the top-most page on the app back stack.
            if (this.AppFrame.CanGoBack && !handled)
            {
                // If not, set the event to handled and go back to the previous page in the app.
                try
                {
                    handled = true;
                    this.AppFrame.GoBack();
                }
                catch
                { }
            }
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

                sbiPlay.Symbol = Symbol.Pause;
            }

        }

        private void vbNext_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
            {
                MessageService.SendMessageToBackground(new SkipNextMessage());
            }
            else if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
            {
                MessageService.SendMessageToBackground(new SkipNextMessage());
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

            BXH.CheckListenedSong();

            sbiPlay.Symbol = Symbol.Pause;
            
        }

        private void vbPrevious_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
            {
                MessageService.SendMessageToBackground(new SkipPreviousMessage());
            }
            else if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
            {
                MessageService.SendMessageToBackground(new SkipPreviousMessage());
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

            BXH.CheckListenedSong();

            sbiPlay.Symbol = Symbol.Pause;
        }

        public async void GetDominantColor()
        {
            try
            {
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

                if (bytes[0] <= 140 || bytes[1] <= 140 || bytes[2] <= 140)
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        griNowPlaying.Background = new SolidColorBrush(myDominantColor);
                    });
                }
                else
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        griNowPlaying.Background = new SolidColorBrush(Color.FromArgb(255, 0, 171, 233));
                    });

                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }
    }
}
