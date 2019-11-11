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
            listViewDIYTools.ItemsSource = StorageManager.dIYToolsSettings.DIYTools;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            bool confirm = await Utility.ShowConfirmDialog("初始化确认", "初始化会清除添加的数据，确定要进行初始化?");
            if (!confirm)
            {
                App.currentMainPage.ShowNotify("操作取消", "未对自定义功能初始化", Symbol.Comment);
                return;
            }
            StorageManager.dIYToolsSettings.Init();
            bool isSucceed = await StorageManager.SaveDIYToolsSettings();
            if (isSucceed)
            {
                App.currentMainPage.ShowNotify("成功初始化", "自定义功能已回到初始状态", Symbol.Accept);
            }
            else
            {
                await Utility.ShowMessageDialog("初始化失败", "自定义功能未初始化.");
            }
            
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

        private async void buttonSaveDIYEntry_Click(object sender, RoutedEventArgs e)
        {
            if(textBoxDIYEntryName.Text == "")
            {
                await Utility.ShowMessageDialog("内容不完整", "自定义功能名称不能为空。");
                return;
            }

            if (textBoxDIYEntryCode.Text == "")
            {
                await Utility.ShowMessageDialog("内容不完整", "代码不能为空。");
                return;
            }

            if (radioDIYEntryUrlTrig.IsChecked == true && 
                !(textBoxDIYEntryUrl.Text.StartsWith("https://")))
            {
                await Utility.ShowMessageDialog("内容不完整", "要使用到达页面触发的方式，必须填写https开头的页面地址.");
                return;
            }

            bool isFound = false;
            foreach(var tool in StorageManager.dIYToolsSettings.DIYTools)
            {
                if(tool.Name == textBoxDIYEntryName.Text)
                {
                    tool.TargetUrl = textBoxDIYEntryUrl.Text;
                    tool.Note = textBoxDIYEntryNote.Text;
                    tool.Code = textBoxDIYEntryCode.Text;
                    tool.TrigType = radioDIYEntryClickTrig.IsChecked == true ? "click" : "navigate";
                    isFound = true;
                    break;
                } 
            }
            if (!isFound)
            {
                var tool = new DIYTool(
                    textBoxDIYEntryName.Text,
                    textBoxDIYEntryUrl.Text,
                    radioDIYEntryClickTrig.IsChecked == true ? "click" : "navigate",
                    textBoxDIYEntryNote.Text,
                    textBoxDIYEntryCode.Text);
                StorageManager.dIYToolsSettings.DIYTools.Add(tool);
            }
            bool isSucceed = await StorageManager.SaveDIYToolsSettings();
            if (!isSucceed)
            {
                await Utility.ShowMessageDialog("保存出错", "自定义工具未能保存到文件. 可联系开发者. 1223989563@qq.com");
            }
            else
            {
                await Utility.ShowMessageDialog("保存成功", "自定义工具已保存.");
                listViewDIYTools.ItemsSource = null;
                listViewDIYTools.ItemsSource = StorageManager.dIYToolsSettings.DIYTools;
            }
        }

        private void buttonTempEditDIYEntry_Click(object sender, RoutedEventArgs e)
        {
            var tool = (DIYTool)((Button)sender).DataContext;
            textBoxDIYEntryName.Text = tool.Name;
            textBoxDIYEntryNote.Text = tool.Note;
            textBoxDIYEntryUrl.Text = tool.TargetUrl;
            textBoxDIYEntryCode.Text = tool.Code;
            if (tool.IsClickTrig)
            {
                radioDIYEntryClickTrig.IsChecked = true;
            }
            else
            {
                radioDIYEntryUrlTrig.IsChecked = true;
            }
        }

        private async void buttonTempDeleteDIYEntry_Click(object sender, RoutedEventArgs e)
        {
            var tool = (DIYTool)((Button)sender).DataContext;
            bool isConfirmed = await Utility.ShowConfirmDialog("确认删除操作", "确定要删除该自定义功能条目? 名称为 " + tool.Name);
            if (isConfirmed)
            {
                StorageManager.dIYToolsSettings.DIYTools.Remove(tool);
                bool isSucceed = await StorageManager.SaveDIYToolsSettings();
                listViewDIYTools.ItemsSource = null;
                listViewDIYTools.ItemsSource = StorageManager.dIYToolsSettings.DIYTools;
                if (!isSucceed)
                {
                    await Utility.ShowMessageDialog("删除出错", "自定义工具删除操作没有保存到文件. 可联系开发者. 1223989563@qq.com");
                }
            }
        }
    }
}
