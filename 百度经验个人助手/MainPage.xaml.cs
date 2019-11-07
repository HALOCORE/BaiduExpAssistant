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

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace 百度经验个人助手
{

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {


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
            gridMain.Visibility = Visibility.Visible;
            gridSecond.Visibility = Visibility.Collapsed;

            JSCodeString.SetWebView(webViewMain);
            WebSetUpdate(true, false, false, false);

            ShowLoading("读取设置...");
            bool isSettingsRead = await StorageManager.ReadSettings();
            if (StorageManager.AppSettings.isFirstIn || StorageManager.AppSettings.version != "1.4.6")
            {
                ContentNewDialog cnd = new ContentNewDialog();
                ContentDialogResult cdr2 = await cnd.ShowAsync();
                if (cdr2 == ContentDialogResult.Secondary)
                {
                    StorageManager.AppSettings.isFirstIn = false;
                    StorageManager.AppSettings.version = "1.4.6";
                }
                ShowLoading("更新设置...");
                
                await StorageManager.SaveSettings();
                
            }
            
            if (!isSettingsRead)
                await StorageManager.SaveSettings();

            ShowLoading("读取Edit设置...");
            await StorageManager.ReadEditSettings();

            if (Window.Current.Bounds.Width < 1100)
            {
                textUserName.Text = "您的窗口过窄";
            }
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
                await ShowMessageDialog("非关键异常","自动背景图更换失败，请联系开发者\n" + e.Message);
            }


            
            isSelfChecked = true;
            ShowControls(); 
            HelpStoryboard.Begin();
            OpenFolderStoryboard.Begin();

            ShowLoading("读取Cookie..."); //TODO: 这里有一个cookie文件损坏就一直读取的bug. 修一下.
            string cookieGet = await StorageManager.GetCookieTry();
            if (cookieGet != null)
            {
                if (!ExpManager.SetCookie(cookieGet)) return;
                ShowLoading("尝试更新个人信息...");
                bool getSucceed = await UpdateMain();
                if (!getSucceed)
                {
                    await ShowMessageDialog("主页获取不成功", "如果不是网络问题，那可能是BDUSS已经失效。（是否有退出登录操作？）");
                }
                HideLoading();
            }
            else
            {
                buttonSetCookie_Click(null, null);
            }


        }

        #region Utilities实用函数

        /// <summary>
        /// 检查err，如果为false弹窗。
        /// </summary>
        /// <param name="err"></param>
        /// <param name="errMsg"></param>
        public void _Error_Check(bool err, string errMsg)
        {
            if (err)
            {
                ShowMessageDialog("意外异常（请联系开发者）", errMsg);
            }
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">信息</param>
        private async Task ShowMessageDialog(string title, string message)
        {
            var msgDialog = new Windows.UI.Popups.MessageDialog(message) { Title = title };
            //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", uiCommand => { this.textUserName.Text = $"您点击了：{uiCommand.Label}"; }));
            //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("取消", uiCommand => { this.textUserName.Text = $"您点击了：{uiCommand.Label}"; }));
            await msgDialog.ShowAsync();
        }

        #endregion


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
            textVisitRecent20.Text = "近20篇浏览：" + ExpManager.currentDataPack.contentExpsView20;
        }

        /// <summary>
        /// 显示TextBlock和Image，关于最新用户状态信息。
        /// </summary>
        private void ShowNewMainInf()
        {
            textUserName.Text = ExpManager.newMainUserName;
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
            //    await ShowMessageDialog("异常", "更新旧版本数据出现异常。已忽略。\n新数据文件存储至用户名文件夹下，旧数据可自行删除。");
            //    await Launcher.LaunchFolderAsync(StorageManager.StorageFolder);
            //}
            //else if (initStat > 0)
            //{
            //    await ShowMessageDialog("数据自动更新", string.Format("自动更新旧版本数据文件 {0} 个\n新数据文件存储至用户名文件夹下，旧数据可自行删除。", initStat));
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
            ShowLoading("联网获取个人主页...");
            if (!await ExpManager.GetMain()) return false;
            
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
                ShowMessageDialog("设置取消", "设置取消，保持原状。");
                buttonSetCookie.IsEnabled = true;
                buttonSetCookieProgress.IsActive = false;
                buttonSetCookieProgress.Visibility = Visibility.Collapsed;
                
                return;
            }
            if (scd.userInputCookie.Trim() == "")
            {
                ShowMessageDialog("输入为空", "设置取消，保持原状。");
                buttonSetCookie.IsEnabled = true;
                buttonSetCookieProgress.IsActive = false;
                buttonSetCookieProgress.Visibility = Visibility.Collapsed;
                return;
            }


            bool isCookieOK = ExpManager.SetCookie(scd.userInputCookie);



            if (!isCookieOK)
            {
                ShowMessageDialog("Cookie添加", ExpManager.setcookieFailedInfo);
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
                await StorageManager.SaveCookie(ExpManager.cookie);
                buttonSetCookieText.Text = "√ Cookie";
                await ShowMessageDialog("设置完成", "Cookie有效，可以更新信息了。\n点击头像链接到个人中心。");


                ContentTipsDialog scd3 = new ContentTipsDialog();
                await scd3.ShowAsync();
            }
            else
            {
                ShowMessageDialog("验证Cookie", "Cookie无效，请重新设置");
            }

            HideLoading();
        }
        
        #endregion ------------------------------------------------------------

        #region ##### 第A2步：Content更新 ##### ----------------------------------
        /// <summary>
        /// 在Main已经成功更新后，更新Content
        /// </summary>
        /// <returns>更新Content是否成功</returns>
        public async Task<bool> UpdateContents()
        {
            
            bool ret =  await ExpManager.GetContents(
                textVisitAll, 
                listViewContentExps);

            if (ret)
            {
                ShowContentInf();
                isDataAvailable = true;
                isThisTimeUpdated = true;
                ShowControls();
            }
            
            return ret;
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

            bool result = await UpdateContents();
            if (result)
            {
                await StorageManager.SaveDataPack(ExpManager.currentDataPack);

                UpdateTile();

                //ShowMessageDialog("更新完成", "点击 \"✅ 本次已更新\" 打开数据所在文件夹：\n" + StorageManager.StorageFolder.Path);

                textDate.Text = "✅ 本次已更新";
                textDate.Foreground = new SolidColorBrush(Color.FromArgb(255,120,230,120));


                buttonUpdateExp.IsEnabled = true;
                buttonUpdateExpProgress.IsActive = false;
                buttonUpdateExpProgress.Visibility = Visibility.Collapsed;
            }
            else
            {
                await ShowMessageDialog("更新出现问题, 应用需要重新启动", "如果频繁出现，那就是情况1，请联系开发者改换算法。\n可能的原因：\n1. 并发网络请求不稳定（重启应用）\n2. 要输入验证码（输入验证码再重启应用）\n3. 用户中途退出登录（重新设置Cookie）\n");
                App.Current.Exit();
            }
        }

        #endregion ------------------------------------------------------------

        #region ##### 第B1步：悬赏更新 ##### ----------------------------------

        public async Task UpdateReward(int pglow, int pghigh, string tp, int cid)
        {
            ExpManager.rewardExps.Clear();
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

                if (!(await Task0))
                {
                    break;
                }
                if (Task1 != null && !(await Task1))
                {
                    break;
                }
                if (Task2 != null && !(await Task2))
                {
                    break;
                }
            }
            isCacheReward = true;
            await ShowMessageDialog("悬赏获取", "共获取 " + ExpManager.rewardExps.Count + " 条.");
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
                await ShowMessageDialog("Warning", "页数过多，需要输入很多次验证码. 请降低页数");
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

            mainChart.DataSource = ExpManager.currentDataPack.contentExps;
            secondChart.Visibility = Visibility.Collapsed;
            mainChart.Visibility = Visibility.Visible;
            StackPanelStatCover.Visibility = Visibility.Collapsed;
            
            buttonStatistic_Click_Substep2();
        }

        private async void buttonStatistic_Click_Substep2()
        {
            ObservableCollection<ContentExpEntry> data = null;

            ContentSelectDataFileDialog sdf = new ContentSelectDataFileDialog(await StorageManager.GetDataPackFiles());
            ContentDialogResult r = await sdf.ShowAsync();

            if (r == ContentDialogResult.Secondary || r == ContentDialogResult.None) return;

            if (sdf.selectedFiles.Count == 0)
            {
                await ShowMessageDialog("无选择文件", "您当前未选择任何历史文件。无分析。");
                return;
            }

            StatManager.DataPacksSelected = await StorageManager.ReadHistoryDataPacks(sdf.selectedFiles);

            if (StatManager.LastDateDataPack != null)
            {
                if (StatManager.LastDateDataPack.date.Date == DateTime.Today.Date)
                {
                    await ShowMessageDialog("选择今天的数据可能使分析无效", "您选择的历史数据是今天的数据。可能得到全0的作差结果。");
                }
                await StatManager.Calc(StatManager.LastDateDataPack.contentExps,
                    ExpManager.currentDataPack.contentExps);
                data = StatManager.DeltaExps;
            }
            if (data != null)
            {
                secondChart.Visibility = Visibility.Visible;
                mainChart.Visibility = Visibility.Collapsed;
                secondChart.DataSource = data;
                listViewContentExps.ItemsSource = null;
                listViewContentExps.ItemsSource = ExpManager.currentDataPack.contentExps;
                textVisitAllIncrease.Text = "总浏览增量：" + (ExpManager.currentDataPack.contentExpsViewSum - StatManager.LastDateDataPack.contentExpsViewSum).ToString();
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

        private void buttonMainPage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ExpManager.cookie))
            {
                ShowMessageDialog("请先设置Cookie", "设置Cookie完成后，可以保持登录进入编辑器。\n禁止在此暴露账号密码进行登陆。");
                return;
            }
            ShowLoading("访问jingyan.baidu.com...");
            webViewMain.Navigate(new Uri("https://jingyan.baidu.com/"));

            App.currentMainPage.StackPanelWebViewCover.Visibility = Visibility.Collapsed;

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

        private void buttonTestMine_Click(object sender, RoutedEventArgs e)
        {
            string js =
                "function(p,a,c,k,e,d){e=function(c){return(c<a?\"\":e(parseInt(c/a)))+((c=c%a)>35?String.fromCharCode(c+29):c.toString(36))};if(!\'\'.replace(/^/,String)){while(c--)d[e(c)]=k[c]||e(c);k=[function(e){return d[e]}];e=function(){return\'\\\\w+\'};c=1;};while(c--)if(k[c])p=p.replace(new RegExp(\'\\\\b\'+e(c)+\'\\\\b\',\'g\'),k[c]);return p;}(\'5 D(v){7 1Z G(5(N,1S){6 h=1s.1T(\"h\");d(h.P){h.1p=5(){d(h.P==\"1Q\"||h.P==\"1R\"){h.1p=O;N()}}}t{h.1N=N};h.1M=v;1s.1P.1O(h)})};(5(f){6 F=[];d(1U 20!==\"5\"){F.l(\"//1C.1B.s/1k/3.2.1/1k.1A\")};G.1t(F.c(5(e){7 D(e)})).1z(5(){G.1t([\"//1C.1B.s/I/3.1.0/I.1A\"].c(5(1D){7 D(1D)})).1z(f)})})(5(){6 C=\"【1l】\";6 n={};M(22,4);5 M(9,r){6 1r=$(\".23 p, 21.1W\").c(5(i,e){7 e.1V}).1Y().1X([$(\"[1H=R]\").1I()]).1J(5(e,i){7 e.A()}).y(\",\");I.1L(`<x 1o=\"L\">1K</x><x 1o=\"Z\"></x>`.A(),{1G:0,1F:[\\\'2n\\\']"
                +",2p:5(k,2q){M(22,4)}});6 b=1m(1r,9,r);d(b.j<2){b.l(C)};$.15.16(O,b.c(5(e,i){7 $.14({v:\\\'//J.K.s/10/11\\\',13:{R:e}})})).18(5(){1v=1d.1e.c.1f(1c,5(e,i){7 e[0]});6 1w=b.c(5(e,i){7 e.2r(\\\'【1l】\\\',\\\'\\\')});6 B=[];1v.1b(5(e,i){d(e&&e.1a!=0){B.l(1w[i])}});d(B.j==0){2m(5(){$(\"#L\").U(\"2u\")},2o)}t{B.1b(5(u,i){n[u]=[];6 9=12;6 17=1x(u,9);$.15.16(O,17.c(5(e,i){7 $.14({v:\\\'//J.K.s/10/11\\\',13:{R:e}})})).18(5(){19=1d.1e.c.1f(1c,5(e,i){7 e[0]});H=19.c(5(e,i){7(e&&e.1a!=0)?\\\'1\\\':\\\'0\\\'}).y(\"\");2s.2x(H);w=1j(H);d(!w.Q){W();7};6 m=w.m;6 o=w.o;1h(6 i=0;i<m.j;i++){6 k=m[i];6 z=o[i];d(9+1-z>0){6 X=u.E(k-9+z,9+1-z);n[u].l(X)}t{};W()}})})}})};5 W(){$(\"#L\").U(`2w${Y.1g(n).j}2y`);$(\"#Z\").U(`${Y.1g(n).c(5(e,i){7`<p><q T=\"V:1y\">2t：</q>${e}</p><p><q T=\"V:1y\">2v：</q>${n[e].j>0?n[e].c(5(1E){7`<q T=\"V:2l;\">${1E}</q>`})" 
                + ".y(\"  \"):\\\'29，28<a 2b=\"25://J.K.s/24/27\" 26=\"2c\">2i</a>2k\\\'}</p>`;}).y(\"\")}`);};5 1x(8,9){8=8.A();1i=\"】\".r(9)+8+\"】\".r(9);6 b=[];1h(6 i=1;i<8.j+9;i++){b.l(1i.E(i,9))};7 b;};5 1j(8){6 S=8.2e(/1+/g);d(!S){7{Q:2d}};6 o=S.c(5(e,i){7 e.j});6 1n=\"0\"+8;6 m=[];6 k=0;1u(1q){k=1n.2g(\\\'2f\\\',k);d(k==-1){2j}t{m.l(k);k++}};7{o:o,m:m,Q:1q}};5 1m(8,9,r){8=8.A();6 b=[];1u(8.j>9){b.l(8.2h(0,9));8=8.E(9-r,2a)};d(8.j>0){b.l(8)};b=b.c(5(e,i){d(e.j<12){7 e+C}t{7 e}});7 b}});\',62,159,\'|||||function|var|return|str|perlen||arr|map|if||||script||length|index|push|indexs|sentences|lens||span|repeat|com|else|sentence|url|indexAndLen|div|join|len|trim|invalides|fill|loadScript|substr|jss|Promise|mine_str|layer|jingyan|baidu|status|firstCheck|resolve|null|readyState|find|title|matchs|style|html|color|show|mine_word|Object|contents"
                +"|common|isTitleValid||data|ajax|when|apply|toCheck|done|results2|errno|forEach|arguments|Array|prototype|call|keys|for|str2|getIndexAndLen|jquery|填充字符|splite|str1|id|onreadystatechange|true|allText|document|all|while|results|reduction|splite2|orange|then|js|bootcss|cdn|e2|e3|btn|shade|name|val|filter|检测中|alert|src|onload|appendChild|body|loaded|complete|reject|createElement|typeof|innerText|normal|concat|toArray|new|jQuery|strong||editor|edit|http|target|content|请在|多个敏感词|999999|href|_0|false|match|01|indexOf|substring|新草稿页面|break|手动把这句话填入标题以精确检测|red|setTimeout|重新检测|200|yes|layero|replace|console|所在句子|检测通过|词汇|检测到|log|句话含敏感词\'.split(\'|\'),0,{})";


        }

        private void buttonTestBroswer_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://jingyan.baidu.com/"));
            //导航到Assets/code/test01.html，
            //必须使用ms -appx-web:
            //webViewMain.ScriptNotify += (o, args) =>
            //{
            //    ShowMessageDialog(o.ToString(), args.Value);
            //};
            //webViewMain.Navigate(new Uri("ms-appx-web:///Assets/code/test01.html"));
        }

        public void WebSetUpdate(bool isMainPage, bool isDraft, bool isEdit, bool isTestMine)
        {
            buttonMainPage.IsEnabled = isMainPage;
            buttonDraft.IsEnabled = isDraft;
            buttonEdit.IsEnabled = isEdit;
            buttonTestMine.IsEnabled = isTestMine;
            buttonAutoFill.IsEnabled = isEdit;
            buttonBigPicture.IsEnabled = isEdit;
            buttonAutoComplete.IsEnabled = isEdit;
        }

        private async void buttonBigPicture_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading("运行JS...");
            try
            {
                await JSCodeString.RunJs(webViewMain, JSCodeString.JsAddPictureBox);
            }
            catch (Exception)
            {
                await ShowMessageDialog("添加失败", "当前页面可能不是编辑器页面");
            }
            HideLoading();
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
                await ShowMessageDialog("自动填写出现问题", "当前页面可能不是编辑器页面，或者设置有问题（含有特殊字符，或者设置格式错误）");
            }
            HideLoading();
        }

        private async void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //await JSCodeString.AddCssUri(webViewMain, "ms-appx-web:///Assets/code/test01.css");
            //await JSCodeString.AddScriptUri(webViewMain, "ms-appx-web:///Assets/code/jquery.autocompleter.js");
            //await ShowMessageDialog("添加完毕", "添加完毕");
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

        private async void buttonAutoComplete_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading("初始化...");
            try
            {
                await JSCodeString.AddScriptUri(webViewMain, "ms-appx-web:///Assets/code/jquery.autocompleter.js");
                await JSCodeString.AddCssUri(webViewMain, "ms-appx-web:///Assets/code/jquery.autocompleter.css");
                await JSCodeString.AddScriptUri(webViewMain, "ms-appx-web:///Assets/code/AutoComplete.js");
                ShowLoading("加载组件中...");
                string data = await StorageManager.ReadAutoCompleteData("");
                await Task.Delay(500);
                
                if (data == "")
                {
                    await JSCodeString.RunJs(webViewMain, "InitAutoComplete();");
                }
                else
                {
                    //希望执行js代码： InitAutoComplete("{...}")，
                    //data就是一个json字符串："{...}"

                    await JSCodeString.RunJs(webViewMain, 
                        "InitAutoComplete(\""
                        + GetTransString(data)
                        + "\")"
                        );
                    //"InitAutoComplete(\"\\\"ms-appdata:///local/AutoCompleteData/default.json\\\"\");");
                }
                
            }
            catch (Exception ee)
            {
                await ShowMessageDialog("加载 [自动补全] 出现问题", "当前页面可能不是编辑器页面。"); // ee.GetType().ToString() + '\n' +  ee.Message);
            }
            HideLoading();
        }

    }
}
