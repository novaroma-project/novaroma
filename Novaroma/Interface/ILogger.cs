using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Novaroma.Interface {

    public interface ILogger {
        Task Log(string message, string detail = null,
            [CallerMemberName] string callerName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLine = -1
        );
        Task Log(LogType logType, string message, string detail = null,
            [CallerMemberName] string callerName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLine = -1
        );
        Task LogInfo(string message, string detail = null,
            [CallerMemberName] string callerName = null, 
            [CallerFilePath] string callerFilePath = null, 
            [CallerLineNumber] int callerLine = -1
        );
        Task LogWarning(string message, string detail = null,
            [CallerMemberName] string callerName = null, 
            [CallerFilePath] string callerFilePath = null, 
            [CallerLineNumber] int callerLine = -1
        );
        Task LogError(string message, string detail = null,
            [CallerMemberName] string callerName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLine = -1
        );
        Task<IEnumerable<ILogItem>> GetLogItems(LogSearchModel logSearchModel);
        Task Clear();
    }

    public interface ILogItem {
        LogType LogType { get; }
        string Message { get; }
        string Detail { get; }
        DateTime LogDate { get; }
    }

    public enum LogType {
        Info,
        Warning,
        Error
    }

    public class LogSearchModel {
        public LogType? LogType { get; set; }
        public TimeSpan? Age { get; set; }
        public int? MaxCount { get; set; }
    }
}
