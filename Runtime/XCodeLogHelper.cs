using System;
using UnityEngine;

public class XCodeLogHelper : ILogHandler
{
    public bool enable;
    private static ILogHandler unityLogHandler = Debug.unityLogger.logHandler;

    public XCodeLogHelper(bool enable = true)
    {
        this.enable = enable;
    }

    public void LogException(Exception exception, UnityEngine.Object context)
    {
        if (enable)
        {
            exception = new Exception("📕📕 `; " + exception.Message, exception);
            unityLogHandler.LogException(exception, context);
        }
    }

    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
    {
//#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (enable)
        {
            switch (logType)
            {
                case LogType.Log:
                    format = "📗 : " + format;
                    break;
                case LogType.Warning:
                    format = "📙 : " + format;
                    break;
                case LogType.Error:
                    format = "📕 : " + format;
                    break;
                case LogType.Exception:
                    format = "📕📕 : " + format;
                    break;
                case LogType.Assert:
                    format = "📘 : " + format;
                    break;
                default:
                    break;
            }
            unityLogHandler.LogFormat(logType, context, format, args);
        }
//#endif
    }
}
