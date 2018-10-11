using ChiasenhacUniversal.Controls;
using System;
using System.Collections.Generic;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace ChiasenhacUniversal.Pages
{
    public sealed partial class DownloadManager : Page
    {
        public DownloadManager()
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

        async void getStatusDownload()
        {
            lvDownload.Items.Clear();
            var activeDownloads = new List<DownloadOperation>();

            IReadOnlyList<DownloadOperation> downloads = null;

            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            }
            catch (Exception)
            {
            }
            if (downloads.Count > 0)
            {
                for (int i = 0; i < downloads.Count; i++)
                {

                    try
                    {
                        DownloadingItem itemDownload = new DownloadingItem(downloads[i]);

                        itemDownload.Margin = new Thickness(10, 5, 10, 5);

                        lvDownload.Items.Add(itemDownload);
                    }
                    catch { }
                }
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            getStatusDownload();
        }
    }
}
