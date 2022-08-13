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

namespace BD经验个人助手
{
    public sealed partial class ContentSetCookieDialog : ContentDialog
    {
        public string userInputCookie;
        public ContentSetCookieDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            userInputCookie = way1Text.Text;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (way2bdussText.Text == "")
            {
                userInputCookie = "";
                return;
            }
            string s1;
            string input = way2bdussText.Text.Trim();
            if (input.StartsWith("BDUSS")) s1 = input;
            else s1 = "BDUSS=" + input;
            s1 = s1.Contains(';') ? s1 : s1 + ';';
            userInputCookie = s1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/article/fedf07379bd69c35ac897795.html"));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/article/9989c746e211eaf649ecfe47.html"));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/article/e4511cf36d03282b845eaf81.html"));
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/article/456c463b07f3910a583144a1.html"));
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/article/fdbd4277bfcd16b89f3f4844.html"));
        }
    }
    
}
