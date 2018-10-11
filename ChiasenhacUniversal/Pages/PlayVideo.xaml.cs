using BackgroundAudioShare.Model;
using ChiasenhacUniversal.ViewModel.Dowload;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace ChiasenhacUniversal.Pages
{
    public sealed partial class PlayVideo : Page
    {
        DispatcherTimer timer;
        public SongDetail.MusicInfo item;
        public bool isHideControl = false;
        public StorageFile destinationFile;
        DownloadOperation _download;
        public string downloadLink;
        public string link360p;
        public string link480p;
        public string link720p;
        public string link1080p;
        public MediaElement mediaPlay { get { return mediaPlay; } }
        public static bool isBack = false;

        public PlayVideo()
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

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.System.Display.DisplayRequest KeepScreenOnRequest = new Windows.System.Display.DisplayRequest();

            KeepScreenOnRequest.RequestActive();

            item = e.Parameter as SongDetail.MusicInfo;
            tblVideoTitle.Text = item.music_title;
            if(item.music_width == "1920" && item.file_lossless_url == "")
            {
                item.file_lossless_url = item.file_m4a_url.Replace("/m4a/", "/flac/").Replace(".mp4", " [MP4 HD 1080p].mp4");
            }
            link360p = item.file_url;
            link480p = item.file_320_url;
            link720p = item.file_m4a_url;
            link1080p = item.file_lossless_url;

            
            mePlay.Source = new Uri(item.file_url, UriKind.Absolute);
            mePlay.Play();
            try
            {
                if (CurrentApp.LicenseInformation.ProductLicenses["Chiasenhac"].IsActive)
                {
                    grads.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grads.Visibility = Visibility.Visible;
                }
            }
            catch (Exception)
            {
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {

        }

        private void mePlay_MediaOpened(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, object e)
        {

            int length = int.Parse(item.music_length);
            int minutes = length / 60;
            int seconds = length % 60;

            if (mePlay.Position.Seconds != 0 && mePlay.Position.Seconds % 5 == 0 && isHideControl == false)
            {
                stbHideTop.Begin();
                isHideControl = true;
            }
        }

        private void griControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            stbHideTop.Begin();
            isHideControl = true;
        }

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (isHideControl == true)
            {
                stbShowTop.Begin();
                isHideControl = false;
            }
        }

        private void vbPlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaElementState.Playing == mePlay.CurrentState)
            {
                mePlay.Pause();
            }
            else if(MediaElementState.Paused == mePlay.CurrentState)
            {
                mePlay.Play();
            }
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;

            switch (item.Text)
            {
                case "360p":
                    mePlay.Source = new Uri(link360p, UriKind.Absolute);
                    mePlay.Play();
                    tblQuality.Text = "360p";
                    break;
                case "480p":
                    if (link480p != "")
                    {
                        mePlay.Source = new Uri(link480p, UriKind.Absolute);
                        mePlay.Play();
                        tblQuality.Text = "480p";
                    }
                    else
                    {
                        griNoti.Visibility = Visibility.Visible;
                        await Task.Delay(2000);
                        griNoti.Visibility = Visibility.Collapsed;
                    }
                    break;
                case "720p":
                    if (link720p != "")
                    {
                        mePlay.Source = new Uri(link720p, UriKind.Absolute);
                        mePlay.Play();
                        tblQuality.Text = "720p";
                    }
                    else
                    {
                        griNoti.Visibility = Visibility.Visible;
                        await Task.Delay(2000);
                        griNoti.Visibility = Visibility.Collapsed;
                    }
                    break;
                case "1080p":
                    if (link1080p != "")
                    {
                        mePlay.Source = new Uri(link1080p, UriKind.Absolute);
                        mePlay.Play();
                        tblQuality.Text = "1080p";
                    }
                    else
                    {
                        griNoti.Visibility = Visibility.Visible;
                        await Task.Delay(2000);
                        griNoti.Visibility = Visibility.Collapsed;
                    }
                    break;
            }
        }

        private void vbQuality_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void vbDownload_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private async void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;

            switch (item.Text)
            {
                case "360p":
                    downloadLink = link360p;
                    break;
                case "480p":
                    downloadLink = link480p;
                    break;
                case "720p":
                    if(link720p != "")
                    {
                        downloadLink = link720p;
                    }
                    break;
                case "1080p":
                    if(link1080p != "")
                    {
                        downloadLink = link1080p;
                    }
                    break;
            }

            if(downloadLink != "")
            {
                IReadOnlyList<DownloadOperation> downloads = null;

                try
                {
                    downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
                }
                catch (Exception)
                {

                }

                Uri source = new Uri(downloadLink, UriKind.RelativeOrAbsolute);

                destinationFile = await KnownFolders.VideosLibrary.CreateFileAsync(tblVideoTitle.Text + ".mp4", CreationCollisionOption.GenerateUniqueName);

                var downloader = new BackgroundDownloader();

                _download = downloader.CreateDownload(source, destinationFile);
                _download.Priority = BackgroundTransferPriority.Default;

                await HandleDownloadAsync();
            }
            else
            {
                
            }
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
                
            }
        }

        private void UpdateDownloadProgress(DownloadOperation download)
        {
            var a = _download.Progress.TotalBytesToReceive;

            var b = _download.Progress.BytesReceived;            
        }

        #endregion

        //private void AdControl_ErrorOccurred(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        //{
        //    grads.Visibility = Visibility.Collapsed;
        //}

        private async void SymbolIcon_Tapped(object sender, TappedRoutedEventArgs e)
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
                        grads.Visibility = Visibility.Collapsed;
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
            grads.Visibility = Visibility.Collapsed;
            await Task.Delay(600000);
            grads.Visibility = Visibility.Visible;

        }
    }
}
