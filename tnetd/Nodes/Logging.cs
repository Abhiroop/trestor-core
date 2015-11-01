using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.CompilerServices;

namespace TNetD.Nodes
{
    class Logging
    {
        public enum LogType { Voting, TimeSync };

        private StreamWriter votinglog, timesynclog;

        /// <summary>
        /// creates a logging class for logging server activity
        /// 
        /// will log to filename logs/name.log for a given name
        /// </summary>
        /// <param name="name">name for the log file</param>
        public Logging(string privateDirectory)
        {
            Directory.CreateDirectory(privateDirectory + "/logs");
            FileStream file;
            file = new FileStream(privateDirectory + "/logs/voting.log", FileMode.Append);
            votinglog = new StreamWriter(file, Encoding.UTF8);
            votinglog.WriteLine("Logging started at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ".");
            votinglog.Flush();
            file = new FileStream(privateDirectory + "/logs/timesync.log", FileMode.Append);
            timesynclog = new StreamWriter(file, Encoding.UTF8);
            timesynclog.WriteLine("Logging started at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ".");
            timesynclog.Flush();
        }

        /// <summary>
        /// print a messge to logfile
        /// </summary>
        /// <param name="method">tell what method is calling</param>
        /// <param name="message">logged message</param>
        public void Log(LogType logType, string message, [CallerMemberName] string methodName = "")
        {
            switch (logType)
            {
                case LogType.TimeSync:
                    timesynclog.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + methodName + "] " + message);
                    timesynclog.Flush();
                    break;
                case LogType.Voting:
                    votinglog.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + methodName + "] " + message);
                    votinglog.Flush();
                    break;
            }

        }
    }
}
