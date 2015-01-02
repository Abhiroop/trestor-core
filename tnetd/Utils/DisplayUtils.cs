﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TNetD
{
    public enum DisplayType { Info, Warning, Exception, AuthFailure };

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
                    //Console.ForegroundColor = ConsoleColor.Green;
                    DisplayText((WriteHeader ? " INFO: " : "") + Message, Colors.LawnGreen, type);
                    break;

                case DisplayType.Warning:
                    //Console.ForegroundColor = ConsoleColor.Yellow;
                    DisplayText((WriteHeader ? " WARNING: " : "") + Message, Colors.Yellow, type);
                    break;

                case DisplayType.Exception:
                    //Console.ForegroundColor = ConsoleColor.Orange;
                    DisplayText((WriteHeader ? " Exception: " : "") + Message, Colors.Orange, type);
                    break;

                case DisplayType.AuthFailure:
                    //Console.ForegroundColor = ConsoleColor.Red;
                    DisplayText((WriteHeader ? " Exception: " : "") + Message, Colors.Red, type);
                    break;
            }
        }

        public static void Display(String Message, Exception ex)
        {
            Display(Message + " - " + ex.Message, DisplayType.Exception);
        }
    }


}