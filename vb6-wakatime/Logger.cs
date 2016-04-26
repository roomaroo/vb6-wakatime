using System;
using log4net;

namespace WakaTime
{
    internal enum LogLevel
    {
        Debug = 1,
        Info,
        Warning,
        HandledException
    };

    static class Logger
    {
        static ILog log = LogManager.GetLogger("vb6-wakatime");

        internal static void Debug(string message)
        {
            log.Debug(message);
        }

        internal static void Warning(string message)
        {
            log.Warn(message);
        }

        internal static void Error(string message, Exception ex = null)
        {
            if (ex != null)
            {
                log.Error(message);
            }
            else
            {
                log.Error(message, ex);
            }
        }

        internal static void Info(string message)
        {
            log.Info(message);
        }
    }
}
