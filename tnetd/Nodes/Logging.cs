using System;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;

namespace TNetD.Nodes
{
    public enum LogType { Voting, TimeSync, Network, Security };

    class Logging
    {
        private StreamWriter votinglog, timesynclog, networklog, securitylog;

        public bool VotingLogEnabled { get; set; } = true;
        public bool TimeSyncLogEnabled { get; set; } = true;
        public bool NetworkLogEnabled { get; set; } = true;
        public bool SecurityLogEnabled { get; set; } = true;


        /// <summary>
        /// Creates a logging class for logging server activity
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

            file = new FileStream(privateDirectory + "/logs/network.log", FileMode.Append);
            networklog = new StreamWriter(file, Encoding.UTF8);
            networklog.WriteLine("Logging started at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ".");
            networklog.Flush();

            file = new FileStream(privateDirectory + "/logs/security.log", FileMode.Append);
            securitylog = new StreamWriter(file, Encoding.UTF8);
            securitylog.WriteLine("Logging started at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ".");
            securitylog.Flush();
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
                    if (TimeSyncLogEnabled)
                    {
                        timesynclog.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + methodName + "] " + message);
                        timesynclog.Flush();
                    }
                    break;
                case LogType.Voting:
                    if (VotingLogEnabled)
                    {
                        votinglog.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + methodName + "] " + message);
                        votinglog.Flush();
                    }
                    break;
                case LogType.Network:
                    if (NetworkLogEnabled)
                    {
                        networklog.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + methodName + "] " + message);
                        networklog.Flush();
                    }
                    break;
                case LogType.Security:
                    if (SecurityLogEnabled)
                    {
                        securitylog.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + methodName + "] " + message);
                        securitylog.Flush();
                    }
                    break;
            }

        }

        public void Enable()
        {
            VotingLogEnabled = true;
            TimeSyncLogEnabled = true;
            NetworkLogEnabled = true;
            SecurityLogEnabled = true;
        }

        public void Disable()
        {
            VotingLogEnabled = false;
            TimeSyncLogEnabled = false;
            NetworkLogEnabled = false;
            SecurityLogEnabled = false;
        }
    }
}
