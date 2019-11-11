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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace 百度经验个人助手
{
    public sealed partial class ContentDIYDialog : ContentDialog
    {
        public ContentDIYDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void buttonClearDIYEntry_Click(object sender, RoutedEventArgs e)
        {
            textBoxDIYEntryName.Text = "";
            textBoxDIYEntryUrl.Text = "";
            textBoxDIYEntryNote.Text = "";
            textBoxDIYEntryCode.Text = "";
            radioDIYEntryClickTrig.IsChecked = true;
        }
    }
}
