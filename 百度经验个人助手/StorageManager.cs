using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Windows.Networking;
using Windows.Foundation;
using System.Text;
using Windows.Storage.Streams;

using System.IO;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.UI;

namespace 百度经验个人助手
{
    [XmlType(TypeName = "ContentExpEntry")]
    public class ContentExpEntry
    {
        public ContentExpEntry(string name, string url, int view, int vote, int collect, string date)
        {
            ExpName = StorageManager.RemoveInvalidXmlChars(name);
            View = view;
            Vote = vote;
            Collect = collect;
            Date = date;
            Url = url;
            ViewIncrease = -1;
        }

        public ContentExpEntry()
        {
            //do nothing.
            ViewIncrease = -1;
        }

        [XmlAttribute("ExpName")]
        public string ExpName { get; set; }
        [XmlElement("View")]
        public int View { get; set; }
        [XmlElement("Vote")]
        public int Vote { get; set; }
        [XmlElement("Collect")]
        public int Collect { get; set; }
        [XmlElement("Date")]
        public string Date { get; set; }
        [XmlElement("Url")]
        public string Url { get; set; }

        [XmlIgnore] public int ViewIncrease { get; set; }

        [XmlIgnore]
        public string ViewIncState
        {
            get
            {
                if (ViewIncrease == -1) return "无对应";
                if (ViewIncrease == 0) return "0";
                else return "↑ " + ViewIncrease;
            }
        }

        [XmlIgnore]
        public string ViewIncColor
        {
            get
            {
                if (ViewIncrease == -1) return Color.FromArgb(120,150,150,150).ToString();
                if (ViewIncrease == 0) return "DarkOrange";
                else return "LimeGreen";
            }
        }

        
    }

    [XmlRoot("DataPack")]
    public class DataPack
    {
        #region 要序列化的字符串

        [XmlElement("mainUserName")]
        public string mainUserName;
        [XmlElement("mainIndexHuiXiang")]
        public string mainIndexHuiXiang;
        [XmlElement("mainIndexYiuZhi")]
        public string mainIndexYiuZhi;
        [XmlElement("mainIndexYuanChuang")]
        public string mainIndexYuanChuang;
        [XmlElement("mainIndexHuoYue")]
        public string mainIndexHuoYue;
        [XmlElement("mainIndexHuDong")]
        public string mainIndexHuDong;
        [XmlElement("mainExpCount")]
        public string mainExpCount;
        [XmlElement("mainPortraitUrl")]
        public string mainPortraitUrl;
        #endregion

        #region 要序列化的整数
        [XmlElement("contentExpsCount")]
        public int contentExpsCount;
        [XmlElement("contentPagesCount")]
        public int contentPagesCount;
        [XmlElement("contentExpsViewSum")]
        public int contentExpsViewSum;
        [XmlElement("contentExpsVoteSum")]
        public int contentExpsVoteSum;
        [XmlElement("contentExpsCollectSum")]
        public int contentExpsCollectSum;
        [XmlElement("contentExpsView20")]
        public int contentExpsView20;
        #endregion

        [XmlElement("date")] public DateTime date;

        [XmlArray("contentExps")]
        public ObservableCollection<ContentExpEntry> contentExps;

        public void SafeSetUserName(string uname)
        {
            mainUserName = StorageManager.RemoveInvalidXmlChars(uname);
        }
    }


    [XmlRoot("Settings")]
    public class Settings
    {
        public Settings()
        {
            isFirstIn = true;
            version = "0";
        }

        [XmlElement("Version")] public string version;
        [XmlElement("isFirstIn")] public bool isFirstIn;
    }

    [XmlRoot("EditSettings")]
    public class EditSettings
    {
        public EditSettings()
        {
            //TODO
            strTitle2Brief = "本经验介绍在\\3开发中，如何\\4。示例：标题是\\0。应用场景如：";
            strTitle2Tool = "极品飞车17=电脑\n极品飞车17=极品飞车17最高通缉\nPython=PyCharm\n关键词=工具";
            strAttention = "如果遇到问题，可以在下面提出疑问。";
            strTitle2Category = "win=2 1 5\nPhotoshop=2 1 5\n狗=4 1\n\n=2 1 5 默认分类";
            strSteps = "<步骤1>\n这里是步骤1的内容\n</步骤1>\n<步骤2>\n这里是步骤2，\n后边还可以添加步骤3\n</步骤2>";
            ifSteps = false;
            ifCheckOrigin = true;
            ifAddStep = true;
            addStepCount = 3;

        }

