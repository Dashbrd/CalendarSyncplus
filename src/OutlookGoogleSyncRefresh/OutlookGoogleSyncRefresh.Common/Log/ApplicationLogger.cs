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
        private static ILog _logger;
        private string LogFilePath;

        public void Setup()
        {
            string applicationDataDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "CalendarSyncPlus", "Log");
            LogFilePath = Path.Combine(applicationDataDirectory, "CalendarSyncPlus.log");

            var hierarchy = (Hierarchy) LogManager.GetRepository();

            var patternLayout = new PatternLayout { ConversionPattern = "%date [%thread] %-5level %message%newline" };
            patternLayout.ActivateOptions();

            var roller = new RollingFileAppender
            {
                AppendToFile = false,
                File = LogFilePath,
                Layout = patternLayout,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "1MB",
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true
            };
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            var memory = new MemoryAppender();
            memory.ActivateOptions();
            hierarchy.Root.AddAppender(memory);

            hierarchy.Root.Level = Level.Info;
            hierarchy.Configured = true;

            BasicConfigurator.Configure();
            _logger = LogManager.GetLogger(typeof (ApplicationLogger));
        }


        public void LogDebug(string message, [CallerFilePath] string filePath = null,
            [CallerMemberName] string methodName = null)
        {
            if (_logger != null)
            {
                string className = Path.GetFileNameWithoutExtension(filePath);
                _logger.Debug(string.Format("{0} - {1} - {2}", className, methodName, message));
            }
        }

        public void LogInfo(string message, [CallerFilePath] string filePath = null,
            [CallerMemberName] string methodName = null)
        {
            if (_logger != null)
            {
                string className = Path.GetFileNameWithoutExtension(filePath);
                _logger.Info(string.Format("{0} - {1} - {2}", className, methodName, message));
            }
        }


        public void LogWarn(string message, [CallerFilePath] string filePath = null,
            [CallerMemberName] string methodName = null)
        {
            if (_logger != null)
            {
                string className = Path.GetFileNameWithoutExtension(filePath);
                _logger.Warn(string.Format("{0} - {1} - {2}", className, methodName, message));
            }
        }

        public void LogError(string message, [CallerFilePath] string filePath = null,
            [CallerMemberName] string methodName = null)
        {
            if (_logger != null)
            {
                string className = Path.GetFileNameWithoutExtension(filePath);
                _logger.Error(string.Format("{0} - {1} - {2}", className, methodName, message));
            }
        }

        public void LogFatal(string message, [CallerFilePath] string filePath = null,
            [CallerMemberName] string methodName = null)
        {
            if (_logger != null)
            {
                string className = Path.GetFileNameWithoutExtension(filePath);
                _logger.Fatal(string.Format("{0} - {1} - {2}", className, methodName, message));
            }
        }
    }
}