using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace BD经验个人助手
{
    public class ExpItemDataTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is ContentExpEntry)
            {
                //Windows.UI.Xaml.Controls.Control ctr = (Windows.UI.Xaml.Controls.Control)container;
                DataTemplate dt = App.currentMainPage.Resources["contentExpItemTemplate"] as DataTemplate;
                return dt;
            }
            else if (item is RewardExpEntry)
            {
                //Windows.UI.Xaml.Controls.Control ctr = (Windows.UI.Xaml.Controls.Control) container;
                return App.currentMainPage.Resources["rewardExpItemTemplate"] as DataTemplate;
            }

            return base.SelectTemplateCore(item);
        }
    }

    public class ExpItemDataSelectedTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is ContentExpEntry)
            {
                //Windows.UI.Xaml.Controls.Control ctr = (Windows.UI.Xaml.Controls.Control)container;
                DataTemplate dt = App.currentMainPage.Resources["contentExpItemSelectedTemplate"] as DataTemplate;
                return dt;
            }
            else if (item is RewardExpEntry)
            {
                //Windows.UI.Xaml.Controls.Control ctr = (Windows.UI.Xaml.Controls.Control)container;
                return App.currentMainPage.Resources["rewardExpItemSelectedTemplate"] as DataTemplate;
            }

            return base.SelectTemplateCore(item);
        }
    }
}