        [XmlIgnore] public string strTitle2Brief;
        [XmlIgnore] public string strTitle2Tool;
        [XmlIgnore] public string strAttention;
        [XmlIgnore] public string strTitle2Category;
        [XmlIgnore] public string strSteps;

        [XmlElement("StoreStrTitle2Brief")]
        public string StoreStrTitle2Brief
        {
            get { return Utility.Transferred(strTitle2Brief); }
            set { strTitle2Brief = Utility.DecodeTransferred(value); }
        }

        [XmlElement("StoreStrTitle2Tool")]
        public string StoreStrTitle2Tool
        {
            get { return Utility.Transferred(strTitle2Tool); }
            set { strTitle2Tool = Utility.DecodeTransferred(value); }
        }

        [XmlElement("StoreStrAttention")]
        public string StoreStrAttention
        {
            get { return Utility.Transferred(strAttention); }
            set { strAttention = Utility.DecodeTransferred(value); }
        }

        [XmlElement("StoreStrSteps")]
        public string StoreStrSteps
        {
            get { return Utility.Transferred(strSteps); }
            set { strSteps = Utility.DecodeTransferred(value); }
        }


        [XmlElement("StoreStrTitle2Category")]
        public string StoreStrTitle2Category
        {
            get { return Utility.Transferred(strTitle2Category); }
            set { strTitle2Category = Utility.DecodeTransferred(value); }
        }

        [XmlElement("ifSteps")] public bool ifSteps;
        [XmlElement("ifCheckOrigin")] public bool ifCheckOrigin;
        [XmlElement("ifAddStep")] public bool ifAddStep;
        [XmlElement("addStepCount")] public int addStepCount;

        
    }


    public static class StorageManager
    {
        private static StorageFolder _storageFolder = 
            ApplicationData.Current.LocalFolder;
        private static string _currentUserName;
        private static StorageFolder _currentUserFolder;
        private static StorageFolder _currentUserRecentFolder;



        public static StorageFolder StorageFolder
        {
            get { return _storageFolder; }
        }
        public static StorageFolder CurrentUserFolder
        {
            get { return _currentUserFolder; }
        }
        public static StorageFolder CurrentUserRecentFolder
        {
            get { return _currentUserRecentFolder; }
        }

        private static Settings _appSettings;
        public static Settings AppSettings{
            get { return _appSettings; }
        }

        public static EditSettings editSettings;


        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">信息</param>
        private static async Task ShowMessageDialog(string title, string message)
        {
            var msgDialog = new Windows.UI.Popups.MessageDialog(message) { Title = title };
            //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", uiCommand => { this.textUserName.Text = $"您点击了：{uiCommand.Label}"; }));
            //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("取消", uiCommand => { this.textUserName.Text = $"您点击了：{uiCommand.Label}"; }));
            await msgDialog.ShowAsync();
        }

        
        private static Regex _invalidXmlChars = new Regex(
            @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
            RegexOptions.Compiled);

        /// <summary>
        /// 移除特殊的unicode字符
        /// </summary>
        public static string RemoveInvalidXmlChars(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return _invalidXmlChars.Replace(text, "");
        }
        private static string GetValidFileName(string input)
        {
            input = input.Replace("_", "_b");

            input = input.Replace("\\", "_s");
            input = input.Replace("/", "_d");
            input = input.Replace(":", "_c");
            input = input.Replace("*", "_x");
            input = input.Replace("?", "_q");
            input = input.Replace("\"", "_y");
            input = input.Replace("<", "_l");
            input = input.Replace(">", "_r");
            input = input.Replace("|", "_v");
            return input;
        }
        private static string GetFolderName(string id)
        {
            char[] forbiddens = {'\\', '/', ':', '*', '?', '"', '<', '>', '|'};
            bool isIdValid = true;

            if (id == null)
            {
                ShowMessageDialog("奇异情况", "百度id不存在。请询问开发者（1223989563@qq.com）此问题。" 
                    + "\n此时用户名："+ExpManager.newMainUserName 
                    + "\n此时经验数："+ExpManager.newMainExpCount);
                return "临时用户";
            }

            return GetValidFileName(id);

        }


