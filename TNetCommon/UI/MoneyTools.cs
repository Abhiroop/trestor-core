using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TNetD.UI
{
   /*public struct MoneyDisplayData
    {
        public string Text;
        public Brush Brush;
    }*/

    public static class MoneyTools
    {
        static Brush lawnGreen_Brush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF62E515"));
        static Brush yellow_Brush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFD700"));
        
        public static string GetMoneyDisplayString(long Money)
        {
            if (Money >= 1000000)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:N0}", ((Money / 1000000))) + " Trest";
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:N0}", ((Money))) + " Tre";
            }         
        }

        public static Brush GetMoneyDisplayColor(long Money)
        {             
            if (Money >= 1000000)
            {            
                return lawnGreen_Brush;
            }
            else
            {            
                return yellow_Brush;
            }            
        }

    }
}
