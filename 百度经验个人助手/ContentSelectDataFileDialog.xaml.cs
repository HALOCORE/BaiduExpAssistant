using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Ailon.QuickCharts;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace 百度经验个人助手
{
    public sealed partial class ContentSelectDataFileDialog : ContentDialog
    {

        private ObservableCollection<StorageFile> _dataFiles;
        
        public bool isSingleFile = true;
        public ObservableCollection<StorageFile> selectedFiles;
        public StorageFile selectedFile;

        public ContentSelectDataFileDialog(IReadOnlyList<StorageFile> dataFiles, bool isSingleFile = true)
        {
            this.InitializeComponent();
            this.isSingleFile = isSingleFile;

            if (isSingleFile)
            {
                this.Title = "选择一个用于作差的历史数据文件";
            }
            else
            {
                this.Title = "选择任意个数历史数据文件";
            }

            _dataFiles = new ObservableCollection<StorageFile>();
            foreach (StorageFile sf in dataFiles)
            {
                _dataFiles.Add(sf);
            }

            if (this.isSingleFile)
            {
                listViewAllFilesSingle.ItemsSource = _dataFiles;
                selectedFile = null;
            }
            else
            {
                listViewAllFiles.ItemsSource = _dataFiles;
                selectedFiles = new ObservableCollection<StorageFile>();
                listViewSelectedFiles.ItemsSource = selectedFiles;
            }
        }

        
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void listViewAllFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isSingleFile)
            {
                if (listViewAllFiles.SelectedItem != null)
                {
                    StorageFile sf = listViewAllFiles.SelectedItem as StorageFile;
                    if (!selectedFiles.Contains(sf))
                        selectedFiles.Add(sf);
                }
            }
            else
            {
                if (listViewAllFilesSingle.SelectedItem != null)
                {
                    StorageFile sf = listViewAllFilesSingle.SelectedItem as StorageFile;
                    selectedFile = sf;
                    textCurrentSelect.Text = "当前选择：" + sf.Name;
                }
            }
            
                
        }

        private void buttonRemoveThis_Click(object sender, RoutedEventArgs e)
        {
            selectedFiles.Remove(((Button) sender).DataContext as StorageFile);
        }
    }
}