        //不需要用户初始化。一开始就能用
        public static async Task<bool> ReadSettings()
        {
            _appSettings = new Settings();

            XmlSerializer serializer =
                new XmlSerializer(typeof(Settings));
            StorageFile f;

            try
            {
                f = await _storageFolder.GetFileAsync("Settings.xml");
            }
            catch (Exception e)
            {
                return false;
            }
            Stream fs = await f.OpenStreamForWriteAsync();
            XmlReader reader = XmlReader.Create(fs);
            Settings tempSets;
            try
            {
                tempSets = (Settings)serializer.Deserialize(reader);
            }
            catch (InvalidOperationException e)
            {
                await ShowMessageDialog("遇到格式错误的设置文件", "非关键问题，一切继续。看到此消息可截图给开发者以解决问题。\n设置文件：" + f.Name + "\n" + e.Message);
                reader.Dispose();
                fs.Dispose();
                return false;
            }

            reader.Dispose();
            fs.Dispose();
            _appSettings = tempSets;
            return true;
        }

        public static async Task<bool> SaveSettings()
        {
            string filename = "Settings.xml";

            XmlSerializer serializer =
                new XmlSerializer(typeof(Settings));

            StorageFile file =
                await _storageFolder.CreateFileAsync(
                    filename,
                    CreationCollisionOption.ReplaceExisting
                );

            Stream fs = await file.OpenStreamForWriteAsync();

            try
            {
                serializer.Serialize(fs, AppSettings);
            }
            catch (Exception e)
            {
                await ShowMessageDialog("设置未保存。序列化Xml发生问题",

                    "错误类型：" + e.GetType() + "\n错误编码：" + string.Format("{0:X}", e.HResult) +
                    "\nXmlSerializer.Serialize函数出错，数据无法保存，看到此错误可截图发送给开发者。其他功能继续。" + e.Message);
                if (e.InnerException != null)
                {
                    await ShowMessageDialog(
                        "e.InnerException", "信息：" + e.Message
                                            + "类型：" + e.GetType() + "\n调用栈："
                                            + e.InnerException.StackTrace);
                }
                fs.Dispose();
                return false;
            }
            fs.Dispose();
            return true;
        }

        public static async Task<bool> ReadEditSettings()
        {
            editSettings = new EditSettings();

            XmlSerializer serializer =
                new XmlSerializer(typeof(EditSettings));
            StorageFile f;

            try
            {
                f = await _storageFolder.GetFileAsync("EditSettings.xml");
            }
            catch (Exception e)
            {
                return false;
            }
            Stream fs = await f.OpenStreamForWriteAsync();
            XmlReader reader = XmlReader.Create(fs);
            EditSettings tempSets;
            try
            {
                tempSets = (EditSettings)serializer.Deserialize(reader);
            }
            catch (InvalidOperationException e)
            {
                await ShowMessageDialog("遇到格式错误的设置文件", "非关键问题，一切继续。看到此消息可截图给开发者以解决问题。\n设置文件：" + f.Name + "\n" + e.Message);
                reader.Dispose();
                fs.Dispose();
                return false;
            }

            reader.Dispose();
            fs.Dispose();
            editSettings = tempSets;
            return true;
        }

        public static async Task<bool> SaveEditSettings()
        {
            string filename = "EditSettings.xml";

            XmlSerializer serializer =
                new XmlSerializer(typeof(EditSettings));

            StorageFile file =
                await _storageFolder.CreateFileAsync(
                    filename,
                    CreationCollisionOption.ReplaceExisting
                );

            Stream fs = await file.OpenStreamForWriteAsync();

            try
            {
                serializer.Serialize(fs, editSettings);
            }
            catch (Exception e)
            {
                await ShowMessageDialog("编辑设置未保存。序列化Xml发生问题",

                    "错误类型：" + e.GetType() + "\n错误编码：" + string.Format("{0:X}", e.HResult) +
                    "\nXmlSerializer.Serialize函数出错，数据无法保存，看到此错误可截图发送给开发者。其他功能继续。" + e.Message);
                if (e.InnerException != null)
                {
                    await ShowMessageDialog(
                        "e.InnerException", "信息：" + e.Message
                                            + "类型：" + e.GetType() + "\n调用栈："
                                            + e.InnerException.StackTrace);
                }
                fs.Dispose();
                return false;
            }
            fs.Dispose();
            return true;
        }

