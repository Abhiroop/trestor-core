﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TNetD.Nodes
{
    class Logging
    {

        private FileStream file;

        /// <summary>
        /// creates a logging class for logging server activity
        /// 
        /// will log to filename logs/name.log for a given name
        /// </summary>
        /// <param name="name">name for the log file</param>
        public Logging(string name)
        {
            file = new FileStream("logs/" + name + ".log", FileMode.Create);
        }

        /// <summary>
        /// print a messge to logfile
        /// </summary>
        /// <param name="method">tell what method is calling</param>
        /// <param name="message">logged message</param>
        public void Log(string method, string message)
        {
            StreamWriter w = new StreamWriter(file, Encoding.UTF8);
            w.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + method + "] " + message);
        }
    }
}
