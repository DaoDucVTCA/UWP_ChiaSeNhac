using BackgroundAudioShare;
using BackgroundAudioShare.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ChiasenhacUniversal.Pages
{
    public sealed partial class Setting : Page
    {
        
        public Setting()
        {
            this.InitializeComponent();
        }

        private void cbbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mainPage = (MainPage)(Window.Current.Content as Frame).Content;
            string themeName = e.AddedItems[0].ToString();

            switch(themeName)
            {
                case "Trắng":
                    App.themeColor.Values["color"] = 0;
                    mainPage.RequestedTheme = ElementTheme.Light;
                    this.RequestedTheme = ElementTheme.Light;
                    this.UpdateLayout();
                    break;
                case "Đen":
                    App.themeColor.Values["color"] = 1;
                    mainPage.RequestedTheme = ElementTheme.Dark;
                    this.RequestedTheme = ElementTheme.Dark;
                    this.UpdateLayout();
                    break;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var a = Convert.ToInt32(App.themeColor.Values["color"]);
            if (a == 0)
            {
                cbbTheme.PlaceholderText = "Trắng";
            }
            else if (a == 1)
            {
                cbbTheme.PlaceholderText = "Đen";
            }

            var b = Convert.ToInt32(App.bitrate.Values["bitrate"]);
            if(b == 0)
            {
                cbbBitrate.PlaceholderText = "128kbs";
            }
            else if(b == 32)
            {
                cbbBitrate.PlaceholderText = "32kbs";
            }
            else if(b == 128)
            {
                cbbBitrate.PlaceholderText = "128kbs";
            }
            else if (b == 320)
            {
                cbbBitrate.PlaceholderText = "320kbs";
            }
            else if (b == 500)
            {
                cbbBitrate.PlaceholderText = "m4a";
            }
            else if (b == 1000)
            {
                cbbBitrate.PlaceholderText = "flac";
            }
        }

        private void cbbBitrate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string bitrate = e.AddedItems[0].ToString();

            switch(bitrate)
            {
                case "32kbs":
                    App.bitrate.Values["bitrate"] = 32;
                    MessageService.SendMessageToBackground(new BitrateChangeMessage("32kbs"));
                    break;
                case "128kbs":
                    App.bitrate.Values["bitrate"] = 128;
                    MessageService.SendMessageToBackground(new BitrateChangeMessage("128kbs"));
                    break;
                case "320kbs":
                    App.bitrate.Values["bitrate"] = 320;
                    MessageService.SendMessageToBackground(new BitrateChangeMessage("320kbs"));
                    break;
                case "m4a":
                    App.bitrate.Values["bitrate"] = 500;
                    MessageService.SendMessageToBackground(new BitrateChangeMessage("m4a"));
                    break;
                case "flac":
                    App.bitrate.Values["bitrate"] = 1000;
                    MessageService.SendMessageToBackground(new BitrateChangeMessage("flac"));
                    break;
            }
        }
    }
}
