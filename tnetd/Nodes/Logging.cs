using System;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Timers;

namespace TNetD.Nodes
{
    public enum LogType { Voting, TimeSync, Network, Security, LedgerSync };

    class Logging
    {
        private StreamWriter votinglog, timesynclog, networklog, securitylog, ledgersynclog;

        public bool VotingLogEnabled { get; set; } = true;
        public bool TimeSyncLogEnabled { get; set; } = true;
        public bool NetworkLogEnabled { get; set; } = true;
        public bool SecurityLogEnabled { get; set; } = true;
        public bool LedgerSyncLogEnabled { get; set; } = true;

        Timer flushTimer;
        int timer_interval = 10000;


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

            file = new FileStream(privateDirectory + "/logs/timesync.log", FileMode.Append);
            timesynclog = new StreamWriter(file, Encoding.UTF8);
            timesynclog.WriteLine("Logging started at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ".");

            file = new FileStream(privateDirectory + "/logs/network.log", FileMode.Append);
            networklog = new StreamWriter(file, Encoding.UTF8);
            networklog.WriteLine("Logging started at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ".");

            file = new FileStream(privateDirectory + "/logs/security.log", FileMode.Append);
            securitylog = new StreamWriter(file, Encoding.UTF8);
            securitylog.WriteLine("Logging started at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ".");

            file = new FileStream(privateDirectory + "/logs/ledgersync.log", FileMode.Append);
            ledgersynclog = new StreamWriter(file, Encoding.UTF8);
            ledgersynclog.WriteLine("Logging started at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ".");

            flushTimer = new Timer();
            flushTimer.Interval = timer_interval;
            flushTimer.Elapsed += flushBuffers;
            flushTimer.Enabled = true;
            flushTimer.Start();
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
                    }
                    break;
                case LogType.Voting:
                    if (VotingLogEnabled)
                    {
                        votinglog.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + methodName + "] " + message);
                    }
                    break;
                case LogType.Network:
                    if (NetworkLogEnabled)
                    {
                        networklog.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + methodName + "] " + message);
                    }
                    break;
                case LogType.Security:
                    if (SecurityLogEnabled)
                    {
                        securitylog.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + methodName + "] " + message);
                    }
                    break;
                case LogType.LedgerSync:
                    if (LedgerSyncLogEnabled)
                    {
                        ledgersynclog.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + methodName + "] " + message);
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
            LedgerSyncLogEnabled = true;
        }

        public void Disable()
        {
            VotingLogEnabled = false;
            TimeSyncLogEnabled = false;
            NetworkLogEnabled = false;
            SecurityLogEnabled = false;
            LedgerSyncLogEnabled = false;
        }

        private void flushBuffers(object sender, ElapsedEventArgs e)
        {
            votinglog.Flush();
            timesynclog.Flush();
            networklog.Flush();
            securitylog.Flush();
            ledgersynclog.Flush();
        }
    }
}
