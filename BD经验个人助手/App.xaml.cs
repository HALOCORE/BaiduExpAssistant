﻿using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BD经验个人助手
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.EnteredBackground += EnteredBackgroundHandler;
            this.UnhandledException += async (sender, e) =>
            {
                if (e.Exception.Message.StartsWith("ERROR-REPORT-FAILED"))
                {
                    return;
                }

                if (e.Exception.Message.StartsWith("ERROR-LET-IT-DOWN"))
                {
                    return;
                }

                e.Handled = true;
                //REPORT
                string relvar = "senderType=" + sender.GetType() + "\nsender=" + sender.ToString();
                await Utility.FireErrorReport("未知错误", relvar, e.Exception, e.Message);
            };

            ApplicationView.PreferredLaunchViewSize = new Size(1200, 750);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
        }

        public static MainPage currentMainPage;

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //ShowWindowsNotice("注意，BD经验个人助手已被挂起", "内存数据将被清除");
            deferral.Complete();
        }

        private void ShowWindowsNotice(string header, string detail)
        {
            var t = ToastTemplateType.ToastText02;
            var content = ToastNotificationManager.GetTemplateContent(t);

            //Windows.Data.Xml.Dom.XmlNodeList
            XmlNodeList xml = content.GetElementsByTagName("text"); 
            xml[0].AppendChild(content.CreateTextNode(header));
            xml[1].AppendChild(content.CreateTextNode(detail));

            ToastNotification toast = new ToastNotification(content);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private void EnteredBackgroundHandler(object sender, EnteredBackgroundEventArgs e)
        { 
            if (App.currentMainPage.isAssistEditorEditing)
            {
                Utility.LogEvent("Editing_EnterBackground");

                ShowWindowsNotice(
                    "辅助编辑器进入后台，保存草稿提醒", 
                    "Windows会根据电量/内存情况清理后台。建议在最小化之前保存草稿。");
            }
            else if (App.currentMainPage.isAssistEditorActivated)
            {
                //ShowWindowsNotice("BD经验个人助手已被暂停", "BD经验个人助手被移入后台，程序暂停运行，暂停超过时长会被清理。");
            }
        }

    }
}
