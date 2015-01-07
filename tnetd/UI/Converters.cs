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
}
