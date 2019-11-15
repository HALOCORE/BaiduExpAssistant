using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Services.Store.Engagement;

namespace 百度经验个人助手
{
    public static class Utility
    {
        public static List<string> customEventsLog = new List<string>();
        public static Hashtable varTrace = new Hashtable();

        public static void LogEvent(string evt)
        {
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log(evt);
            customEventsLog.Add(evt);
        }

        //if fire report succeed, this app should exit or continue. No throw exceptions.
        // ------------------
        //The final exceptions are report failures, and unknown ones.
        public static async Task FireErrorReport(string name, string relatedVars, Exception err=null)
        {
            string eventStr = string.Join(", ", customEventsLog);
            string errStr = "";
            if (err != null) errStr = err.HResult.ToString() + "\n" + err.Message + "\n" + err.Source + "\n" + err.StackTrace;
            string bar = "==================";
            string report = bar + "NAME" + bar + "\n\n"
                + name + "\n"
                + bar + "VER" + bar + "\n"
                + StorageManager.VER + "\n\n"
                + bar + "EXCEPTION" + bar + "\n"
                + errStr + "\n\n"
                + bar + "RELATED-VARS" + bar + "\n"
                + relatedVars + "\n\n";

            var dlg = new ContentErrorReportDialog(name, report);
            var result = await dlg.ShowAsync();
            if(result == Windows.UI.Xaml.Controls.ContentDialogResult.Secondary)
            {
                App.currentMainPage.ShowLoading("正在发送错误报告...");
                string data = report;
                if (dlg.errorNote != "") data += bar + "NOTE" + bar + "\n" + dlg.errorNote + "\n\n";
                var formData = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("data", data) };
                string postResult = await ExpManager.PostData("http://193.112.68.240:8122/errorreport", "", formData);
                App.currentMainPage.HideLoading();

                if (postResult.StartsWith("ERROR")) {
                    await ShowMessageDialog("发送失败", postResult + "下次出错可再尝试发送，或者直接邮件开发者。");
                    throw new Exception("ERROR-REPORT-FAILED");
                }
                else
                {
                    await ShowMessageDialog("发送成功", "谢谢，错误报告已经提交成功。");
                    return;
                }
            }
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
