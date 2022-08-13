﻿using System;
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

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace BD经验个人助手
{
    public sealed partial class ContentHelpDialog : ContentDialog
    {
        public ContentHelpDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void PopUpImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PopUpImage.Visibility = Visibility.Collapsed;
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Uri uri = XamlAnimatedGif.AnimationBehavior.GetSourceUri((Image) sender);

            XamlAnimatedGif.AnimationBehavior.SetSourceUri(PopUpImage, uri);

            PopUpImage.Visibility = Visibility.Visible;
        }
    }
}
