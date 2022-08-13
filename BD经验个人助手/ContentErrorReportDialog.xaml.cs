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

namespace BD经验个人助手
{
    public sealed partial class ContentErrorReportDialog : ContentDialog
    {
        public ContentErrorReportDialog(string name, string report)
        {
            this.InitializeComponent();
            errorName = name;
            errorReportContent = report;
            textErrorName.Text = errorName;
            textBoxErrorReportPreview.Text = report;
        }

        public string errorName = "";
        public string errorReportContent = "";
        public string errorNote = "";

        public string FullContent
        {
            get
            {
                return errorReportContent + "\n\n=====NOTE=====\n" + errorNote + "\n";
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            errorNote = textBoxErrorNote.Text;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            errorNote = textBoxErrorNote.Text;
        }
    }
}
