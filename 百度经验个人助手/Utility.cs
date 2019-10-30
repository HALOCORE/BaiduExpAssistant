using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 百度经验个人助手
{
    public static class Utility
    {
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
    }

}
