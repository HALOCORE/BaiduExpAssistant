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

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace 百度经验个人助手
{
    public sealed partial class ContentEditSettingsDialog : ContentDialog
    {
        public ContentEditSettingsDialog()
        {
            this.InitializeComponent();
            TextBoxTitle2Brief.Text = StorageManager.editSettings.strTitle2Brief;
            TextBoxTitle2Tool.Text = StorageManager.editSettings.strTitle2Tool;
            TextBoxAttention.Text = StorageManager.editSettings.strAttention;
            TextBoxTitle2Category.Text = StorageManager.editSettings.strTitle2Category;
            TextBoxSteps.Text = StorageManager.editSettings.strSteps;

            TextBoxAddStepCount.Text = StorageManager.editSettings.addStepCount.ToString();
            CheckBoxAddStep.IsChecked = StorageManager.editSettings.ifAddStep;
            CheckBoxCheckOrigin.IsChecked = StorageManager.editSettings.ifCheckOrigin;
            CheckBoxSteps.IsChecked = StorageManager.editSettings.ifSteps;

            CheckBoxSteps_Click(null, null);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            StorageManager.editSettings.strTitle2Brief = TextBoxTitle2Brief.Text;
            StorageManager.editSettings.strTitle2Tool = TextBoxTitle2Tool.Text;
            StorageManager.editSettings.strAttention = TextBoxAttention.Text;
            StorageManager.editSettings.strSteps = TextBoxSteps.Text;

            StorageManager.editSettings.strTitle2Category = TextBoxTitle2Category.Text;
            int step;
            if (!int.TryParse(TextBoxAddStepCount.Text, out step))
            {
                step = 0;
            }
            StorageManager.editSettings.addStepCount = step;
            try
            {
                StorageManager.editSettings.ifAddStep = (bool) CheckBoxAddStep.IsChecked;
                StorageManager.editSettings.ifCheckOrigin = (bool) CheckBoxCheckOrigin.IsChecked;
                StorageManager.editSettings.ifSteps = (bool)CheckBoxSteps.IsChecked;
            }
            catch (InvalidOperationException)
            {
                //不知道什么时候会出现这种
                //StorageManager.editSettings.ifAddStep = true;
                //StorageManager.editSettings.ifCheckOrigin = true;
            }


        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //Ignore
        }

        private void CheckBoxSteps_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxSteps.IsChecked != null && (bool) CheckBoxSteps.IsChecked)
            {
                GridSteps.Opacity = 1;
            }
            else
            {
                GridSteps.Opacity = 0.4;
            }
        }
    }
}
