using ChiasenhacUniversal.Pages;
using ChiasenhacUniversal.ViewModel.Dowload;
using System;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ChiasenhacUniversal.Controls
{
    public sealed partial class DownloadingItem : UserControl
    {
        DownloadOperation dl;

        public DownloadingItem(DownloadOperation dowload)
        {
            this.InitializeComponent();

            dl = dowload;

            if (dowload.Progress.Status == BackgroundTransferStatus.PausedByApplication)
            {
                dowload.Resume();
            }
            else if (dowload.Progress.Status != BackgroundTransferStatus.Running)
            {
                try
                {
                    dowload.Resume();
                }
                catch
                {
                }
            }

            try
            {
                nameControl.Text = BXH.fileName;
            }
            catch { }

            var a = dowload.Progress.TotalBytesToReceive;

            var b = dowload.Progress.BytesReceived;

            var totalByte = FileSizeHelper.GetFileSize(a);

            var receiveByte = FileSizeHelper.GetFileSize(b);

            if (a != 0)
            {
                sizeControl.Text = FileSizeHelper.GetFileSize(b) + " / " + FileSizeHelper.GetFileSize(a) + " ( " + (int)(b * 100 / a) + " % )";
                progessbarControl.Value = (int)(b * 100 / a);
            }

            if (progessbarControl.Value == 100)
            {
                statusControl.Text = "Complete";
            }
        }

        public async Task HandleDownloadAsync()
        {
            var mainPage = (MainPage)(Window.Current.Content as Frame).Content;
            Progress<DownloadOperation> progessDownload = new Progress<DownloadOperation>(UpdateDownloadProgress);
            try
            {
                await dl.AttachAsync().AsTask(progessDownload);
            }
            finally
            {
                statusControl.Text = "Complete";
                griDownloadItem.Visibility = Visibility.Collapsed;

                mainPage.griDownloaded.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                mainPage.griDownloaded.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateDownloadProgress(DownloadOperation download)
        {
            var mainPage = (MainPage)(Window.Current.Content as Frame).Content;

            try
            {
                var a = dl.Progress.TotalBytesToReceive;

                var b = dl.Progress.BytesReceived;

                sizeControl.Text = FileSizeHelper.GetFileSize(b) + " / " + FileSizeHelper.GetFileSize(a) + " ( " + (int)(b * 100 / a) + " % )";

                statusControl.Text = dl.Progress.Status.ToString();

                progessbarControl.Value = (int)(b * 100 / a);
                
            }
            catch (Exception)
            {

            }
        }

        private async void griDownloadItem_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await HandleDownloadAsync();
            }
            catch { }
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (dl.Progress.Status == BackgroundTransferStatus.Running)
            {
                dl.Pause();
                btnPause.Content = "Continute";
            }

            if (dl.Progress.Status == BackgroundTransferStatus.PausedByApplication)
            {
                dl.Resume();
                btnPause.Content = "Pause";
            }
        }

        private async void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await dl.ResultFile.DeleteAsync();
                griDownloadItem.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                Windows.UI.ViewManagement.InputPane.GetForCurrentView().TryHide();
            }
            catch { }
        }
    }
}
