using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Services.Store.Engagement;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;

namespace BD经验个人助手
{
    public static class Utility
    {
        public static List<string> customEventsLog = new List<string>();
        public static List<string> localEventLog = new List<string>();
        public static Hashtable varTrace = new Hashtable();

        public static void LogEvent(string evt)
        {
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log(evt);
            customEventsLog.Add(evt);
        }

        public static void LogLocalEvent(string evt)
        {
            localEventLog.Add(evt);
        }

        //if fire report succeed, this app should exit or continue. No throw exceptions.
        // ------------------
        //The final exceptions are report failures, and unknown ones.
        public static async Task FireErrorReport(string name, string relatedVars, Exception err = null, string eventMessage = "")
        {
            string eventStr = string.Join(", ", customEventsLog);
            string localEventStr = string.Join("\n", localEventLog);
            string errStr = "";
            if (err != null) errStr = eventMessage + "\n" + err.HResult.ToString() + "\n" + err.Message + "\n" + err.Source + "\n" + err.StackTrace;

            string varTraceStr = "";
            string relatedVarsFirstLine = relatedVars.Split('\n')[0].Trim();
            if (relatedVarsFirstLine.StartsWith("[") && relatedVarsFirstLine.EndsWith("]"))
            {
                foreach(string key in Utility.varTrace.Keys)
                {
                    if (key.Contains(relatedVarsFirstLine))
                    {
                        varTraceStr = varTraceStr + key + " = " + Utility.varTrace[key] + "\n";
                    }
                }
            }

            string bar = "==================";
            string report = bar + "NAME" + bar + "\n\n"
                + name + "\n"
                + bar + "VER" + bar + "\n"
                + StorageManager.VER + "\n\n"
                + bar + "EXCEPTION" + bar + "\n"
                + errStr + "\n\n"
                + bar + "RELATED-VARS" + bar + "\n"
                + relatedVars + "\n" + varTraceStr + "\n\n"
                + bar + "EVENTS" + bar + "\n"
                + eventStr + "\n\n"
                + bar + "LOCAL-EVENTS" + bar + "\n"
                + localEventStr + "\n\n";

            var dlg = new ContentErrorReportDialog(name, report);
            var result = Windows.UI.Xaml.Controls.ContentDialogResult.None;
            try
            {
                result = await dlg.ShowAsync();
            }
            catch (Exception) //允许在ContentDialog打开的情况下出错
            {
                bool shouldSend = await ShowConfirmDialog("是否将错误报告发送给开发者? ", "窗口被占用, 错误报告无法显示. 是否将错误报告发送给开发者?", "发送给开发者", "忽略此错误");
                if (shouldSend) result = Windows.UI.Xaml.Controls.ContentDialogResult.Secondary;
            }
            if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Secondary)
            {
                App.currentMainPage.ShowLoading("正在发送错误报告...");
                string data = report;
                if (dlg.errorNote != "") data += bar + "NOTE" + bar + "\n" + dlg.errorNote + "\n\n";
                var formData = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("data", data) };
                string postResult = await ExpManager.PostData("http://193.112.68.240:8122/errorreport", "", formData);
                App.currentMainPage.HideLoading();

                if (postResult.StartsWith("ERROR"))
                {
                    await ShowMessageDialog("发送失败", postResult + "如果始终无法发送，下次出错直接复制错误报告的内容，发送邮件给开发者 1223989563@qq.com");
                    throw new Exception("ERROR-REPORT-FAILED");
                }
                else
                {
                    await ShowMessageDialog("匿名发送成功", "谢谢，错误报告已经匿名提交成功。\n注意，开发者可以看到报告但是无法与你取得联系。\n如果需要与开发者取得联系，可邮件 1223989563@qq.com");
                    return;
                }
            }
        }

        public static async Task<string> WritableBitmapToPngBase64Async(WriteableBitmap bitmap)
        {
            Stream pixelStream = bitmap.PixelBuffer.AsStream();
            byte[] pixels = new byte[pixelStream.Length];
            await pixelStream.ReadAsync(pixels, 0, pixels.Length);

            Debug.WriteLine("# WritableBitmapToPngBase64Async called.");
            //byte[] outputData = new byte[pixelStream.Length * 2];
            //IBuffer outputBuffer = outputData.AsBuffer();
            IRandomAccessStream outputStream = new InMemoryRandomAccessStream();
            Guid BitmapEncoderGuid = BitmapEncoder.PngEncoderId;
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoderGuid, outputStream);

            Debug.WriteLine("# WritableBitmapToPngBase64Async ready to SetPixelData.");
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight,
                                (uint)bitmap.PixelWidth,
                                (uint)bitmap.PixelHeight,
                                96.0,
                                96.0,
                                pixels);
            await encoder.FlushAsync();

            Debug.WriteLine("# WritableBitmapToPngBase64Async normalStream setPosition to 0");
            Stream normalStream = outputStream.AsStream();
            normalStream.Position = 0;

            var reader = new DataReader(normalStream.AsInputStream());
            var bytes = new byte[normalStream.Length];
            await reader.LoadAsync((uint)normalStream.Length);
            reader.ReadBytes(bytes);

            normalStream.Dispose();
            pixelStream.Dispose();
            outputStream.Dispose();
            return "data:image/png;base64," + Convert.ToBase64String(bytes);
        }
        public static string StringEscaped(string input)
        {
            input = input.Replace("\\", "\\\\");
            return input;
        }

        public static string Transferred(string input)
        {
            input = input.Replace("&", "&amp;");
            input = input.Replace("<", "&lt;");
            input = input.Replace(">", "&gt;");
            input = input.Replace("'", "&apos;");
            input = input.Replace("\"", "&quot;");
            return input;
        }


        public static string DecodeTransferred(string input)
        {
            input = input.Replace("&lt;", "<");
            input = input.Replace("&gt;", ">");
            input = input.Replace("&apos;", "'");
            input = input.Replace("&quot;", "\"");
            input = input.Replace("&amp;", "&");
            return input;
        }


        public static async Task ShowMessageDialog(string title, string message)
        {
            var msgDialog = new Windows.UI.Popups.MessageDialog(message) { Title = title };
            await msgDialog.ShowAsync();
        }

        public static async Task<bool> ShowConfirmDialog(string title, string message, string ok = "确认", string cancel = "取消")
        {
            var msgDialog = new Windows.UI.Popups.MessageDialog(message) { Title = title };
            bool isConfirmed = false;
            msgDialog.Commands.Add(new Windows.UI.Popups.UICommand(ok, uiCommand => { isConfirmed = true; }));
            msgDialog.Commands.Add(new Windows.UI.Popups.UICommand(cancel, uiCommand => { isConfirmed = false; }));
            await msgDialog.ShowAsync();
            return isConfirmed;
        }

        public static async Task ShowDetailedError(string title, Exception e)
        {

            string moreInfo = "";
            if ((uint)e.HResult == 0x80020101) moreInfo = "80020101 通常是Javascript语法错误，或者其它软件未捕获的异常. 如果确定不是语法错误请联系开发者.";

            string showMsg = "错误代码：" + String.Format("{0:x8}", e.HResult) + "\n错误类型：" + e.GetType() + "\n错误信息：" + e.Message
                        + "\n错误提示：" + moreInfo;


            if (e.InnerException != null)
            {
                string innerType = e.InnerException.GetType().ToString();
                string innerMessage = e.InnerException.Message;
                showMsg += "\n\n内部错误类型：" + innerType + "\n内部错误信息：" + innerMessage;
            }

            await ShowMessageDialog(title, showMsg);
        }
    }
}