        public static async Task<int> InitUserFolder(string id)
        {
            //Do nothing
            bool needCreateFolder = false;
            _currentUserName = id;
            string folderName = GetFolderName(id);
            try
            {
                _currentUserFolder = await _storageFolder.GetFolderAsync(folderName);
            }
            catch (Exception e)
            {
                needCreateFolder = true;
            }

            if (needCreateFolder)
            {
                try
                {
                    _currentUserFolder = await _storageFolder.CreateFolderAsync(folderName);
                }
                catch (Exception e)
                {
                    await ShowMessageDialog("创建文件夹出错。可以截图发送给开发者", e.InnerException.ToString() + "\n" + e.StackTrace);
                    return 0;
                }
            }

            bool needCreateRecentFolder = false;
            string recentfolderName = "最新数据";
            try
            {
                _currentUserRecentFolder = await _currentUserFolder.GetFolderAsync(recentfolderName);
            }
            catch (Exception e)
            {
                needCreateRecentFolder = true;
            }

            if (needCreateRecentFolder)
            {
                try
                {
                    _currentUserRecentFolder = await _currentUserFolder.CreateFolderAsync(recentfolderName);
                }
                catch (Exception e)
                {
                    await ShowMessageDialog("创建最新文件夹出错。可以截图发送给开发者", e.InnerException.ToString() + "\n" + e.StackTrace);
                    return 0;
                }
            }

            return 0;
            //int updateCount = 0;
            //try
            //{
            //    updateCount = await UpdateSavedDataPacks();
            //}
            //catch (Exception e)
            //{
            //    return -1;
            //}
            //return updateCount;

        }

        public static async Task<string> GetCookieTry()
        {
            Stream fs;
            string storedCookie;
            try
            {
                fs = await _storageFolder.OpenStreamForReadAsync("Cookie.txt") as Stream;
                StreamReader sw = new StreamReader(fs);
                string cookie = await sw.ReadToEndAsync();
                storedCookie = cookie;
                sw.Dispose();
                fs.Dispose();
            }
            catch (Exception)
            {
                return null;
            }
            
            return storedCookie;
        }

        public static async Task SaveCookie(string cookie)
        {
            StorageFile file =
                await _storageFolder.CreateFileAsync("Cookie.txt", CreationCollisionOption.ReplaceExisting);

            Stream fs = await file.OpenStreamForWriteAsync();
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(cookie);

            sw.Dispose();
            fs?.Dispose();
        }

        public static async Task SaveDataPack(DataPack dp)
        {
            //TODO
            string filename = string.Format("每日数据Ver2_{0:d}.xml", dp.date).Replace("/","-");

            if (_currentUserFolder == null)
            {
                await ShowMessageDialog("目录不存在，无法保存数据","要解决此问题请将相关信息提供给开发者。");
                return;
            }
            StorageFile file =
                await _currentUserFolder.CreateFileAsync(
                    filename,
                    CreationCollisionOption.ReplaceExisting
                );
            StorageFile file2 =
                await _currentUserRecentFolder.CreateFileAsync(
                    "最新.xml",
                    CreationCollisionOption.ReplaceExisting
                );

            XmlSerializer serializer = 
                new XmlSerializer(typeof(DataPack));
 
            
            Stream fs = await file.OpenStreamForWriteAsync();
            Stream fs2 = await file2.OpenStreamForWriteAsync();

            try
            {
                serializer.Serialize(fs, dp);
                serializer.Serialize(fs2, dp);
            }
            catch (Exception e)
            {
                await ShowMessageDialog("序列化Xml发生问题（System.Xml.Serialization）",

                    "错误类型：" + e.GetType() + "\n错误编码：" + string.Format("{0:X}", e.HResult) +
                    "\nXmlSerializer.Serialize函数出错，数据无法保存，看到此错误可截图发送给开发者。其他功能继续。" + e.Message);
                if (e.InnerException != null)
                {
                    await ShowMessageDialog(
                        "e.InnerException", "信息：" + e.Message 
                        +"类型：" + e.GetType() +"\n调用栈：" 
                        + e.InnerException.StackTrace);
                }
            }
            //mys.Dispose();
            fs.Dispose();
            fs2.Dispose();
        }


