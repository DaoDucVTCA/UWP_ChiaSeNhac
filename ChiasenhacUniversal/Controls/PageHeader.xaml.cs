using ChiasenhacUniversal.Model;
using ChiasenhacUniversal.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ChiasenhacUniversal.Controls
{
    public sealed partial class PageHeader : UserControl
    {
        public List<SearchKey> listSearchKey = new List<SearchKey>();
        bool isKeyExist;
        public event EventHandler SearchTap;

        public PageHeader()
        {
            this.InitializeComponent();
            SearchTap?.Invoke(new object(), new EventArgs());
            this.Loaded += (s, a) =>
            {
                MainPage.Current.TogglePaneButtonRectChanged += Current_TogglePaneButtonSizeChanged;
                this.titleBar.Margin = new Thickness(MainPage.Current.TogglePaneButtonRect.Right, 0, 0, 0);
            };

            asbSearch.Visibility = Visibility.Visible;
            asbSearch.Focus(FocusState.Keyboard);
        }

        
        private void Current_TogglePaneButtonSizeChanged(MainPage sender, Rect e)
        {
            this.titleBar.Margin = new Thickness(e.Right, 0, 0, 0);
        }

        public UIElement HeaderContent
        {
            get { return (UIElement)GetValue(HeaderContentProperty); }
            set { SetValue(HeaderContentProperty, value); }
        }

        public static readonly DependencyProperty HeaderContentProperty =
            DependencyProperty.Register("HeaderContent", typeof(UIElement), typeof(PageHeader), new PropertyMetadata(DependencyProperty.UnsetValue));

        private void grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(Window.Current.Bounds.Width < 433)
            {
                asbSearch.Visibility = Visibility.Collapsed;
                siSearch.Visibility = Visibility.Visible;
            }
            else
            {
                asbSearch.Visibility = Visibility.Visible;
                siSearch.Visibility = Visibility.Collapsed;
            }
        }

        

        public void siSearch_Tapped(object sender, TappedRoutedEventArgs e)
        {
            asbSearch.Visibility = Visibility.Visible;
            siSearch.Visibility = Visibility.Collapsed;
            asbSearch.Focus(FocusState.Keyboard);
        }

        private void asbSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width < 433)
            {
                asbSearch.Visibility = Visibility.Collapsed;
                siSearch.Visibility = Visibility.Visible;
            }
            
        }

        private async void asbSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if(asbSearch.Text != null)
            {
                bool isFileExist = await WriteAndReadFile.CheckFileExist("ListSearchKey");
                

                if(isFileExist == true)
                {
                    string listKey = await WriteAndReadFile.ReadFile("ListSearchKey");
                    listSearchKey = JsonConvert.DeserializeObject<List<SearchKey>>(listKey);
                    if(listSearchKey != null)
                    {
                        for (int i = 0; i < listSearchKey.Count; i++)
                        {
                            if (listSearchKey[i].search_key.ToLower() == asbSearch.Text.ToLower())
                            {
                                isKeyExist = true;
                                break;

                            }
                            else
                            {
                                isKeyExist = false;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            listSearchKey = new List<SearchKey>();
                            listSearchKey.Add(new SearchKey() { search_key = asbSearch.Text });
                            WriteAndReadFile.SaveListSearchKey(listSearchKey);
                        }
                        catch { }
                    }

                    if(isKeyExist == false)
                    {
                        listSearchKey.Add(new SearchKey() { search_key = asbSearch.Text });
                        WriteAndReadFile.SaveListSearchKey(listSearchKey);
                    }
                    sender.ItemsSource = listSearchKey;
                }
                else
                {
                    listSearchKey.Add(new SearchKey() { search_key = asbSearch.Text });
                    WriteAndReadFile.SaveListSearchKey(listSearchKey);
                }

                var mainPage = (MainPage)(Window.Current.Content as Frame).Content;

                mainPage.AppFrame.Navigate(typeof(Pages.Search), asbSearch.Text);
            }
        }

        private async void asbSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (asbSearch.Text != null)
            {
                bool isFileExist = await WriteAndReadFile.CheckFileExist("ListSearchKey");

                if (isFileExist == true)
                {
                    string listKey = await WriteAndReadFile.ReadFile("ListSearchKey");
                    listSearchKey = JsonConvert.DeserializeObject<List<SearchKey>>(listKey);

                    sender.ItemsSource = listSearchKey;
                    
                }
                //else
                //{
                //    listSearchKey.Add(new SearchKey(asbSearch.Text));
                //    WriteAndReadFile.SaveListSearchKey(listSearchKey);
                //}

            }
        }
    }
}
