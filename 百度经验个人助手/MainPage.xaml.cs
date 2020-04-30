using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Data.Xml.Dom;
using Windows.Networking;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using Windows.Graphics.Imaging;
using System.Diagnostics;
//using JiebaNet.Segmenter;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace 百度经验个人助手
{

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public bool isAssistEditorActivated { get; private set; } = false;
        public bool isAssistEditorEditing { get; set; } = false;

        public bool isCheckedAutoComplete { get { return checkboxAutoComplete.IsChecked == true; } }
        public bool isCheckedBigPic { get { return checkboxBigPic.IsChecked == true; } }
        public bool isCheckedBasicCheck { get { return checkboxBasicCheck.IsChecked == true; } }
        public bool isCheckedPicInsert { get { return checkboxPicInsert.IsChecked == true; } }
        public bool isCheckedBriefPic { get { return checkboxBriefPic.IsChecked == true; } }

        public MainPage()
        {
            this.InitializeComponent();
            App.currentMainPage = this;
            ExpManager.Init();
            StatManager.Init();


            listViewSearchExps.ItemsSource = ExpManager.rewardExps;

            VisualStateManager.GoToState(buttonSetCookie, "stateNormal", false);
            ShowControls();//first disable something

            SelfCheckAndInit();

        }

        public bool SecondWebViewVisibility{
            get { return borderWebViewSecondary.Visibility == Visibility.Visible; }
            set
            {
                if (value) borderWebViewSecondary.Visibility = Visibility.Visible;
                else borderWebViewSecondary.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowLoading(string loadmsg)
        {
            GridLoading.Visibility = Visibility.Visible;
            ProgressRingLoading.IsActive = true;
            TextBlockLoading.Text = loadmsg;
        }

        public void HideLoading()
        {
            ProgressRingLoading.IsActive = false;
            GridLoading.Visibility = Visibility.Collapsed;
        }

        public void ShowNotify(string title, string message, Symbol symbol = Symbol.Accept)
        {
            SymbolIconNotify.Symbol = symbol;
            TextBlockNotifyMain.Text = title;
            TextBlockNotifyMessage.Text = message;
            StoryBoardNotify.Begin();
        }

        //更新磁贴的方法
        private void UpdateTile()
        {
            //通过这个方法，我们就可以为动态磁贴的添加做基础。
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();

            //这里设置的是所以磁贴都可以为动态
            updater.EnableNotificationQueue(true);
            updater.Clear();

            //然后这里是重点：记得分3步走：
            //foreach (var item in feed.Items)
            //{
            //1：创建xml对象，这里看你想显示几种动态磁贴，如果想显示正方形和长方形的，那就分别设置一个动态磁贴类型即可。
            //下面这两个分别是矩形的动态磁贴，和方形的动态磁贴，具体样式，自己可以去微软官网查一查。我这里用到的是换行的文字形式。
            //XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text03);
            XmlDocument tileXml2 = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Text01);

            
            //2.接着给这个xml对象赋值
            XmlNodeList tileTextAttributes = tileXml2.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = "百度经验个人助手";
            tileTextAttributes[1].InnerText = textVisitAll.Text;
            tileTextAttributes[2].InnerText = textVisitRecent20.Text;
            tileTextAttributes[3].InnerText = textVoteAll.Text;
            tileTextAttributes[4].InnerText = textCollectAll.Text;

            //3.然后用Update方法来更新这个磁贴
            updater.Update(new TileNotification(tileXml2));

            //4.最后这里需要注意的是微软规定动态磁贴的队列数目小于5个，所以这里做出判断。
            //}
        }

        /// <summary>
        /// 尝试从存储读取Cookie，尝试更新Main。
        /// </summary>
        public async void SelfCheckAndInit()
        {
            textVersion.Text = "v " + StorageManager.VER;
            gridMain.Visibility = Visibility.Visible;
            gridSecond.Visibility = Visibility.Collapsed;
            SecondWebViewVisibility = false;

            JSCodeString.SetWebView(webViewMain, webViewSecondary);
            WebSetUpdate(true, false, false, false);

            ShowLoading("读取设置...");
            bool isSettingsRead = await StorageManager.ReadSettings();
            if (StorageManager.appSettings.isFirstIn || StorageManager.appSettings.version != StorageManager.FUNC_VER)
            {
                ContentNewDialog cnd = new ContentNewDialog();
                ContentDialogResult cdr2 = await cnd.ShowAsync();
                if (cdr2 == ContentDialogResult.Secondary)
                {
                    //bool confirm = await Utility.ShowConfirmDialog("确认：已知晓该版本更改了数据位置", "数据分析不能分析之前的数据。", "已知晓", "再看一下说明");
                    //if (!confirm)
                    //{
                    //    ContentNewDialog cnd3 = new ContentNewDialog();
                    //    ContentDialogResult cdr3 = await cnd.ShowAsync();
                    //}
                    StorageManager.appSettings.isFirstIn = false;
                    StorageManager.appSettings.version = StorageManager.FUNC_VER;
                }
                ShowLoading("更新设置...");
                
                await StorageManager.SaveSettings();
                
            }
            
            if (!isSettingsRead)
                await StorageManager.SaveSettings();

            ShowLoading("读取Edit设置...");
            await StorageManager.ReadEditSettings();
            checkboxAutoComplete.IsChecked = StorageManager.editSettings.ifLoadAutoComplete;

            ShowLoading("读取DIY功能设置...");
            await StorageManager.ReadDIYToolsSettings();

            Uri[] uris =
            {
                new Uri("ms-appx:///Assets/8-5-4.jpg") ,
                new Uri("ms-appx:///Assets/8-5-5.jpg") ,
                new Uri("ms-appx:///Assets/8-5-6.jpg") ,
                new Uri("ms-appx:///Assets/8-5-1.jpg") ,
                new Uri("ms-appx:///Assets/8-5-2.jpg") ,
                new Uri("ms-appx:///Assets/8-5-3.jpg")
            };

            Random r = new Random();
            int i = r.Next() % 6;

            ShowLoading("加载背景...");
            try
            {
                StorageFile file = 
                    await StorageFile.GetFileFromApplicationUriAsync(uris[i]);

                IRandomAccessStream stream = 
                    await file.OpenReadAsync(); //打开读取流


                BitmapImage img = new BitmapImage();
                await img.SetSourceAsync(stream);   //从流产生BitmapImage

                ImageBrush ib = new ImageBrush();   //新建ImageBrush画刷
                ib.ImageSource = img;
                ib.Stretch = Stretch.UniformToFill; //设置拉伸样式


                this.Background = ib;

                //如上设置刚才创建的ImageBrush
                //Background是一个Brush类型属性


                stream.Dispose();
            }
            catch (Exception e)
            {
                await Utility.ShowMessageDialog("非关键异常","自动背景图更换失败，请联系开发者\n" + e.Message);
                Utility.LogEvent("InitWarn_BackgroundFailed");
            }


            
            isSelfChecked = true;
            ShowControls(); 
            HelpStoryboard.Begin();
            OpenFolderStoryboard.Begin();
            DataAnalysisHintStoryboard.Begin();

            ShowLoading("读取Cookie..."); //TODO: 这里有一个cookie文件损坏就一直读取的bug. 修一下.
            string cookieGet = await StorageManager.GetCookieTry();
            if (cookieGet != null)
            {
                if (!ExpManager.SetCookie(cookieGet))
                {
                    await Utility.ShowMessageDialog("请重新设置Cookie", "Cookie文件内容不正确");
                    HideLoading();
                    return;
                }
                ShowLoading("尝试更新个人信息...");
                bool getSucceed = await UpdateMain();
                if (!getSucceed)
                {
                    Utility.LogEvent("InitFailed_InvalidBDUSS");                    
                }
                HideLoading();
                if (Window.Current.Bounds.Width< 1270)
                {
                    ShowNotify("提醒: 您的窗口过窄", "当前宽度为 " + Window.Current.Bounds.Width + ", 建议的最低宽度为 1366", Symbol.Comment);
                    Utility.LogEvent("Notify_WindowTooNarrow");
                }
            }
            else
            {
                HideLoading();
                buttonSetCookie_Click(null, null);
            }


        }


        #region 显示状态更新

        public bool isCookieValid = false;
        public bool isDataAvailable = false;
        public bool isCacheReward = false;
        public bool isSelfChecked = false;
        public bool isThisTimeUpdated = false;
        public bool isThisTimeAnalysed = false;

        /// <summary>
        /// 根据Cookie更新，Data更新，Cache缓存，Init自检更新控件状态。
        /// </summary>
        public void ShowControls()
        {
            if (!isSelfChecked) //selfcheck is special. use wait ring.
            {
                buttonSetCookieText.Text = "请稍等";

                buttonSetCookie.IsEnabled = false;
                buttonSetCookieProgress.IsActive = true;
                buttonSetCookieProgress.Visibility = Visibility.Visible;
            }
            else
            {
                buttonSetCookie.IsEnabled = true;
                buttonSetCookieProgress.IsActive = false;
                buttonSetCookieProgress.Visibility = Visibility.Collapsed;
            }

            if (isCookieValid)
            {
                buttonSetCookieText.Text = "√ Cookie";
                buttonUpdateExp.IsEnabled = true;
            }

            else
            {
                buttonSetCookieText.Text = "设置Cookie";
                buttonUpdateExp.IsEnabled = false;
            }

            if (isDataAvailable)
            {
                buttonSearch.IsEnabled = true;
                if(!isThisTimeAnalysed)
                    buttonStatistic.IsEnabled = true;
                else
                    buttonStatistic.IsEnabled = false;

                if (isThisTimeUpdated)
                    textContentSearchState.Text = "√ 来自更新信息：已更新";
                else
                    textContentSearchState.Text = "! 来自上次数据";
                areaSearchSet.Visibility = Visibility.Visible;
            }
            else
            {
                buttonSearch.IsEnabled = false;
                buttonStatistic.IsEnabled = false;
                areaSearchSet.Visibility = Visibility.Collapsed;
            }

            if (isCacheReward)
                buttonSearchReward.IsEnabled = true;
            else
                buttonSearchReward.IsEnabled = false;


        }

        /// <summary>
        /// 显示TextBlock，关于Content的统计信息
        /// </summary>
        private void ShowContentInf()
        {
            textVisitAll.Text = "总浏览：" + ExpManager.currentDataPack.contentExpsViewSum;
            textVoteAll.Text = "总投票：" + ExpManager.currentDataPack.contentExpsVoteSum;
            textCollectAll.Text = "总收藏：" + ExpManager.currentDataPack.contentExpsCollectSum;
            textVisitAllIncrease.Text = "总浏览增量: (点击数据分析)";
            //textVisitRecent20.Text = "近20篇浏览：" + ExpManager.currentDataPack.contentExpsView20;
        }

        /// <summary>
        /// 显示TextBlock和Image，关于最新用户状态信息。
        /// </summary>
        private void ShowNewMainInf()
        {
            textUserName.Text = ExpManager.NewMainUserNameDecoded;
            textHX.Text = "回享度：" + ExpManager.newMainIndexHuiXiang;
            textYZ.Text = "优质度：" + ExpManager.newMainIndexYiuZhi;
            textHD.Text = "互动度：" + ExpManager.newMainIndexHuDong;
            textHY.Text = "活跃度：" + ExpManager.newMainIndexHuoYue;
            textYC.Text = "原创度：" + ExpManager.newMainIndexYuanChuang;
            textExpCount.Text = "经验数：" + ExpManager.newMainExpCount;
            if (ExpManager.newMainPortrait != null)
                imageProfile.Source = ExpManager.newMainPortrait;
        }

        #endregion


        #region ##### 第A1步：主信息更新 ##### ----------------------------------

        private async Task<bool> UpdateMainSubStep_InitDataPacks()
        {
            
            int initStat = await StorageManager.InitUserFolder(ExpManager.newMainUserName); //Get user. Init StorageManager
            //if (initStat == -1)
            //{
            //    await Utility.ShowMessageDialog("异常", "更新旧版本数据出现异常。已忽略。\n新数据文件存储至用户名文件夹下，旧数据可自行删除。");
            //    await Launcher.LaunchFolderAsync(StorageManager.StorageFolder);
            //}
            //else if (initStat > 0)
            //{
            //    await Utility.ShowMessageDialog("数据自动更新", string.Format("自动更新旧版本数据文件 {0} 个\n新数据文件存储至用户名文件夹下，旧数据可自行删除。", initStat));
            //    await Launcher.LaunchFolderAsync(StorageManager.StorageFolder);
            //}
            DataPack latestDataPack = await StorageManager.ReadRecentDataPack();
            if (latestDataPack != null)
            {
                ExpManager.currentDataPack = latestDataPack;
                listViewContentExps.ItemsSource = ExpManager.currentDataPack.contentExps;
                string when = StorageManager.GetDataPackDescribe(latestDataPack);
                textDate.Text = "⚠ 来自上次：" + when;
                textDate.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 155, 0));


                isDataAvailable = true;
                ShowContentInf();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 根据以设置的Cookie，更新用户状态，读取以往数据
        /// </summary>
        /// <returns>Cookie是否成功使用</returns>
        public async Task<bool> UpdateMain()
        {
            ShowLoading("联网获取个人主页...\n" + ExpManager.CurrentCookieDisplayValue);
            if (!await ExpManager.GetMain()) return false;
            ShowLoading("储存完整Cookie...\n");
            await ExpManager.SaveCurrentCookie();

            ShowLoading("读取个人上次数据...");
            await UpdateMainSubStep_InitDataPacks();
            HideLoading();
            ShowNewMainInf();
            isCookieValid = true;
            ShowControls(); //这会点亮更新按钮
            
            return true;
        }

        /// <summary>
        /// 设置按钮状态，弹窗请求输入Cookie，设置Cookie，尝试更新Main。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void buttonSetCookie_Click(object sender, RoutedEventArgs e)
        {
            if(isAssistEditorActivated)
            {
                await Utility.ShowMessageDialog("请重启应用再修改Cookie", "本次启动使用了辅助编辑功能，造成Cookie锁定，无法修改。\n请重新启动应用程序再修改。");
                return;
            }

            buttonSetCookie.IsEnabled = false;
            buttonSetCookieProgress.IsActive = true;
            buttonSetCookieProgress.Visibility = Visibility.Visible;

            ShowLoading("Cookie设置流程中...");
            ContentWelcomeDialog cwd = new ContentWelcomeDialog();
            ContentDialogResult resultCwd = await cwd.ShowAsync();

            if (resultCwd == ContentDialogResult.Primary)
            {
                ContentTeachDialog ctd = new ContentTeachDialog();
                await ctd.ShowAsync();

                ContentHelpDialog chd = new ContentHelpDialog();
                await chd.ShowAsync();
            }
            

            ContentSetCookieDialog scd = new ContentSetCookieDialog();
            ContentDialogResult cdr = await scd.ShowAsync();
            HideLoading();
            if (cdr == ContentDialogResult.None)
            {
                ShowNotify("设置取消", "Cookie设置取消，保持原状。", Symbol.Comment);
                buttonSetCookie.IsEnabled = true;
                buttonSetCookieProgress.IsActive = false;
                buttonSetCookieProgress.Visibility = Visibility.Collapsed;
                
                return;
            }
            if (scd.userInputCookie.Trim() == "")
            {
                ShowNotify("设置无效", "Cookie输入为空，保持原状。", Symbol.Cancel);
                buttonSetCookie.IsEnabled = true;
                buttonSetCookieProgress.IsActive = false;
                buttonSetCookieProgress.Visibility = Visibility.Collapsed;
                return;
            }

            bool isCookieOK = ExpManager.SetCookie(scd.userInputCookie);
            if (!isCookieOK)
            {
                await Utility.ShowMessageDialog("Cookie添加", ExpManager.setcookieFailedInfo);
                buttonSetCookie.IsEnabled = true;
                buttonSetCookieProgress.IsActive = false;
                buttonSetCookieProgress.Visibility = Visibility.Collapsed;
                return;
            }
            VisualStateManager.GoToState(buttonSetCookie, "stateWaiting", false);
            bool isValid = await UpdateMain();
            VisualStateManager.GoToState(buttonSetCookie, "stateNormal", false);

            buttonSetCookie.IsEnabled = true;
            buttonSetCookieProgress.IsActive = false;
            buttonSetCookieProgress.Visibility = Visibility.Collapsed;
            if (isValid)
            {
                await ExpManager.SaveCurrentCookie();
                //await StorageManager.SaveCookie(ExpManager.cookie);
                buttonSetCookieText.Text = "√ Cookie";
                await Utility.ShowMessageDialog("设置完成", "Cookie有效，可以更新信息了。\n点击头像进入辅助编辑器。");
                Utility.LogEvent("YES_SetCookieSucceed");

                ContentTipsDialog scd3 = new ContentTipsDialog();
                await scd3.ShowAsync();
            }
            else
            {
                await Utility.ShowMessageDialog("验证Cookie", "Cookie无效，请重新设置");
                Utility.LogEvent("NO_SetCookieFailed");
            }

            HideLoading();
        }
        
        #endregion ------------------------------------------------------------

        #region ##### 第A2步：Content更新 ##### ----------------------------------
        /// <summary>
        /// 在Main已经成功更新后，更新Content
        /// </summary>
        /// <returns>更新Content是否成功</returns>
        public async Task UpdateContents()
        {
            if (Window.Current.Bounds.Width< 1270)
            {
                ShowNotify("提醒: 您的窗口过窄", "当前宽度为 " + Window.Current.Bounds.Width + ", 建议的最低宽度为 1366", Symbol.Comment);
                Utility.LogEvent("Notify_WindowTooNarrow");
            }

            await ExpManager.GetContents(
                textVisitAll, 
                listViewContentExps);


            ShowContentInf();
            isDataAvailable = true;
            isThisTimeUpdated = true;
            ShowControls();

        }

        /// <summary>
        /// 设置按钮状态，更新Content，如果成功就保存数据文件。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void buttonUpdateExp_Click(object sender, RoutedEventArgs e)
        {
            buttonUpdateExp.IsEnabled = false;
            buttonUpdateExpProgress.IsActive = true;
            buttonUpdateExpProgress.Visibility = Visibility.Visible;

            textVisitAll.Text = "更新主页...";
            await ExpManager.GetMain();
            ShowNewMainInf();
            await UpdateContents();
            await StorageManager.SaveDataPack(ExpManager.currentDataPack);

            UpdateTile();

            //Utility.ShowMessageDialog("更新完成", "点击 \"✅ 本次已更新\" 打开数据所在文件夹：\n" + StorageManager.StorageFolder.Path);
            ShowNotify("更新完成", "点击数据分析以计算增量");
            Utility.LogEvent("YES_UpdateExpSucceed");

            textDate.Text = "✅ 本次已更新";
            textDate.Foreground = new SolidColorBrush(Color.FromArgb(255,120,230,120));


            buttonUpdateExp.IsEnabled = true;
            buttonUpdateExpProgress.IsActive = false;
            buttonUpdateExpProgress.Visibility = Visibility.Collapsed;
        }

        #endregion ------------------------------------------------------------

        #region ##### 第B1步：悬赏更新 ##### ----------------------------------

        public async Task UpdateReward(int pglow, int pghigh, string tp, int cid)
        {
            if (Window.Current.Bounds.Width< 1270)
            {
                ShowNotify("提醒: 您的窗口过窄", "当前宽度为 " + Window.Current.Bounds.Width + ", 建议的最低宽度为 1366", Symbol.Comment);
                Utility.LogEvent("Notify_WindowTooNarrow");
            }


            ExpManager.rewardExps.Clear();
            ExpManager.rewardExpIDs.Clear();
            for (int i = pglow; i <= pghigh; )
            {
                Task<bool> Task0 = ExpManager.CookielessGetReward(tp, cid, i);
                string cacheStateText = "正在获取页码: " + i;
                i++;
                Task<bool> Task1 = null;
                Task<bool> Task2 = null;
                if (i <= pghigh)
                {
                    Task1 = ExpManager.CookielessGetReward(tp, cid, i);
                    cacheStateText += ", " + i;
                    i++;
                }
                if (i <= pghigh)
                {
                    Task2 = ExpManager.CookielessGetReward(tp, cid, i);
                    cacheStateText += ", " + i;
                    i++;
                }
                textRewardCacheState.Text = cacheStateText;

                bool shouldBreak = false;
                if (!(await Task0))
                {
                    shouldBreak = true;
                }
                if (Task1 != null && !(await Task1))
                {
                    shouldBreak = true;
                }
                if (Task2 != null && !(await Task2))
                {
                    shouldBreak = true;
                }
                if (shouldBreak) break;
            }
            isCacheReward = true;
            ShowNotify("悬赏获取完成", "共获取 " + ExpManager.rewardExps.Count + " 条.");
            if(ExpManager.rewardExps.Count == 0)
            {
                bool isNotBug = await Utility.ShowConfirmDialog("获取了0条悬赏，是确实没有还是程序出错？", "", "忽略此问题", "我认为程序有错");

                if (!isNotBug)
                {
                    //REPORT
                    await Utility.FireErrorReport("悬赏获取0条", "[reward]");
                }
            }
            ShowControls();
        }

        private async void buttonCacheReward_Click(object sender, RoutedEventArgs e)
        {
            

            string s = textCacheRewardFromTo.Text;
            string[] ss = s.Split('-');
            int low = 0, high = 0;
            if (ss.Length > 1)
            {
                Int32.TryParse(ss[0], out low);
                Int32.TryParse(ss[1], out high);
            }
            else
            {
                Int32.TryParse(ss[0], out high);
            }
            if (low < 0) low = 0;
            if (high - low > 1000)
            {
                await Utility.ShowMessageDialog("Warning", "页数过多，需要输入很多次验证码. 请降低页数");
                return;
            }

            helpSearchHolder.Visibility = Visibility.Collapsed;

            buttonCacheReward.IsEnabled = false;
            buttonCacheRewardProgress.IsActive = true;
            buttonCacheRewardProgress.Visibility = Visibility.Visible;
            buttonCacheRewardText.Visibility = Visibility.Collapsed;
            comboCacheReward.IsEnabled = false;
            comboCacheRewardType.IsEnabled = false;
            textCacheRewardFromTo.IsEnabled = false;

            string tp = "highquality";
            string tpstr = ((TextBlock)comboCacheRewardType.SelectionBoxItem).Text;
            switch (tpstr)
            {
                case "优质": tp = "highquality"; break;
                case "普通": tp = "special"; break;
                default: tp = "highquality"; break;
            }

            int cid = 0;
            string cur = ((TextBlock)comboCacheReward.SelectionBoxItem).Text;
            switch (cur)
            {
                case "全部": cid = 0; break;
                case "数码": cid = 10; break;
                case "美食": cid = 1; break;
                case "爱好": cid = 37; break;
                case "生活": cid = 50; break;
                case "健康": cid = 73; break;
                case "运动": cid = 86; break;
                case "职场": cid = 93; break;
                case "情感": cid = 101; break;
                case "教育": cid = 108; break;
                case "时尚": cid = 123; break;
                default: cid = 0; break;
            }
            listViewSearchExps.ItemsSource = ExpManager.rewardExps;
            await UpdateReward(low, high, tp, cid);
            textRewardCacheState.Text = string.Format("√ 已获取 {0} 的 {1}~{2} 页", cur, low, high);
            Utility.LogEvent("YES_CacheRewardSucceed");

            buttonCacheReward.IsEnabled = true;
            buttonCacheRewardProgress.IsActive = false;
            buttonCacheRewardProgress.Visibility = Visibility.Collapsed;
            buttonCacheRewardText.Visibility = Visibility.Visible;
            comboCacheReward.IsEnabled = true;
            comboCacheRewardType.IsEnabled = true;
            textCacheRewardFromTo.IsEnabled = true;
        }

        #endregion ------------------------------------------------------------


        #region 各路搜索功能

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            helpSearchHolder.Visibility = Visibility.Collapsed;
            string order;
            switch (((TextBlock)comboSearchOrderType.SelectionBoxItem).Text)
            {
                case "最佳匹配":
                    order = "match";
                    break;
                case "最高浏览量":
                    order = "view";
                    break;
                case "最高浏览量增量":
                    order = "viewinc";
                    break;
                case "最高收藏量":
                    order = "collect";
                    break;
                case "最靠前":
                    order = "new";
                    break;
                default: order = "view"; break;
            }

            int count;
            switch (((TextBlock)comboSearchCount.SelectionBoxItem).Text)
            {
                case "50条":
                    count = 50;
                    break;
                case "100条":
                    count = 100;
                    break;
                case "全部":
                    count = 100000;
                    break;
                default: count = 100000; break;
            }

            ExpManager.SearchContentExps(textBoxSearch.Text, order, count);
            listViewSearchExps.ItemsSource = ExpManager.contentExpsSearched;
        }

        private void buttonSearchReward_Click(object sender, RoutedEventArgs e)
        {
            helpSearchHolder.Visibility = Visibility.Collapsed;
            string order;
            switch (((TextBlock)comboSearchRewardOrderType.SelectionBoxItem).Text)
            {
                case "最佳匹配":
                    order = "match";
                    break;
                case "最高赏金":
                    order = "money";
                    break;
                case "名称最短":
                    order = "short";
                    break;
                case "最靠前":
                    order = "new";
                    break;
                default: order = "match"; break;
            }

            int count;
            switch (((TextBlock)comboSearchRewardCount.SelectionBoxItem).Text)
            {
                case "20条":
                    count = 20;
                    break;
                case "50条":
                    count = 50;
                    break;
                case "全部":
                    count = 100000;
                    break;
                default: count = 100000; break;
            }

            ExpManager.SearchRewardExps(textBoxSearch.Text, order, count);
            listViewSearchExps.ItemsSource = ExpManager.rewardExpsSearched;
        }

        #endregion

        #region 列表数据模板更新

        private void ListViewContentExps_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (listViewContentExps.SelectedItem != null)
            {
                ListViewItem li =
                    (ListViewItem)listViewContentExps.ContainerFromItem(listViewContentExps.SelectedItem);
                if (li.ContentTemplate.Equals(this.Resources["contentExpItemTemplate"]))
                    li.ContentTemplate = this.Resources["contentExpItemSelectedTemplate"] as DataTemplate;
                else if (li.ContentTemplate.Equals(this.Resources["contentExpItemSelectedTemplate"]))
                    li.ContentTemplate = this.Resources["contentExpItemTemplate"] as DataTemplate;
            }

        }

        private void ListViewSearchExps_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (listViewSearchExps.SelectedItem != null)
            {
                ListViewItem li =
                    (ListViewItem)listViewSearchExps.ContainerFromItem(listViewSearchExps.SelectedItem);

                if (li.ContentTemplateSelector.Equals(this.Resources["ExpItemDataTemplateSelector"]))
                {
                    li.ContentTemplateSelector =
                        this.Resources["ExpItemDataSelectedTemplateSelector"] as DataTemplateSelector;
                    return;
                }
                if (li.ContentTemplateSelector.Equals(this.Resources["ExpItemDataSelectedTemplateSelector"]))
                {
                    li.ContentTemplateSelector =
                        this.Resources["ExpItemDataTemplateSelector"] as DataTemplateSelector;
                    return;
                }


            }
        }

        #endregion

        private void buttonStatistic_Click(object sender, RoutedEventArgs e)
        {
            //if(StorageManager.dataPacks.Count > 2)
            isThisTimeAnalysed = true;
            ShowControls();

            StatManager.CondenseExps(ExpManager.currentDataPack.contentExps);
            mainChart.DataSource = StatManager.CondensedExps;
            secondChart.Visibility = Visibility.Collapsed;
            mainChart.Visibility = Visibility.Visible;
            StackPanelStatCover.Visibility = Visibility.Collapsed;
            
            buttonStatistic_Click_Substep2();
        }

        private async void buttonStatistic_Click_Substep2()
        {
            ContentSelectDataFileDialog sdf = new ContentSelectDataFileDialog(await StorageManager.GetDataPackFiles());
            ContentDialogResult r = await sdf.ShowAsync();

            if (r == ContentDialogResult.Secondary || r == ContentDialogResult.None) return;

            if (sdf.selectedFile == null)
            {
                await Utility.ShowMessageDialog("无选择文件", "您当前未选择作差文件。无分析。");
                Utility.LogEvent("NO_StatisticNoSelected");
                return;
            }

            ShowLoading("读取文件...");
            try
            {
                StatManager.DataPackSingleSelected = await StorageManager.ReadHistoryDataPackSingle(sdf.selectedFile);
            }catch(Exception ee)
            {
                //REPORT
                string rel = "filename=" + sdf.selectedFile.Name;
                await Utility.FireErrorReport("数据分析读取历史文件失败", rel, ee);
                StatManager.DataPackSingleSelected = null;
            }
            HideLoading();

            ObservableCollection<ContentExpEntry> data = null;
            if (StatManager.DataPackSingleSelected != null)
            {
                if (StatManager.DataPackSingleSelected.date.Date == DateTime.Today.Date)
                {
                    await Utility.ShowMessageDialog("选择今天的数据可能使分析无效", "您选择的历史数据是今天的数据。可能得到全0的作差结果。");
                    Utility.LogEvent("NO_StatisticTodaySelected");
                }
                ShowLoading("计算中...");
                await StatManager.Calc(StatManager.DataPackSingleSelected.contentExps,
                    ExpManager.currentDataPack.contentExps);
                data = StatManager.DeltaExps;
                HideLoading();
                if (Window.Current.Bounds.Width< 1270)
                {
                    ShowNotify("提醒: 您的窗口过窄", "当前宽度为 " + Window.Current.Bounds.Width + ", 建议的最低宽度为 1366", Symbol.Comment);
                    Utility.LogEvent("Notify_WindowTooNarrow");
                }
            }
            if (data != null) //to check if calc is done.
            {
                secondChart.Visibility = Visibility.Visible;
                mainChart.Visibility = Visibility.Collapsed;

                StatManager.CondenseDeltaExps();
                secondChart.DataSource = StatManager.CondensedDeltaExps;

                listViewContentExps.ItemsSource = null;
                listViewContentExps.ItemsSource = ExpManager.currentDataPack.contentExps;
                textVisitAllIncrease.Text = "总浏览增量：" + (ExpManager.currentDataPack.contentExpsViewSum - StatManager.DataPackSingleSelected.contentExpsViewSum).ToString();

                StatManager.CalcDeltaExpsRecentAverage(50);
                textVisitRecent20.Text = "近50篇平均增量：" + StatManager.DeltaExpsRecentAverage.ToString("F2");
                try
                {
                    StatManager.CalcDeltaExpsOneYearIncrease();
                    textVisitOneYearIncrease.Text = "一年内经验增量：" + StatManager.DeltaExpsOneYearInc;
                    Utility.LogEvent("YES_StatisticCalcAllSucceed");
                }
                catch(Exception e)
                {
                    await Utility.ShowMessageDialog("计算一年内浏览增量失败", "可通知开发者 1223989563@qq.com");
                    Utility.LogEvent("NO_StatisticCalcOneYearFailed");
                }
                UpdateTile();
            }
            //buttonStatistic.IsEnabled = true;
        }


        private double _oldOpacity = 0;
        private void buttonCenter_Click(object sender, RoutedEventArgs e)
        {
            
            gridMain.Visibility = Visibility.Collapsed;
            gridSecond.Visibility = Visibility.Visible;
            _oldOpacity = Opacity;
            Opacity = 1;
        }

        private async void buttonOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchFolderAsync(StorageManager.CurrentUserFolder);
        }

        private void secondChart_Tapped(object sender, TappedRoutedEventArgs e)
        {
            secondChart.Visibility = Visibility.Collapsed;
            mainChart.Visibility = Visibility.Visible;
        }

        private void mainChart_Tapped(object sender, TappedRoutedEventArgs e)
        {
            secondChart.Visibility = Visibility.Visible;
            mainChart.Visibility = Visibility.Collapsed;
            
        }

        private async void buttonTips_Click(object sender, RoutedEventArgs e)
        {
            ContentTipsDialog scd = new ContentTipsDialog();
            ContentDialogResult cdr = await scd.ShowAsync();
        }

        private void buttonContentLink_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri(((ContentExpEntry)((Button)sender).DataContext).Url));
        }

        private void buttonRewardLink_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri(((RewardExpEntry)((Button)sender).DataContext).PageUrl));
        }

        private async void buttonRewardGet_Click(object sender, RoutedEventArgs e)
        {
            string queryId = ((RewardExpEntry)((Button)sender).DataContext).QueryId;
            ShowLoading("领取请求...");
            bool isEnterEdit = await ExpManager.CookiedGetReward(queryId);
            HideLoading();
            if (isEnterEdit)
            {
                Utility.LogEvent("YES_GetRewardEnterEdit");
                buttonCenter_Click(sender, e);
                // await Task.Delay(300);
                JSCodeString.planToGoUrl = "https://jingyan.baidu.com/edit/content?queryId=" + queryId;
                buttonMainPage_Click(sender, e);
            }
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard sb = ((StackPanel)sender).Resources["ButtonContentLinkStoryboard"] as Storyboard;
            sb.Begin();
        }


        private void StackPanel_Loaded_1(object sender, RoutedEventArgs e)
        {
            Storyboard sb = ((StackPanel)sender).Resources["ButtonRewardLinkStoryboard"] as Storyboard;
            sb.Begin();
        }


        #region Helper

        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            gridSecond.Visibility = Visibility.Collapsed;
            gridMain.Visibility = Visibility.Visible;
            Opacity = _oldOpacity;
        }

        private async void buttonMainPage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ExpManager.cookie))
            {
                Utility.ShowMessageDialog("请先设置Cookie", "设置Cookie完成后，可以保持登录进入编辑器。\n禁止在此暴露账号密码进行登陆。");
                return;
            }

            ShowLoading("设置数据读取...");
            await StorageManager.InitReadAllCommonData();

            ShowLoading("访问jingyan.baidu.com...");
            webViewMain.Navigate(new Uri("https://jingyan.baidu.com/"));
            App.currentMainPage.StackPanelWebViewCover.Visibility = Visibility.Collapsed;
            isAssistEditorActivated = true;

            //List<Uri> allowedUris = new List<Uri>();
            //allowedUris.Add(new Uri("https://jingyan.baidu.com/"));
            //allowedUris.Add(new Uri("https://jingyan.baidu.com/user/nuc/content?tab=exp&expType=draft"));
            //allowedUris.Add(new Uri("https://jingyan.baidu.com/edit/content"));
            //webViewMain.AllowedScriptNotifyUris = allowedUris;
        }

        private void buttonDraft_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading("访问草稿箱...");
            webViewMain.Navigate(new Uri("https://jingyan.baidu.com/user/nuc/content?tab=exp&expType=draft"));
        }

        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading("打开经验编辑器...");
            webViewMain.Navigate(new Uri("https://jingyan.baidu.com/edit/content"));
        }

        private async void buttonTestMine_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading("载入雷区检测...");
            Utility.LogEvent("OK_TestMineCalled");
            try
            {
                await JSCodeString.RunJss(webViewMain, new string[] {JSCodeString.JsPrependErrorReport});
                await Task.Delay(200);
                await JSCodeString.RunJss(webViewMain, new string[] {JSCodeString.ErrableUsingErrBoard(JSCodeString.JsMineDetect, "雷区检测载入...", "雷区检测已调用，请稍等. Thanks to 孢子真好玩") });
            }
            catch (Exception e2)
            {
                await Utility.ShowMessageDialog("雷区检测失败", "当前页面可能不是编辑器页面");
                //await Utility.ShowDetailedError("详细信息", e2);
            }
            HideLoading();
        }

        private void buttonTestBroswer_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/"));
        }

        public void WebSetUpdate(bool isMainPage, bool isDraft, bool isEdit, bool isTestMine)
        {
            buttonMainPage.IsEnabled = isMainPage;
            buttonDraft.IsEnabled = isDraft;
            buttonEdit.IsEnabled = isEdit;
            buttonTestMine.IsEnabled = isTestMine;
            buttonAutoFill.IsEnabled = isEdit;
            buttonBigPicture.IsEnabled = isEdit;
            checkboxAutoComplete.IsEnabled = isEdit;
            buttonDIY.IsEnabled = isEdit;
        }

        private async void buttonBigPicture_Click(object sender, RoutedEventArgs e)
        {
            //move to JSCodeString
        }


        #endregion

        private void buttonBigger_Click(object sender, RoutedEventArgs e)
        {
            //Cant do
        }

        private async void buttonEditSettings_Click(object sender, RoutedEventArgs e)
        {
            ContentEditSettingsDialog cd = new ContentEditSettingsDialog();
            ContentDialogResult re = await cd.ShowAsync();
            if (re == ContentDialogResult.Primary)
            {
                ShowLoading("保存Edit设置...");
                await StorageManager.SaveEditSettings();
                HideLoading();
            }
        }

        private async void buttonAutoFill_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading("自动填写...");
            Utility.LogEvent("OK_AutoFillCalled");
            try
            {
                await JSCodeString.RunJs(
                    webViewMain, 
                    JSCodeString.JsAutoFillTitle +
                    "AutoSetBrief(\"" + GetTransString(StorageManager.editSettings.strTitle2Brief) + "\");"); //.Replace("\\", "\\\\")

                await JSCodeString.RunJs(
                    webViewMain,
                    JSCodeString.JsAutoFillTools + 
                    "AutoSetTools(\"" + GetTransString(StorageManager.editSettings.strTitle2Tool) + "\");"); //Replace("\n", "\\n").Replace("\r","\\n")

                await JSCodeString.RunJs(
                    webViewMain,
                    JSCodeString.JsAutoFillNotice + 
                    "SetNoticeIndex(0, \"" + GetTransString(StorageManager.editSettings.StoreStrAttention) + "\");");

                await JSCodeString.RunJs2(
                    webViewMain,
                    JSCodeString.JsAutoSetCategory,
                    "AutoSetCategory(\"" + GetTransString(StorageManager.editSettings.strTitle2Category) + "\");"); //.Replace("\n", "\\n").Replace("\r", "\\n")

                if (StorageManager.editSettings.ifAddStep)
                {
                    await JSCodeString.RunJs2(
                        webViewMain,
                        JSCodeString.JsAutoAddStep,
                        "AddStep(" + StorageManager.editSettings.addStepCount.ToString() + ");");
                }

                if (StorageManager.editSettings.ifCheckOrigin)
                {
                    await JSCodeString.RunJs2(
                        webViewMain,
                        JSCodeString.JsAutoCheckOrigin,
                        "CheckOrigin(true)");
                }

                if (StorageManager.editSettings.ifSteps)
                {
                    await JSCodeString.RunJs2(
                        webViewMain,
                        JSCodeString.JsAutoFillSteps,
                        "SetSteps(\"" + GetTransString(StorageManager.editSettings.strSteps) + "\");"
                    );
                }

                    
                    
            }
            catch (Exception ee)
            {
                await Utility.ShowMessageDialog("自动填写出现问题", "当前页面可能不是编辑器页面，或者设置有问题（含有特殊字符，或者设置格式错误）");
            }
            HideLoading();
        }

        private async void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
        }

        public static string GetTransString(string input)
        {
            input = input.Replace("\\", "\\\\");

            input = input.Replace("\n", "\\n");
            input = input.Replace("\"", "\\\"");
            input = input.Replace("\'", "\\\'");
            input = input.Replace("\r", "\\r");
            input = input.Replace("\t", "\\t");
            return input;
        }

        public async Task LoadAutoCompleteAsync()
        {
            Utility.LogEvent("OK_LoadAutoCompleteCalled");
            try
            {
                await JSCodeString.AddScriptUri(webViewMain, "ms-appx-web:///Assets/code/AutoComplete.js");
                ShowLoading("加载自动补全...");
                string data = await StorageManager.ReadAutoCompleteData("");
                await Task.Delay(500);
                
                if (data == "")
                {
                    await JSCodeString.RunJs(webViewMain, "InitAutoComplete();");
                }
                else
                {
                    //希望执行js代码： InitAutoComplete("[...]")，
                    await JSCodeString.RunJs(webViewMain, 
                        "InitAutoComplete(\""
                        + GetTransString(data)
                        + "\")"
                        );
                }
            }
            catch (Exception ee)
            {
                await Utility.ShowMessageDialog("加载 [自动补全] 出现问题", "如果当前页面确实是编辑器页面，可将错误截图给开发者 (wang1223989563)\n" + ee.GetType().ToString() + '\n' +  ee.Message);
                Utility.LogEvent("ERROR_LoadAutoCompleteFailed");
                //await Utility.FireErrorReport("自动补全加载出错", "[edit]", ee);
            }
        }

        public async Task<WriteableBitmap> GetWebViewImageAsync(double ratio, int reWidth, int reHeight)
        {
            InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
            await webViewMain.CapturePreviewToStreamAsync(ms);

            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(ms);
            int width = (int)decoder.PixelWidth;
            int height = (int)decoder.PixelHeight;
            int cropWidth = (int)(width / 1.5) - 4;
            int cropHeight = (int)(width / ratio / 1.5) - 4;
            
            WriteableBitmap bitmap = new WriteableBitmap(width, height);
            bitmap.SetSource(ms);
            bitmap = bitmap.Crop(2, 2, cropWidth, cropHeight);
            bitmap = bitmap.Resize(reWidth, reHeight, WriteableBitmapExtensions.Interpolation.Bilinear);
           
            ms.Dispose();
            return bitmap;
        }

        private async void buttonDIY_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDIYDialog();
            var result = await dlg.ShowAsync();
        }

        public async Task RunDIYClickTool(DIYTool tool)
        {
            await JSCodeString.RunDIYToolCode(webViewMain, tool);
        }

        public async Task ToggleDIYNavigateTool(DIYTool tool)
        {
            tool.IsActivate = !tool.IsActivate;
            ShowNotify("Navigate Tool Toggle", tool.Name);
        }

        private async void TextBlock_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            //ConfigManager.ConfigFileBaseDir = "ms-appx:///Assets/jiebadict";
            //var segmenter = new JiebaSegmenter();
            //var segments = segmenter.Cut("我来到北京清华大学", cutAll: true);
            throw new Exception("异常捕获测试 Catch Exception Test");
        }

        private void buttonWebViewSecondaryClose_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SecondWebViewVisibility = false;
        }

        public async Task SaveDraft()
        {
            await JSCodeString.RunJs(webViewMain, JSCodeString.JsSaveDraft);
        }

        private async void CheckboxAnyFunc_CheckEvent(object sender, RoutedEventArgs e)
        {
            if (StorageManager.editSettings == null)
            {
                Debug.WriteLine("启动阶段Func Checkbox变化.");
                return;
            }
            bool isDirty = false;
            
            if (StorageManager.editSettings.ifLoadAutoComplete != isCheckedAutoComplete)
            {
                isDirty = true;
                StorageManager.editSettings.ifLoadAutoComplete = isCheckedAutoComplete;
            }
            
            if (StorageManager.editSettings.ifLoadBasicCheck != isCheckedBasicCheck)
            {
                isDirty = true;
                StorageManager.editSettings.ifLoadBasicCheck = isCheckedBasicCheck;
            }
            
            if (StorageManager.editSettings.ifLoadBigPic != isCheckedBigPic)
            {
                isDirty = true;
                StorageManager.editSettings.ifLoadBigPic = isCheckedBigPic;
            }
            
            if (StorageManager.editSettings.ifLoadBriefPic != isCheckedBriefPic)
            {
                isDirty = true;
                StorageManager.editSettings.ifLoadBriefPic = isCheckedBriefPic;
            }
            
            if (StorageManager.editSettings.ifLoadPicInsert != isCheckedPicInsert)
            {
                isDirty = true;
                StorageManager.editSettings.ifLoadPicInsert = isCheckedPicInsert;
            }
            if(isDirty) await StorageManager.SaveEditSettings();
        }

        private void WebViewSecondary_LoadCompleted(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
        }

        private async void ButtonOpenSource_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/HALOCORE/BaiduExpAssistant"));
        }
    }
}
