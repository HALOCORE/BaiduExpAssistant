using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BD经验个人助手
{

    public class ByteToColorConverter : IValueConverter
    {
        //绑定值变Color
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            byte i = (byte) value;
            return Color.FromArgb(255, i, i, i);

        }

        //color变绑定值
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Color == false)
                return DependencyProperty.UnsetValue;

            return value;
        }
    }

}
