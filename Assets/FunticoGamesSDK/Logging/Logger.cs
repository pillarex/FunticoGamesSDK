using UnityEngine;

namespace FunticoGamesSDK.Logging
{
    public static class Logger
    {
        private static readonly UnityLogger UnityLogger = new UnityLogger();

        public static void Log(string message, LogType logType = LogType.Log)
        {
            UnityLogger.Log(CustomizeMessage(message, logType), logType);
        }

        public static void LogWarning(string message)
        {
            UnityLogger.Log(CustomizeMessage(message, LogType.Warning), LogType.Warning);
        }

        public static void LogError(string message)
        {
            UnityLogger.Log(CustomizeMessage(message, LogType.Error), LogType.Error);
        }

        public static void LogDedicated(string message, LogType logType = LogType.Log, bool autoSend = false)
        {
            var log = CustomizeMessage(message, logType);
            UnityLogger.Log(log, logType);
        }

        public static void LogWarningDedicated(string message, bool autoSend = false)
        {
            var log = CustomizeMessage(message, LogType.Warning);
            UnityLogger.Log(log, LogType.Warning);
        }

        public static void LogErrorDedicated(string message)
        {
            var log = CustomizeMessage(message, LogType.Error);
            UnityLogger.Log(log, LogType.Error);
        }

        private static string CustomizeMessage(string message, LogType logType) => $"[{logType}] {message}";
    }
}