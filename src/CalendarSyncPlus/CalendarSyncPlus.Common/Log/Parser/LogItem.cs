#region Imports

using System;
using System.IO;
using System.Waf.Foundation;

#endregion

namespace CalendarSyncPlus.Common.Log.Parser
{
    /// <summary>
    ///     Details for each Log Entry
    /// </summary>
    public class LogItem : Model
    {
        #region Fields

        private string logFile = string.Empty;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public int Index { get; set; }

        /// <summary>
        ///     Gets or sets the time stamp.
        /// </summary>
        /// <value>
        ///     The time stamp.
        /// </value>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     Gets or sets the level.
        /// </summary>
        /// <value>
        ///     The level.
        /// </value>
        public LogLevel Level { get; set; }

        /// <summary>
        ///     Gets or sets the thread.
        /// </summary>
        /// <value>
        ///     The thread.
        /// </value>
        public string Thread { get; set; }

        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        ///     Gets or sets the name of the machine.
        /// </summary>
        /// <value>
        ///     The name of the machine.
        /// </value>
        public string MachineName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the user.
        /// </summary>
        /// <value>
        ///     The name of the user.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the identity.
        /// </summary>
        /// <value>
        ///     The name of the identity.
        /// </value>
        public string Identity { get; set; }

        /// <summary>
        ///     Gets or sets the NDC.
        /// </summary>
        /// <value>
        ///     The NDC.
        /// </value>
        public string NDC { get; set; }

        /// <summary>
        ///     Gets or sets the name of the host.
        /// </summary>
        /// <value>
        ///     The name of the host.
        /// </value>
        public string HostName { get; set; }

        /// <summary>
        ///     Gets or sets the application.
        /// </summary>
        /// <value>
        ///     The application.
        /// </value>
        public string App { get; set; }

        /// <summary>
        ///     Gets or sets the throwable.
        /// </summary>
        /// <value>
        ///     The throwable.
        /// </value>
        public string Throwable { get; set; }

        /// <summary>
        ///     Gets or sets the class.
        /// </summary>
        /// <value>
        ///     The class.
        /// </value>
        public string Class { get; set; }

        /// <summary>
        ///     Gets or sets the method.
        /// </summary>
        /// <value>
        ///     The method.
        /// </value>
        public string Method { get; set; }

        /// <summary>
        ///     Gets or sets the file.
        /// </summary>
        /// <value>
        ///     The file.
        /// </value>
        public string File { get; set; }

        /// <summary>
        ///     Gets or sets the line.
        /// </summary>
        /// <value>
        ///     The line.
        /// </value>
        public string Line { get; set; }

        /// <summary>
        ///     Gets or sets the log file name (used in merge mode).
        /// </summary>
        /// <value>
        ///     The log file.
        /// </value>
        public string LogFile
        {
            get { return JustFileName ? Path.GetFileName(logFile) : logFile; }
            set { logFile = value; }
        }

        /// <summary>
        ///     Gets the full log path.
        /// </summary>
        /// <value>
        ///     The full log path.
        /// </value>
        public string FullLogPath
        {
            get { return LogFile; }
        }

        /// <summary>
        ///     Gets the short log file.
        /// </summary>
        /// <value>
        ///     The short log file.
        /// </value>
        public string ShortLogFile
        {
            get { return Path.GetFileName(LogFile); }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether
        ///     LogItem="Model.LogItem.LogFile" /> property is returning the full
        ///     path of just the file name.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [just file name]; otherwise, <c>false</c> .
        /// </value>
        public static bool JustFileName { get; set; }

        #endregion
    }
}