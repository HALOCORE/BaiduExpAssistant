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
using Windows.Data.Json;
using Windows.System;
using System.Runtime.InteropServices.WindowsRuntime;

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
                if (ViewIncrease == -1) return Color.FromArgb(120, 150, 150, 150).ToString();
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

        [XmlIgnore]
        public string MainUserNameDecoded
        {
            get
            {
                return Uri.UnescapeDataString(mainUserName);
            }
        }

        public void SafeSetUserName(string uname)
        {
            mainUserName = StorageManager.RemoveInvalidXmlChars(uname);
        }

        public async Task CheckRemoveDuplicate(bool silence = false)
        {
            HashSet<string> expids = new HashSet<string>();
            string[] urlSeperator = { ".com/" };
            int duplicateCount = 0;
            List<ContentExpEntry> toRemove = new List<ContentExpEntry>();
            foreach (var elem in contentExps)
            {
                string eid = elem.Url.Split(urlSeperator, StringSplitOptions.None)[1].Trim();
                if (expids.Contains(eid))
                {
                    toRemove.Add(elem);
                    duplicateCount++;
                }
                else
                {
                    expids.Add(eid);
                }
            }

            foreach (var rme in toRemove)
            {
                contentExps.Remove(rme);
            }

            if (silence)
            {
                if (duplicateCount > 0)
                {
                    Utility.LogEvent("ERROR_DataPackDuplicate");
                }
                else if (contentExps.Count != contentExpsCount)
                {
                    Utility.LogEvent("ERROR_DataPackCount");
                }
                return;
            }

            if (duplicateCount > 0)
            {
                Utility.LogEvent("ERROR_DataPackDuplicate_REPORT");
                await Utility.ShowMessageDialog("发现重复的经验ID，可能是由于更新过程中有经验通过审核", "需要重新更新。如果反复出现，可以提交错误");
                await Utility.FireErrorReport("CheckRemoveDuplicate 发现重复的经验ID", "[exp]\ntotal=" + contentExpsCount + "\nactual=" + contentExps.Count);
            }
            else if(contentExps.Count != contentExpsCount)
            {
                Utility.LogEvent("ERROR_DataPackCount_REPORT");
                string err = "获取的经验个数和预期 " + contentExpsCount + " 不符";
                if (Utility.varTrace.Contains("last-error") && Utility.varTrace["last-error"].ToString().StartsWith("pubCountError"))
                {
                    err = "经验个数不符，原因是发现某页的已发布经验数不等于预期的总经验数";
                }
                err += "(很可能是由于更新过程中有经验通过审核)";
                await Utility.ShowMessageDialog(err, "需要重新更新。如果反复出现，可提交错误报告给开发者");
                await Utility.FireErrorReport("CheckRemoveDuplicate " + err, "[exp]\ntotal=" + contentExpsCount + "\nactual=" + contentExps.Count);
            }
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
            ifLoadAutoComplete = true;
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

        [XmlElement("ifLoadAutoComplete")] public bool ifLoadAutoComplete;
    }


    [XmlType(TypeName = "DIYTool")]
    public class DIYTool
    {
        public DIYTool(string name, string targetUrl, string trigType, string note, string code)
        {
            Name = name;
            TargetUrl = targetUrl;
            TrigType = trigType;
            Note = note;
            Code = code;
            IsActivate = false;
        }

        public DIYTool()
        {
            Name = TargetUrl = "";
            TrigType = "click";
            Note = "";
            Code = "";
            IsActivate = false;
        }

        [XmlElement("Name")] public string Name { get; set; }
        [XmlElement("TargetUrl")] public string TargetUrl { get; set; }
        [XmlElement("TrigType")] public string TrigType { get; set; }
        [XmlElement("Note")] public string Note { get; set; }
        [XmlElement("Code")] public string Code { get; set; }

        [XmlIgnore]
        public bool IsActivate { get; set; }

        [XmlIgnore]
        public bool IsClickTrig
        {
            get
            {
                return TrigType == "click";
            }
        }

        [XmlIgnore]
        public string ShowTrigType
        {
            get
            {
                if (TrigType == "click") return "🖱";
                else if (IsActivate) return "🔗 激活中";
                else return "🔗";
            }
        }

        [XmlIgnore]
        public string ShowNote
        {
            get
            {
                if (Note.Trim() == "") return "(请编辑该功能的描述)";
                if (Note.Length < 40) return Note;
                else return Note.Substring(0, 35) + "...";
            }
        }

        [XmlIgnore]
        public string StateColor1
        {
            get
            {
                if (IsActivate) return "White";
                else return "Black";
            }
        }

        [XmlIgnore]
        public string ToolSymbol
        {
            get
            {
                if (IsClickTrig)
                {
                    return "TouchPointer";
                }
                else if (IsActivate)
                {
                    return "Pause";
                }
                else
                {
                    return "Play";
                }
            }
        }
    }

    [XmlRoot("DIYToolsSettings")]
    public class DIYToolsSettings
    {
        [XmlArray("DIYTools")]
        public ObservableCollection<DIYTool> DIYTools;

        public DIYToolsSettings()
        {
            DIYTools = new ObservableCollection<DIYTool>();
        }

        public void Init(bool allClear = false)
        {
            //准备自带的功能
            var tempTools = new ObservableCollection<DIYTool>();

            DIYTool dt1 = new DIYTool(
                "开宝箱",
                "https://jingyan.baidu.com/usersign",
                "navigate",
                "先激活此工具，再打开签到日历页面，会一直开宝箱，直到所有宝箱都开启.",
                "var openb = document.getElementById('openBoxBtn'); if(openb) openb.click();");

            DIYTool dt21 = new DIYTool(
                "去往老虎机页面",
                "https://jingyan.baidu.com/user/nuc",
                "click",
                "先点击此工具，然后用 “开老虎机” 工具.",
                "window.external.notify(\"GOTO: https://jingyan.baidu.com/activity/lottery\");");

            DIYTool dt22 = new DIYTool(
                "开老虎机",
                "https://jingyan.baidu.com/user/nuc",
                "click",
                "先通过 “去往老虎机页面” 工具进入老虎机，再点击此工具.",
                "var zp = document.getElementsByClassName(\"zhuanpan\")[0];\nvar try10 = zp.getElementsByClassName(\"try10\")[0];\nfunction lwj(){\n  if(!try10.classList.contains(\"disable\")){\n     try10.click();\n     setTimeout(lwj, 1000);\n }\n}\nlwj();");

            DIYTool dt3 = new DIYTool(
               "查看未读消息-触发器",
               "https://jingyan.baidu.com/user/nuc",
               "click",
               "先激活 “查看未读消息” 功能，然后点击这个进入工作页面。",
               "window.external.notify('GOTO: https://jingyan.baidu.com/user/nucpage/message FROM: https://jingyan.baidu.com/user/nuc/');");

            DIYTool dt4 = new DIYTool(
                 "查看未读消息",
                 "https://jingyan.baidu.com/user/nucpage/message",
                 "navigate",
                 "先激活此功能，然后点击 查看未读消息-触发器",
                 "var cks = document.getElementsByClassName('msg-more-btn'); var tcount = 200; for(let ck of cks) {setTimeout(function(){ck.click()}, tcount); tcount += 200;}");

            tempTools.Add(dt1);
            tempTools.Add(dt21);
            tempTools.Add(dt22);
            tempTools.Add(dt3);
            tempTools.Add(dt4);

            if (allClear)
            {
                DIYTools.Clear();
                foreach(var tool in tempTools)
                {
                    DIYTools.Add(tool);
                }
            }
            else
            {
                var midTools = new ObservableCollection<DIYTool>();
                foreach (var utool in DIYTools)
                {
                    bool isDefault = false;
                    foreach (var tool in tempTools)
                    {
                        if (tool.Name == utool.Name) isDefault = true;
                    }
                    if (!isDefault) midTools.Add(utool);
                }

                DIYTools.Clear();
                foreach (var tool in tempTools)
                {
                    DIYTools.Add(tool);
                }
                foreach (var tool in midTools)
                {
                    DIYTools.Add(tool);
                }
            }
        }
    }

    public static class StorageManager
    {
        private static StorageFolder _storageFolder = ApplicationData.Current.LocalFolder;
        private static StorageFolder _currentUserFolder;
        private static StorageFolder _currentUserRecentFolder;

        public const string VER = "1.7.1";
        public const string FUNC_VER = "1.7.1";

        private static string _editSettingsFileName = "EditSettings.xml";
        private static string _settingsFileName = "Settings.xml";
        private static string _dIYToolsSettingsFileName = "DIYToolsSettingsV1.xml";

        private static string _commonSetDataFolderName = "CommonSetDataV1";

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


        public static Settings appSettings;
        public static EditSettings editSettings;
        public static DIYToolsSettings dIYToolsSettings;

        #region COMMON DATA
        public static JsonObject _commonSetData;

        public static string CommonSetDataString
        {
            get { return _commonSetData.ToString(); }
        }

        public static async Task InitReadAllCommonData()
        {
            _commonSetData = new JsonObject();
            StorageFolder sf = await GetSubFolderAsync(StorageFolder, _commonSetDataFolderName);
            IReadOnlyList<StorageFile> files = await sf.GetFilesAsync();
            int fileCount = 0;
            foreach (var file in files)
            {
                string data = "";
                JsonObject jo = null;
                string groupId = "";
                bool isSucceed = false;
                try
                {
                    data = await ReadStringFromFileAsync(file);
                    jo = JsonValue.Parse(data).GetObject();
                    groupId = DecodeValidFileName(file.Name).Replace(".json", "");
                    isSucceed = true;
                    fileCount++;
                }
                catch(Exception e)
                {
                    await Utility.ShowMessageDialog("发现无法解析的数据文件, 请修复或清除", "文件位置: " + sf.Name + " > " + file.Name);
                    await Launcher.LaunchFolderAsync(sf);
                    await Utility.ShowDetailedError("解析失败的详细信息", e);
                }

                if (isSucceed)
                {
                    SetCommonData(groupId, jo);
                }
            }
            App.currentMainPage.ShowNotify("设置数据读取完成", "共成功读取 " + fileCount + " 个文件");
        }

        public static void SetCommonData(string groupId, JsonObject dataObj)
        {
            if (!_commonSetData.ContainsKey(groupId)) _commonSetData[groupId] = new JsonObject();
            foreach (var key in dataObj.Keys)
            {
                _commonSetData[groupId].GetObject()[key] = dataObj[key];
            }
        }

        public static async Task SaveCommonData(string groupId)
        {
            string fname = GetValidFileName(groupId + ".json");
            await SaveStringToFileAsync(_commonSetDataFolderName, fname, _commonSetData[groupId].GetObject().ToString());
        }

        private static async Task SaveStringToFileAsync(string foldername, string filename, string data)
        {
            StorageFolder sf = await GetSubFolderAsync(StorageFolder, foldername);
            StorageFile f = await sf.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            Stream fs = await f.OpenStreamForWriteAsync();
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(data);
            sw.Dispose();
            fs?.Dispose();
        }

        private static async Task<string> ReadStringFromFileAsync(StorageFile f)
        {
            if (f == null) return "";
            Stream fs = await f.OpenStreamForReadAsync();
            StreamReader sw = new StreamReader(fs);
            string data = await sw.ReadToEndAsync();
            sw.Dispose();
            fs?.Dispose();
            return data;
        }
        #endregion

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

        public static string GetValidFileName(string input)
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

        public static string DecodeValidFileName(string input)
        {
            input = input.Replace("_v", "|");
            input = input.Replace("_r", ">");
            input = input.Replace("_l", "<");
            input = input.Replace("_y", "\"");
            input = input.Replace("_q", "?");
            input = input.Replace("_x", "*");
            input = input.Replace("_c", ":");
            input = input.Replace("_d", "/");
            input = input.Replace("_s", "\\");

            input = input.Replace("_b", "_");
            return input;
        }

        private static string GetFolderName(string id)
        {
            char[] forbiddens = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            bool isIdValid = true;

            if (id == null)
            {
                Utility.ShowMessageDialog("奇异情况", "百度id不存在。请询问开发者（1223989563@qq.com）此问题。"
                    + "\n此时用户名：" + ExpManager.newMainUserName
                    + "\n此时经验数：" + ExpManager.newMainExpCount);
                return "临时用户";
            }

            return GetValidFileName("USER-" + id);

        }

        private static async Task _handleSerializeExceptions(Exception e)
        {
            await Utility.ShowMessageDialog("设置未保存。序列化Xml发生问题",

                    "错误类型：" + e.GetType() + "\n错误编码：" + string.Format("{0:X}", e.HResult) +
                    "\nXmlSerializer.Serialize函数出错，数据无法保存，看到此错误可截图发送给开发者。其他功能继续。" + e.Message);
            if (e.InnerException != null)
            {
                await Utility.ShowMessageDialog(
                    "e.InnerException", "信息：" + e.Message
                                        + "类型：" + e.GetType() + "\n调用栈："
                                        + e.InnerException.StackTrace);
            }
        }

        //不需要用户初始化。一开始就能用
        public static async Task<bool> ReadSettings()
        {
            appSettings = new Settings();

            XmlSerializer serializer =
                new XmlSerializer(typeof(Settings));
            StorageFile f;

            try
            {
                f = await _storageFolder.GetFileAsync(_settingsFileName);
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
                await Utility.ShowMessageDialog("遇到格式错误的设置文件", "非关键问题，一切继续。看到此消息可截图给开发者以解决问题。\n设置文件：" + f.Name + "\n" + e.Message);
                reader.Dispose();
                fs.Dispose();
                return false;
            }

            reader.Dispose();
            fs.Dispose();
            appSettings = tempSets;
            return true;
        }

        public static async Task<bool> SaveSettings()
        {
            string filename = _settingsFileName;

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
                serializer.Serialize(fs, appSettings);
            }
            catch (Exception e)
            {
                await _handleSerializeExceptions(e);
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
                f = await _storageFolder.GetFileAsync(_editSettingsFileName);
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
                await Utility.ShowMessageDialog("遇到格式错误的设置文件", "非关键问题，一切继续。看到此消息可截图给开发者以解决问题。\n设置文件：" + f.Name + "\n" + e.Message);
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
            string filename = _editSettingsFileName;

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
                await _handleSerializeExceptions(e);
                fs.Dispose();
                return false;
            }
            fs.Dispose();
            return true;
        }

        public static async Task<bool> ReadDIYToolsSettings()
        {
            dIYToolsSettings = new DIYToolsSettings();

            XmlSerializer serializer =
                new XmlSerializer(typeof(DIYToolsSettings));
            StorageFile f;

            try
            {
                f = await _storageFolder.GetFileAsync(_dIYToolsSettingsFileName);
            }
            catch (Exception e)
            {
                dIYToolsSettings.Init();
                return false;
            }
            Stream fs = await f.OpenStreamForWriteAsync();
            XmlReader reader = XmlReader.Create(fs);
            DIYToolsSettings tempSets;
            try
            {
                tempSets = (DIYToolsSettings)serializer.Deserialize(reader);
            }
            catch (InvalidOperationException e)
            {
                await Utility.ShowMessageDialog("遇到格式错误的设置文件", "非关键问题，一切继续。看到此消息可截图给开发者以解决问题。\n设置文件：" + f.Name + "\n" + e.Message);
                reader.Dispose();
                fs.Dispose();
                return false;
            }

            reader.Dispose();
            fs.Dispose();
            dIYToolsSettings = tempSets;
            return true;
        }

        public static async Task<bool> SaveDIYToolsSettings()
        {
            string filename = _dIYToolsSettingsFileName;

            XmlSerializer serializer =
                new XmlSerializer(typeof(DIYToolsSettings));

            StorageFile file =
                await _storageFolder.CreateFileAsync(
                    filename,
                    CreationCollisionOption.ReplaceExisting
                );

            Stream fs = await file.OpenStreamForWriteAsync();

            try
            {
                serializer.Serialize(fs, dIYToolsSettings);
            }
            catch (Exception e)
            {
                await _handleSerializeExceptions(e);
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
                    await Utility.ShowMessageDialog("创建文件夹出错。可以截图发送给开发者", e.InnerException.ToString() + "\n" + e.StackTrace);
                    return 0;
                }
            }

            bool needCreateRecentFolder = false;
            string recentfolderName = "NewestData";
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
                    await Utility.ShowMessageDialog("创建最新文件夹出错。可以截图发送给开发者", e.InnerException.ToString() + "\n" + e.StackTrace);
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
            string filename = string.Format("DailyDataVer2_{0:d}.xml", dp.date).Replace("/", "-");

            if (_currentUserFolder == null)
            {
                await Utility.ShowMessageDialog("目录不存在，无法保存数据", "要解决此问题请将相关信息提供给开发者。");
                return;
            }
            StorageFile file =
                await _currentUserFolder.CreateFileAsync(
                    filename,
                    CreationCollisionOption.ReplaceExisting
                );
            StorageFile file2 =
                await _currentUserRecentFolder.CreateFileAsync(
                    "newest.xml",
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
                await Utility.ShowMessageDialog("序列化Xml发生问题（System.Xml.Serialization）",

                    "错误类型：" + e.GetType() + "\n错误编码：" + string.Format("{0:X}", e.HResult) +
                    "\nXmlSerializer.Serialize函数出错，数据无法保存，看到此错误可截图发送给开发者。其他功能继续。" + e.Message);
                if (e.InnerException != null)
                {
                    await Utility.ShowMessageDialog(
                        "e.InnerException", "信息：" + e.Message
                        + "类型：" + e.GetType() + "\n调用栈："
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
                f = await _currentUserRecentFolder.GetFileAsync("newest.xml");
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
                await Utility.ShowMessageDialog("遇到格式错误的数据文件", "非关键问题，一切继续。看到此消息可截图给开发者以解决问题。\n文件名是：" + f.Name + "\n" + e.Message);
                reader.Dispose();
                fs.Dispose();
                return null;
            }

            reader.Dispose();
            fs.Dispose();

            //检查和去除重复
            await tempDp.CheckRemoveDuplicate(silence: true);
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
            await tempDataPacks[0].CheckRemoveDuplicate(silence: true);
            return tempDataPacks[0];
        }

        //读出历史数据包返回 (数据分析调用)
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
                    DataPack tempDp = (DataPack)serializer.Deserialize(reader);
                    tempDataPacks.Add(tempDp);
                }
                catch (InvalidOperationException e)
                {
                    await Utility.ShowMessageDialog("遇到格式错误的数据文件",
                        "看到此消息可截图给开发者。\n文件名是：" + sf.Name + "\n" + e.Message);
                    throw e;
                }

                reader.Dispose();
                fs.Dispose();
            }

            if (tempDataPacks.Count == 0)
            {
                await Utility.ShowMessageDialog("无成功读取",
                    "没有成功读取的历史数据包");
                throw new InvalidDataException("没有成功读取的历史数据包");
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

        public static async Task<bool> SaveWritableBitmapAsync(WriteableBitmap bmp, bool isSaveAs)
        {
            string fileName = DateTime.UtcNow.Ticks + ".png";
            Guid BitmapEncoderGuid = BitmapEncoder.PngEncoderId;

            StorageFile file = null;
            StorageFolder folder = null;
            if (isSaveAs)
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("PNG图片", new List<string>() { ".png" });
                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = fileName;
                file = await savePicker.PickSaveFileAsync();
                if (file == null) return false;
            }
            else
            {
                folder = await GetSubFolderAsync(ApplicationData.Current.LocalFolder, "EDITOR-BriefPicture");
                file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            }

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoderGuid, stream);
                Stream pixelStream = bmp.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                                    (uint)bmp.PixelWidth,
                                    (uint)bmp.PixelHeight,
                                    96.0,
                                    96.0,
                                    pixels);
                await encoder.FlushAsync();
            }
            if (folder != null) await Launcher.LaunchFolderAsync(folder);
            return true;
        }
    

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


