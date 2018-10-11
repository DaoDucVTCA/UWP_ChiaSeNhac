using BackgroundAudioShare;
using BackgroundAudioShare.Model;
using ChiasenhacUniversal.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using Windows.ApplicationModel.Store;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace ChiasenhacUniversal.Pages
{
    public sealed partial class Videos : Page
    {
        public TypeMusicModel.RootObject ro;

        public Videos()
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

            LoadData("v-video");
        }

        public async void LoadData(string typeName)
        {
            try
            {
                prLoading.Visibility = Visibility.Visible;
                string url = "http://chiasenhac.com/api/category.php?code=duc_wp_2014&return=json&c=" + typeName;

                HttpClient httpClient = new HttpClient();
                string results = await httpClient.GetStringAsync(new Uri(url));

                ro = JsonConvert.DeserializeObject<TypeMusicModel.RootObject>(results);

                lvVideoNew.ItemsSource = ro.@new.music;
                lvVideoHot.ItemsSource = ro.hot.music;


                prLoading.Visibility = Visibility.Collapsed;
            }
            catch (Exception)
            {

            }
        }

        private void griTitleMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;

            switch (item.Text)
            {
                case "Video Việt Nam":
                    tblBXHTitle.Text = "Video Việt Nam";
                    LoadData("v-video");
                    break;
                case "Video Âu, Mĩ":
                    LoadData("u-video");
                    break;
                case "Video Hàn":
                    LoadData("k-video");
                    break;
                case "Video Hoa":
                    LoadData("c-video");
                    break;
                case "Video nước khác":
                    LoadData("o-video");
                    break;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }
        private async void lvVideoNew_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
                {
                    BackgroundMediaPlayer.Current.Pause();
                }
                var mainPage = (MainPage)(Window.Current.Content as Frame).Content;
                mainPage.nowPlaying.Visibility = Visibility.Collapsed;
                TypeMusicModel.Music2 itemSelected = (TypeMusicModel.Music2)e.ClickedItem;

                string url = "http://chiasenhac.com/api/listen.php?code=duc_wp_2014&return=json&m=" + itemSelected.music_id + "&url=" + itemSelected.music_title_url;

                HttpClient httpClient = new HttpClient();
                string results = await httpClient.GetStringAsync(new Uri(url));

                SongDetail.RootObject ro = JsonConvert.DeserializeObject<SongDetail.RootObject>(results);

                if (ro.music_info != null)
                {
                    Frame.Navigate(typeof(PlayVideo), ro.music_info);
                }
                
            }
            catch (Exception)
            {

            }
            
        }

        private async void lvVideoHot_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
                {
                    BackgroundMediaPlayer.Current.Pause();
                }

                var mainPage = (MainPage)(Window.Current.Content as Frame).Content;
                mainPage.nowPlaying.Visibility = Visibility.Collapsed;

                TypeMusicModel.Music itemSelected = (TypeMusicModel.Music)e.ClickedItem;

                string url = "http://chiasenhac.com/api/listen.php?code=duc_wp_2014&return=json&m=" + itemSelected.music_id + "&url=" + itemSelected.music_title_url;

                HttpClient httpClient = new HttpClient();
                string results = await httpClient.GetStringAsync(new Uri(url));

                SongDetail.RootObject ro = JsonConvert.DeserializeObject<SongDetail.RootObject>(results);

                Frame.Navigate(typeof(PlayVideo), ro.music_info);
            }
            catch { }
        }
    }
}
