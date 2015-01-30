/*
 @Author: Arpan Jati
 @Date: Aug 2014
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TNetD
{
    public enum DisplayType 
    {
        /// <summary>
        /// [0] : General Messages
        /// </summary>
        Info,

        /// <summary>
        /// [1] : Important Status Messages
        /// </summary>
        ImportantInfo,

        /// <summary>
        /// [2] :
        /// </summary>
        Warning,

        /// <summary>
        ///  [3] : Exception Caught / Thrown. 
        /// </summary>        
        Exception,

        /// <summary>
        /// [4] : Authentication Failure / Bad Signature
        /// </summary>
        AuthFailure, 

        /// <summary>
        /// [5] : Invalid Transactions / Malformed Packets.
        /// </summary>
        BadData 
    };

    static class DisplayUtils
    {
        public delegate void DisplayHandler(string Text, Color color, DisplayType type);
        public static event DisplayHandler DisplayText;

        public static void Display(String Message, DisplayType type)
        {
            Display(Message, type, true);
        }

        public static void Display(String Message)
        {
            Display(Message, DisplayType.Info, false);
        }

        public static void Display(String Message, DisplayType type, bool WriteHeader)
        {
            switch (type)
            {
                case DisplayType.Info:
                    DisplayText((WriteHeader ? " Info: " : "") + Message, Colors.LawnGreen, type);
                    break;

                case DisplayType.ImportantInfo:
                    DisplayText((WriteHeader ? " Info: " : "") + Message, Colors.CornflowerBlue, type);
                    break;

                case DisplayType.Warning:
                    DisplayText((WriteHeader ? " Warning: " : "") + Message, Colors.Yellow, type);
                    break;

                case DisplayType.Exception:
                    DisplayText((WriteHeader ? " Exception: " : "") + Message, Colors.Orange, type);
                    break;

                case DisplayType.AuthFailure:
                    DisplayText((WriteHeader ? " AuthFailure: " : "") + Message, Colors.Red, type);
                    break;

                case DisplayType.BadData:
                    DisplayText((WriteHeader ? " BadData: " : "") + Message, Colors.Magenta, type);
                    break;
            }
        }

        public static void Display(String Message, Exception ex)
        {
            Display(Message + " - " + ex.Message, DisplayType.Exception);
        }
    }


}
