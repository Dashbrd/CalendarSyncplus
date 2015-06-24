using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
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
        private readonly Dictionary<string, ILog> _logDictionary = new Dictionary<string, ILog>();
        public string LogFilePath;

        public ILog GetLogger(Type type)
        {
            var className = type.Name;

            ILog logger = null;
            ILog value;
            if (_logDictionary.TryGetValue(className, out value))
            {
                logger = value;
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
            var applicationDataDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "CalendarSyncPlus", "Log");
            LogFilePath = Path.Combine(applicationDataDirectory, "CalSyncPlusLog.xml");

            var hierarchy = (Hierarchy) LogManager.GetRepository();

            var patternLayout = new XmlLayoutSchemaLog4j();
            patternLayout.LocationInfo = true;
            patternLayout.ActivateOptions();

            var roller = new RollingFileAppender
            {
                AppendToFile = true,
                MaximumFileSize = "2MB",
                File = LogFilePath,
                PreserveLogFileNameExtension = true,
                MaxSizeRollBackups = 10,
                RollingStyle = RollingFileAppender.RollingMode.Size,
                Layout = patternLayout
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
    }
}