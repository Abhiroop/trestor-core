﻿/*
 @Author: Arpan Jati
 @Date: Aug 2014 / August 2015
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TNetD.UI;

namespace TNetD
{
    public enum DisplayType
    {
        /// <summary>
        /// [0] : Debugging Messages { Verbose }
        /// </summary>
        Debug,

        /// <summary>
        /// [1] : General Messages
        /// </summary>
        Info,

        /// <summary>
        /// [2] : Important Status Messages
        /// </summary>
        ImportantInfo,

        /// <summary>
        /// [3] :
        /// </summary>
        Warning,

        /// <summary>
        /// [4] : Exception Caught / Thrown. 
        /// </summary>        
        Exception,

        /// <summary>
        /// [5] : Authentication Failure / Bad Signature
        /// </summary>
        AuthFailure,

        /// <summary>
        /// [6] : Invalid Transactions / Malformed Packets.
        /// </summary>
        BadData,

        /// <summary>
        /// [7] : Should not happen for good code.
        /// </summary>
        CodeAssertionFailed
        
    };

    public static class DisplayUtils
    {
        public delegate void DisplayHandler(DisplayMessageType displayMessage);
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
                    DisplayText?.Invoke(new DisplayMessageType((WriteHeader ? " Info: " : "") + Message, Brushes.LawnGreen, type, DateTime.Now));
                    break;

                case DisplayType.ImportantInfo:
                    DisplayText?.Invoke(new DisplayMessageType((WriteHeader ? " Info: " : "") + Message, Brushes.CornflowerBlue, type, DateTime.Now));
                    break;

                case DisplayType.Warning:
                    DisplayText?.Invoke(new DisplayMessageType((WriteHeader ? " Warning: " : "") + Message, Brushes.Yellow, type, DateTime.Now));
                    break;

                case DisplayType.Exception:
                    DisplayText?.Invoke(new DisplayMessageType((WriteHeader ? " Exception: " : "") + Message, Brushes.Orange, type, DateTime.Now));
                    break;

                case DisplayType.AuthFailure:
                    DisplayText?.Invoke(new DisplayMessageType((WriteHeader ? " AuthFailure: " : "") + Message, Brushes.Red, type, DateTime.Now));
                    break;

                case DisplayType.BadData:
                    DisplayText?.Invoke(new DisplayMessageType((WriteHeader ? " BadData: " : "") + Message, Brushes.Magenta, type, DateTime.Now));
                    break;

                case DisplayType.CodeAssertionFailed:
                    DisplayText?.Invoke(new DisplayMessageType((WriteHeader ? " CodeAssertionFailed: " : "") + Message, Brushes.OrangeRed, type, DateTime.Now));
                    break;

                case DisplayType.Debug:
                    DisplayText?.Invoke(new DisplayMessageType((WriteHeader ? " Debug: " : "") + Message, Brushes.LightPink, type, DateTime.Now));
                    break;
            }
        }

        public static void Display(String Message, Exception ex)
        {
            string msg = Message + " - " + ex.Message;
            if (Common.GLOBAL_EXCEPTION_STACKTRACE_DISPLAY_ENABLED) msg += " | ST: " + ex.StackTrace;
            Display(msg, DisplayType.Exception);
        }

        public static void Display(String Message, Exception ex, bool stackTrace)
        {
            string msg = Message + " - " + ex.Message;
            if (stackTrace) msg += " | ST: " + ex.StackTrace;
            Display(msg, DisplayType.Exception);
        }
    }


}