        public static async Task<DataPack> ReadRecentDataPack()
        {
            XmlSerializer serializer =
                new XmlSerializer(typeof(DataPack));
            StorageFile f;

            try
            {
                f = await _currentUserRecentFolder.GetFileAsync("最新.xml");
            }
            catch (Exception e)
            {
                return null;
            }
            Stream fs = await f.OpenStreamForWriteAsync();
            XmlReader reader = XmlReader.Create(fs);
            DataPack tempDp;
            try
            {
                tempDp = (DataPack)serializer.Deserialize(reader);
            }
            catch (InvalidOperationException e)
            {
                await ShowMessageDialog("遇到格式错误的数据文件", "非关键问题，一切继续。看到此消息可截图给开发者以解决问题。\n文件名是：" + f.Name + "\n" + e.Message);
                reader.Dispose();
                fs.Dispose();
                return null;
            }

            reader.Dispose();
            fs.Dispose();
            return tempDp;


        }

        public static async Task<IReadOnlyList<StorageFile>> GetDataPackFiles()
        {
            return await _currentUserFolder.GetFilesAsync();
        }


        public static async Task<DataPack> ReadHistoryDataPackSingle(StorageFile file)
        {
            ObservableCollection<StorageFile> files = new ObservableCollection<StorageFile>();
            files.Add(file);
            ObservableCollection<DataPack> tempDataPacks = await ReadHistoryDataPacks(files);
            return tempDataPacks[0];
        }

        //读出历史数据包返回
        public static async Task<ObservableCollection<DataPack>> ReadHistoryDataPacks(ObservableCollection<StorageFile> files)
        {
            ObservableCollection<DataPack> tempDataPacks = new ObservableCollection<DataPack>();

            XmlSerializer serializer =
                new XmlSerializer(typeof(DataPack));



            foreach (StorageFile sf in files)
            {

                Stream fs = await sf.OpenStreamForWriteAsync();
                XmlReader reader = XmlReader.Create(fs);
                try
                {
                    DataPack tempDp = (DataPack) serializer.Deserialize(reader);
                    tempDataPacks.Add(tempDp);
                }
                catch (InvalidOperationException e)
                {
                    await ShowMessageDialog("遇到格式错误的数据文件",
                        "非关键问题，一切继续。看到此消息可截图给开发者以解决问题。\n文件名是：" + sf.Name + "\n" + e.Message);
                }

                reader.Dispose();
                fs.Dispose();
            }

            if (tempDataPacks.Count == 0)
            {
                await ShowMessageDialog("无成功读取",
                    "没有成功读取的历史数据包");
            }
            return tempDataPacks;
        }



        #region AutoComplete

        public static async Task<StorageFolder> GetSubFolderAsync(StorageFolder sfd, string folderName)
        {
            StorageFolder fd = null;
            IReadOnlyList<StorageFolder> fds = await sfd.GetFoldersAsync();
            foreach (StorageFolder tfd in fds)
            {
                if (tfd.Name == folderName)
                {
                    fd = tfd;
                    break;
                }
            }
            if (fd == null)
            {
                fd = await sfd.CreateFolderAsync(folderName);
            }
            return fd;
        }

        public static StorageFile FindFile(IReadOnlyList<StorageFile> sfs, string fileName)
        {
            foreach (StorageFile sf in sfs)
            {
                if (sf.Name == fileName)
                {
                    return sf;
                }
            }
            return null;
        }

        public static async Task SaveAutoCompleteData(string filename, string data)
        {

            StorageFolder sf = await GetSubFolderAsync(StorageFolder, "AutoCompleteData");
            StorageFile f = await sf.CreateFileAsync(
                filename == "" ? "default.json" : filename,
                CreationCollisionOption.ReplaceExisting);
            Stream fs = await f.OpenStreamForWriteAsync();
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(data);
            sw.Dispose();
            fs?.Dispose();
        }

        public static async Task<string> ReadAutoCompleteData(string filename)
        {
            StorageFolder sf = await GetSubFolderAsync(StorageFolder, "AutoCompleteData");
            IReadOnlyList<StorageFile> sfs = await sf.GetFilesAsync();
            StorageFile f = FindFile(sfs, filename == "" ? "default.json" : filename);
            if (f == null) return "";
            Stream fs = await f.OpenStreamForReadAsync();
            StreamReader sw = new StreamReader(fs);
            string data = await sw.ReadToEndAsync();
            sw.Dispose();
            fs?.Dispose();
            return data;
        }
    
        #endregion


        //Canceled this way
        //public static async Task<Collection<StorageFile>> SelectDataPackFiles()
        //{
        //    //TODO: 打开一个对话框，让用户选择一些StorageFile
        //    return null;
        //}
        ///// <summary>
        ///// 读取用户名文件夹下的所有xml
        ///// </summary>
        ///// <returns>最近的DataPack</returns>
        //public static async Task<DataPack> ReadDataPacks() 
        //{
        //    Collection<DataPack> tempDataPacks = new Collection<DataPack>();
        //    _dataPacks = new Collection<DataPack>();

