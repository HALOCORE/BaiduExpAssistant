using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace 百度经验个人助手
{
    public sealed partial class ContentTipsDialog : ContentDialog
    {
        public ContentTipsDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void buttonTip1_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/article/0f5fb099234ae06d8334ea86.html"));
        }

        private void buttonTip2_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/article/0964eca25ae2608284f53661.html"));
        }

        private void buttonTip3_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/article/b907e62770aa7846e7891c03.html"));
        }

        private void buttonTip4_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/article/0964eca25ae2608284f53661.html"));
        }

        private void buttonTip5_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/article/eae078276866771fed548563.html"));
        }

        private void buttonTip6_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/article/f96699bb08d56f894e3c1b85.html"));
        }

        private void buttonTip7_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/article/a3761b2be1d1951577f9aa61.html"));
        }

        private void buttonTip8_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/article/5d6edee2cf952599eadeec9d.html"));
        }
    }
}
