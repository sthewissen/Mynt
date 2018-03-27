using System;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;
using log4net.Config;
using Mynt.Common.Enums.DisplayEnums;
using Mynt.Common.Framework.Singleton;

namespace Mynt.Common.Framework.Logging
{
    /// <summary>
    /// Class that manages logging
    /// </summary>
    public class LogHelper : SingletonBase<LogHelper>, ISingletonManageable
    {
        public static string LogPropertyDetails = "Details";
        public static string LogPropertyNamespace = "Namespace";

        private readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LogHelper()
        {
            XmlConfigurator.Configure();
        }

        public void LogFatal(string message)
        {
            Log(LogLevel.Fatal, message);
        }

        public void LogOnlyForUser(BootstrapAlertSteps level, string message)
        {
            LogLevel loglevel;
            switch (level)
            {
                case BootstrapAlertSteps.Danger:
                    loglevel = LogLevel.Error;
                    break;
                case BootstrapAlertSteps.Warning:
                    loglevel = LogLevel.Warn;
                    break;
                case BootstrapAlertSteps.Info:
                    loglevel = LogLevel.Debug;
                    break;
                case BootstrapAlertSteps.Success:
                    loglevel = LogLevel.Info;
                    break;
                default:
                    loglevel = LogLevel.Debug;
                    break;
            }
            _entries.Add(new LogEntry() { Message = message, LogLevel = loglevel });
        }

        public void Log(LogLevel level, string message, string details = "")
        {
            Log(level, message, null, details);
        }

        public void LogException(Exception ex)
        {
            var stackTrace = new StackTrace();
            var stack = stackTrace.GetFrame(1);
            if (stack != null)
            {
                var method = stack.GetMethod();
                Log(LogLevel.Error, "Exception occured in " + method.Name, ex);
            }
            else
            {
                Log(LogLevel.Error, "Exception occured, but there is no stack trace available", ex);
            }
        }

        public void Log(LogLevel level, string message, Exception ex, string details = "")
        {
            ThreadContext.Properties[LogPropertyDetails] = details;
            ThreadContext.Properties[LogPropertyNamespace] = "";

            var preconstructedEntry = new LogEntry() { LogLevel = level, Message = message, Details = details + " " + ex };

            DoLog(level, message, ex, preconstructedEntry);
        }

        public void Log(object instance, LogLevel level, string message, string details = "")
        {
            Log(instance, level, message, null, details);
        }

        public void Log(object instance, LogLevel level, string message, Exception ex, string details = "")
        {
            ThreadContext.Properties[LogPropertyDetails] = details;

            Type type = instance.GetType();
            ThreadContext.Properties[LogPropertyNamespace] = type.Namespace + "." + type.Name;

            var preconstructedEntry = new LogEntry() { LogLevel = level, Message = message, Details = details + " " + ex };

            DoLog(level, message, ex, preconstructedEntry);
        }

        private List<LogEntry> _entries = new List<LogEntry>();
        private void DoLog(LogLevel level, string message, Exception ex, LogEntry preconstructedEntry = null)
        {
            if (preconstructedEntry == null)
                preconstructedEntry = new LogEntry() { LogLevel = level, Message = message, Details = ex?.ToString() };
            switch (level)
            {
                case LogLevel.Debug:
                    _logger.Debug(message, ex);
                    break;
                case LogLevel.Info:
                    _logger.Info(message, ex);
                    break;
                case LogLevel.Warn:
                    _logger.Warn(message, ex);
                    break;
                case LogLevel.Error:
                    _logger.Error(message, ex);
                    break;
                case LogLevel.Fatal:
                    _logger.Fatal(message, ex);
                    break;
                default:
                    _logger.Fatal(message, ex);
                    break;
            }
            _entries.Add(preconstructedEntry);
        }

        public List<LogEntry> GetEntries(bool cleanAfter = true)
        {
            var oldentries = _entries;
            if (cleanAfter)
                _entries = new List<LogEntry>();
            return oldentries;
        }
    }
}
