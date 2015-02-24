using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.CompilerServices;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace OutlookGoogleSyncRefresh.Common.Log
{
    [Export]
    public class ApplicationLogger
    {
        private string LogFilePath;

        public void Setup()
        {
            var applicationDataDirectory =
               Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                   "CalendarSyncPlus", "Log");
            LogFilePath = Path.Combine(applicationDataDirectory, "CalendarSyncPlus.log");

            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %message%newline";
            patternLayout.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender();
            roller.AppendToFile = false;
            roller.File = LogFilePath;
            roller.Layout = patternLayout;
            roller.MaxSizeRollBackups = 5;
            roller.MaximumFileSize = "1MB";
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.StaticLogFileName = true;
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            MemoryAppender memory = new MemoryAppender();
            memory.ActivateOptions();
            hierarchy.Root.AddAppender(memory);

            hierarchy.Root.Level = Level.Info;
            hierarchy.Configured = true;

            BasicConfigurator.Configure();
            _logger = LogManager.GetLogger(typeof(ApplicationLogger));
        }

        private static ILog _logger = null;


        public void LogDebug(string message, [CallerFilePath] string filePath = null, [CallerMemberName] string methodName = null)
        {
            if (_logger != null)
            {
                string className = Path.GetFileNameWithoutExtension(filePath);
                _logger.Debug(string.Format("{0} - {1} - {2}", className, methodName, message));
            }
        }

        public void LogInfo(string message, [CallerFilePath] string filePath = null, [CallerMemberName] string methodName = null)
        {
            if (_logger != null)
            {
                string className = Path.GetFileNameWithoutExtension(filePath);
                _logger.Info(string.Format("{0} - {1} - {2}",className, methodName, message));
            }
        }


        public void LogWarn(string message, [CallerFilePath] string filePath = null, [CallerMemberName] string methodName = null)
        {
            if (_logger != null)
            {
                string className = Path.GetFileNameWithoutExtension(filePath);
                _logger.Warn(string.Format("{0} - {1} - {2}", className, methodName, message));
            }
        }

        public void LogError(string message, [CallerFilePath] string filePath = null, [CallerMemberName] string methodName = null)
        {
            if (_logger != null)
            {
                string className = Path.GetFileNameWithoutExtension(filePath);
                _logger.Error(string.Format("{0} - {1} - {2}", className, methodName, message));
            }
        }

        public void LogFatal(string message, [CallerFilePath] string filePath = null, [CallerMemberName] string methodName = null)
        {
            if (_logger != null)
            {
                string className = Path.GetFileNameWithoutExtension(filePath);
                _logger.Fatal(string.Format("{0} - {1} - {2}", className, methodName, message));
            }
        }
    }
}