        //    XmlSerializer serializer =
        //        new XmlSerializer(typeof(DataPack));


        //    IReadOnlyList<StorageFile> flist = await _currentUserFolder.GetFilesAsync();


        //    foreach (StorageFile sf in flist)
        //    {
        //        if (sf.Name.EndsWith(".xml"))
        //        {
        //            Stream fs = await sf.OpenStreamForWriteAsync();
        //            XmlReader reader = XmlReader.Create(fs);
        //            try
        //            {
        //                DataPack tempDp = (DataPack) serializer.Deserialize(reader);
        //                tempDataPacks.Add(tempDp);
        //            }
        //            catch (InvalidOperationException e)
        //            {
        //                await ShowMessageDialog("遇到格式错误的数据文件", "非关键问题，一切继续。看到此消息可截图给开发者以解决问题。\n文件名是：" + sf.Name + "\n" + e.Message);
        //            }

        //            reader.Dispose();
        //            fs.Dispose();
        //        }
        //    }

        //    if (tempDataPacks.Count > 0)
        //    {
        //        IEnumerable<DataPack> ied = tempDataPacks.OrderBy(t => t.date);
        //        foreach (DataPack tdp in ied)
        //        {
        //            _dataPacks.Add(tdp);
        //        }
        //        return _dataPacks[_dataPacks.Count - 1];
        //    }
        //    return null;

        //}

        /// <summary>
        /// 获取一个数据包的描述（时间）
        /// </summary>
        /// <param name="dp">数据包</param>
        /// <returns>描述字符串</returns>
        public static string GetDataPackDescribe(DataPack dp)
        {
            string when;
            if (DateTime.Today.Date - ExpManager.currentDataPack.date.Date == TimeSpan.FromDays(0))
                when = "今天";
            else if (DateTime.Today.Date - ExpManager.currentDataPack.date.Date == TimeSpan.FromDays(1))
                when = "昨天";
            else if (DateTime.Today.Date - ExpManager.currentDataPack.date.Date == TimeSpan.FromDays(2))
                when = "2天前";
            else if (DateTime.Today.Date - ExpManager.currentDataPack.date.Date == TimeSpan.FromDays(2))
                when = "3天前";
            else when = "很久以前";

            return when;
        }
    }
}



//try
//{
//StorageFile stfile =
//    await storageFolder.GetFileAsync("Cookeeie.txt");
//}
//catch (Exception e)
//{
//string err = e;
//}
//private static async Task<int> UpdateSavedDataPacks()
//{
//    IReadOnlyList<StorageFile> flist = await _storageFolder.GetFilesAsync();
//    XmlSerializer serializer =
//        new XmlSerializer(typeof(DataPack));
//    int updateCount = 0;

//    foreach (StorageFile sf in flist)
//    {
//        if (sf.Name.EndsWith(".xml"))
//        {
//            Stream fs = await sf.OpenStreamForWriteAsync();
//            XmlReader reader = XmlReader.Create(fs);
//            try
//            {
//                DataPack tempDp = (DataPack) serializer.Deserialize(reader);
//                if (IsDataPackObsolete(tempDp))
//                {
//                    if (await SaveUpdateToDataPack(sf.Name, tempDp))
//                        updateCount++;
//                }
//            }
//            catch (InvalidOperationException e)
//            {
//                await ShowMessageDialog("遇到无法更新的数据文件", "非关键问题，一切继续。看到此消息可反馈给开发者。\n文件名是：" + sf.Name + "\n" + e.Message);
//            }

//            reader.Dispose();
//            fs.Dispose();
//        }
//    }

//    return updateCount;          
//}
//#region Maintain obsolete data
//private static bool IsDataPackObsolete(DataPack dp)
//{
//if (dp.date.Year < 2000) return true;
//return false;
//}

//private static async Task<bool> SaveUpdateToDataPack(string filename, DataPack dp)
//{
//Match mc = Regex.Match(filename, "每日数据(.*?).xml");
//if (mc.Success)
//{
//DateTime tempDt;
//DateTime.TryParse(mc.Groups[1].Value, out tempDt);
//if (tempDt.Year > 2000)
//{
//dp.date = tempDt;
//await SaveDataPack(dp);
//return true;
//}
//}
//return false;

//}


//#endregion