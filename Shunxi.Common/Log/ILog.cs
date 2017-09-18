using System;
using System.Reflection;

namespace Shunxi.Common.Log
{
    public interface ILog
    {
        void Info(string msg);
        void Warnning(string msg);
        void Error(string msg);
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    [LogProvider(typeof(LocalLog))]
    public static class LogFactory
    {
        private static ILog _log = null;
        public static ILog Create()
        {
            if (null == _log)
            {
                var p = typeof(LogFactory).GetTypeInfo().GetCustomAttribute<LogProviderAttribute>();
                _log = Type.GetType(p.LogProviderType.GetTypeInfo().FullName).GetConstructor(Type.EmptyTypes).Invoke(new object[0]) as ILog;
            }

            return _log;
        }
    }

    internal class LogProviderAttribute : Attribute
    {
        public Type LogProviderType { get; private set; }

        public LogProviderAttribute(Type log)
        {
            LogProviderType = log;
        }
    }
}
