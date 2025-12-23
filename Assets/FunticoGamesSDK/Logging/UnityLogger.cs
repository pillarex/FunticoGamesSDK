using UnityEngine;

namespace FunticoGamesSDK.Logging
{
    public class UnityLogger : ICustomLogger
    {
        public void Log(string message, LogType logType)
        {
            switch (logType)
            {
                case LogType.Exception:
                case LogType.Error:
                    Debug.LogError(message);
                    break;
                case LogType.Assert:
                    Debug.LogAssertion(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Log:
                    Debug.Log(message);
                    break;
            }
        }
    }
}