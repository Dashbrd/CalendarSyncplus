using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.CompilerServices;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace CalendarSyncPlus.Common.Log
{
    [Export]
    public class ApplicationLogger
    {
        private string _logFilePath;
        readonly Dictionary<string, ILog> _logDictionary = new Dictionary<string, ILog>();
        private ILog GetLogger(Type type)
        {
            string className = type.Name;
            ILog logger = null;
            if (_logDictionary.ContainsKey(className))
            {
                logger = _logDictionary[className];
            }
            else
            {
                logger = LogManager.GetLogger(type);
                _logDictionary.Add(className, logger);
            }
            return logger;
        }


        public void Setup()
        {
            string applicationDataDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "CalendarSyncPlus", "Log");
            _logFilePath = Path.Combine(applicationDataDirectory, "CalSyncPlusLog.xml");

            var hierarchy = (Hierarchy)LogManager.GetRepository();

            var patternLayout = new XmlLayout();
            patternLayout.ActivateOptions();

            var roller = new RollingFileAppender()
            {
                AppendToFile = true,
                MaximumFileSize = "1MB",
                File = _logFilePath,
                PreserveLogFileNameExtension = true,
                MaxSizeRollBackups = 10,
                RollingStyle = RollingFileAppender.RollingMode.Size,
                Layout = patternLayout,
            };
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            var memory = new MemoryAppender();
            memory.ActivateOptions();
            hierarchy.Root.AddAppender(memory);

            hierarchy.Root.Level = Level.Info;
            hierarchy.Configured = true;

            XmlConfigurator.Configure(hierarchy);
        }


        public void LogDebug(string message, Type type,
            [CallerMemberName] string methodName = null)
        {
            ILog logger = GetLogger(type);
            if (logger != null)
            {
                logger.Debug(string.Format("{0} - {1}", methodName, message));
            }
        }

        public void LogInfo(string message, Type type,
            [CallerMemberName] string methodName = null)
        {
            ILog logger = GetLogger(type);
            if (logger != null)
            {
                logger.Info(string.Format("{0} - {1}", methodName, message));
            }
        }


        public void LogWarn(string message, Type type,
            [CallerMemberName] string methodName = null)
        {
            ILog logger = GetLogger(type);
            if (logger != null)
            {
                logger.Warn(string.Format("{0} - {1}", methodName, message));
            }
        }

        public void LogError(string message, Exception exception, Type type,
            [CallerMemberName] string methodName = null)
        {
            ILog logger = GetLogger(type);
            if (logger != null && exception != null)
            {
                logger.Error(string.Format("{0} - {1} : {2}", methodName, message, exception));
            }
        }

        public void LogError(Exception exception, Type type,
            [CallerMemberName] string methodName = null)
        {
            ILog logger = GetLogger(type);
            if (logger != null && exception != null)
            {
                logger.Error(string.Format("{0} - {1}", methodName, exception));
            }
        }


        public void LogError(string message, Type type,
            [CallerMemberName] string methodName = null)
        {
            ILog logger = GetLogger(type);
            if (logger != null)
            {
                logger.Error(string.Format("{0} - {1}", methodName, message));
            }
        }

        public void LogFatal(string message, Type type,
            [CallerMemberName] string methodName = null)
        {
            ILog logger = GetLogger(type);
            if (logger != null)
            {
                logger.Fatal(string.Format("{0} - {1}", methodName, message));
            }
        }
    }
}