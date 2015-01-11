/*
 @Author: Arpan Jati
 @Date: 8th Jan 2015 | 10th Jan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace TNetD.UI
{
    public class StatusToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retColor = new SolidColorBrush();
            retColor.Color = System.Windows.Media.Color.FromRgb(0, 0, 0);
            if ((bool)value)
            {
                retColor.Color = System.Windows.Media.Colors.Pink;
            }
            else
            {
                retColor.Color = System.Windows.Media.Colors.LightGreen; //FromRgb(0, 128, 0);
            }
            return retColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ByteArrayToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string bats = HexUtil.ToString((byte[])value);

            if (((byte[])value).Length == 64)
            {
                bats = bats.Insert(64, "\r\n"); // Newlines for signature !!! FIX BETTER method to achieve this.
            }

            return bats;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HashToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string bats = HexUtil.ToString(((Hash)value).Hex);

            if ((((Hash)value).Hex).Length == 64)
            {
                bats = bats.Insert(64, "\r\n"); // Newlines for signature !!! FIX BETTER method to achieve this.
            }

            return bats;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
