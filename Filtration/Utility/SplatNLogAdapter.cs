using System;
using NLog;
using ILogger = Splat.ILogger;
using LogLevel = Splat.LogLevel;

namespace Filtration.Utility
{
    public interface ISplatNLogAdapter
    {
    }

    public class SplatNLogAdapter : ILogger, ISplatNLogAdapter
    {
        private static readonly Logger Logger = LogManager.GetLogger("SquirrelLogger");

        public void Write(string message, LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                {
                    Logger.Debug(message);
                    break;
                }
                case LogLevel.Info:
                {
                    Logger.Info(message);
                    break;
                }
                case LogLevel.Error:
                {
                    Logger.Error(message);
                    break;
                }
                case LogLevel.Fatal:
                {
                    Logger.Fatal(message);
                    break;
                }
                case LogLevel.Warn:
                {
                    Logger.Warn(message);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }

        public LogLevel Level { get; set; }
    }
}
