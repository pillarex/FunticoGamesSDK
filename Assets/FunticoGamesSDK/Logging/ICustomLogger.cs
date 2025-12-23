using UnityEngine;

namespace FunticoGamesSDK.Logging
{
    public interface ICustomLogger
    {
        void Log(string message, LogType logType);
    }
}