using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Services.Store.Engagement;

namespace 百度经验个人助手
{
    public static class Utility
    {

        public static void LogEvent(string evt)
        {
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log(evt);
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
