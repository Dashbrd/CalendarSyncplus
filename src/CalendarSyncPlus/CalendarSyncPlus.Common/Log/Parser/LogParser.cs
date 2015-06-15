#region File Header
// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        LogViewer
//  *      SubProject:     SmartLogViewer
//  *      Author:         Dave, Ankesh
//  *      Created On:     12-06-2015 1:57 PM
//  *      Modified On:    12-06-2015 1:57 PM
//  *      FileName:       LogParser.cs
//  * 
//  *****************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Xml;

namespace CalendarSyncPlus.Common.Log.Parser
{
    /// <summary>
    /// </summary>
    public class LogParser
    {
        /// <summary>
        /// </summary>
        /// <param name="filePath">Path of Log File, Should be in XmlLayoutLog4j Format</param>
        /// <param name="lastIndexCount">Last Index of LogEntry, Default is Zero</param>
        /// <returns></returns>
        public IEnumerable<LogItem> Parse(string filePath, int lastIndexCount = 1)
        {
            var logItems = new List<LogItem>();

            var fileText = ReadFile(filePath);

            using (var stringReader = new StringReader(fileText))
            {
                using (var xmlReader = new XmlTextReader(stringReader) { Namespaces = false, })
                {
                    try
                    {
                        while (xmlReader.Read())
                        {
                            if ((xmlReader.NodeType != XmlNodeType.Element) || (xmlReader.Name != "log4j:event"))
                            {
                                continue;
                            }
                            var logItem = new LogItem
                            {
                                Index = lastIndexCount,
                                TimeStamp = ReadLogTimeStamp(xmlReader),
                                Thread = ReadLogThread(xmlReader),
                                LogFile = filePath,
                                Level = ReadLogLevel(xmlReader)
                            };
                            while (xmlReader.Read())
                            {
                                var breakLoop = false;
                                switch (xmlReader.Name)
                                {
                                    case "log4j:event":
                                        breakLoop = true;
                                        break;
                                    default:
                                        switch (xmlReader.Name)
                                        {
                                            case ("log4j:message"):
                                            {
                                                logItem.Message = xmlReader.ReadString();
                                                break;
                                            }
                                            case ("log4j:data"):
                                            {
                                                switch (xmlReader.GetAttribute("name"))
                                                {
                                                    case ("log4jmachinename"):
                                                    {
                                                        logItem.MachineName = xmlReader.GetAttribute("value");
                                                        break;
                                                    }
                                                    case ("log4net:HostName"):
                                                    {
                                                        logItem.HostName = xmlReader.GetAttribute("value");
                                                        break;
                                                    }
                                                    case ("log4net:UserName"):
                                                    {
                                                        logItem.UserName = xmlReader.GetAttribute("value");
                                                        break;
                                                    }
                                                    case ("log4net:Identity"):
                                                    {
                                                        logItem.Identity = xmlReader.GetAttribute("value");
                                                        break;
                                                    }
                                                    case ("NDC"):
                                                    {
                                                        logItem.NDC = xmlReader.GetAttribute("value");
                                                        break;
                                                    }
                                                    case ("log4japp"):
                                                    {
                                                        logItem.App = xmlReader.GetAttribute("value");
                                                        break;
                                                    }
                                                }
                                                break;
                                            }
                                            // ReSharper disable StringLiteralsWordIsNotInDictionary
                                            case ("log4j:throwable"):
                                                // ReSharper restore StringLiteralsWordIsNotInDictionary
                                            {
                                                logItem.Throwable = xmlReader.ReadString();
                                                break;
                                            }
                                            case ("log4j:locationInfo"):
                                            {
                                                logItem.Class = xmlReader.GetAttribute("class");
                                                logItem.Method = xmlReader.GetAttribute("method");
                                                logItem.File = xmlReader.GetAttribute("file");
                                                logItem.Line = xmlReader.GetAttribute("line");
                                                break;
                                            }
                                        }
                                        break;
                                }
                                if (breakLoop)
                                {
                                    break;
                                }
                            }
                            logItems.Add(logItem);
                            lastIndexCount++;
                        }
                    }
                    catch (XmlException xmlException)
                    {
                    }
                }
            }
            return logItems;
        }

        private LogLevel ReadLogLevel(XmlTextReader xmlReader)
        {
            var logLevel = xmlReader.GetAttribute("level");
            switch (logLevel)
            {
                case "ERROR":
                    return LogLevel.Error;
                case "INFO":
                    return LogLevel.Info;
                case "DEBUG":
                    return LogLevel.Debug;
                case "WARN":
                    return LogLevel.Warn;
                case "FATAL":
                    return LogLevel.Fatal;
                default:
                    return LogLevel.Info;
            }
        }

        private string ReadLogThread(XmlTextReader xmlReader)
        {
            return xmlReader.GetAttribute("thread");
        }

        private DateTime ReadLogTimeStamp(XmlTextReader xmlReader)
        {
            var seconds = Convert.ToDouble(xmlReader.GetAttribute("timestamp"));
            var date = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(seconds);
            return date;
        }

        /// <summary>
        /// Read File
        /// </summary>
        /// <param name="filePath">Path of File To Read</param>
        /// <returns></returns>
        private string ReadFile(string filePath)
        {
            string fileText = String.Empty;
            try
            {
                using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        fileText = String.Format("<root>{0}</root>", reader.ReadToEnd());
                        reader.Close();
                    }
                    stream.Close();
                }
            }
            catch (DirectoryNotFoundException directoryNotFoundException)
            {
            }
            catch (FileNotFoundException fileNotFoundException)
            {
            }
            catch (SecurityException securityException)
            {
            }
            catch (IOException ioException)
            {
            }
            catch (Exception exception)
            {
                // ignored
            }
            return fileText;
        }
    }
}