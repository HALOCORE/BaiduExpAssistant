using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace 百度经验个人助手
{
    public sealed partial class CookieSelectDialog : ContentDialog
    {
        private ObservableCollection<string> _cookies;
        public string selectedCookie;

        public CookieSelectDialog(IReadOnlyList<string> cookies)
        {
            this.InitializeComponent();
            _cookies = new ObservableCollection<string>();
            foreach (string cookie in cookies)
            {
                _cookies.Add(cookie);
            }
            listViewAllCookies.ItemsSource = _cookies;
            selectedCookie = null;
            
        }
        private void listViewAllCookies_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("ItemClick:" + e.ClickedItem);
            selectedCookie = (string)e.ClickedItem;
            this.Hide();
        }
    }
}
