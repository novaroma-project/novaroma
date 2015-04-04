using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Novaroma.Interface;
using Novaroma.Model;

namespace Novaroma.Engine {

    public class DbLogger : ILogger {
        private readonly IContextFactory _contextFactory;

        public DbLogger(IContextFactory contextFactory) {
            _contextFactory = contextFactory;
        }

        public Task Log(string message, string detail = null,
                [CallerMemberNameAttribute] string callerName = null,
                [CallerFilePath] string callerFilePath = null,
                [CallerLineNumber] int callerLine = -1) {
            return LogInfo(message, detail);
        }

        public Task Log(LogType logType, string message, string detail = null,
                [CallerMemberName] string callerName = null,
                [CallerFilePath] string callerFilePath = null,
                [CallerLineNumber] int callerLine = -1) {
            switch (logType) {
                case LogType.Info:
                    return LogInfo(message, detail, callerName, callerFilePath, callerLine);
                case LogType.Warning:
                    return LogWarning(message, detail, callerName, callerFilePath, callerLine);
                case LogType.Error:
                    return LogError(message, detail, callerName, callerFilePath, callerLine);
                default:
                    return LogInfo(message, detail, callerName, callerFilePath, callerLine);
            }
        }

        public Task LogInfo(string message, string detail = null,
                [CallerMemberName] string callerName = null,
                [CallerFilePath] string callerFilePath = null,
                [CallerLineNumber] int callerLine = -1) {
            return SaveLog(LogType.Info, message, detail, callerName, callerFilePath, callerLine);
        }

        public Task LogWarning(string message, string detail = null,
                [CallerMemberName] string callerName = null,
                [CallerFilePath] string callerFilePath = null,
                [CallerLineNumber] int callerLine = -1) {
            return SaveLog(LogType.Warning, message, detail, callerName, callerFilePath, callerLine);
        }

        public Task LogError(string message, string detail = null,
                [CallerMemberName] string callerName = null,
                [CallerFilePath] string callerFilePath = null,
                [CallerLineNumber] int callerLine = -1) {
            return SaveLog(LogType.Error, message, detail, callerName, callerFilePath, callerLine);
        }

        public Task<IEnumerable<ILogItem>> GetLogItems(LogSearchModel logSearchModel) {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    IQueryable<Log> q = context.Logs;
                    if (logSearchModel.LogType.HasValue)
                        q = q.Where(l => l.LogType == logSearchModel.LogType.Value);
                    if (logSearchModel.Age.HasValue)
                        q = q.Where(l => (l.LogDate - DateTime.UtcNow) < logSearchModel.Age.Value);
                    if (logSearchModel.MaxCount.HasValue)
                        q = q.Take(logSearchModel.MaxCount.Value);

                    IEnumerable<ILogItem> result = q.OrderByDescending(l => l.LogDate).ToList();
                    return result;
                }
            });
        }

        public Task Clear() {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    var logs = context.Logs.ToList();
                    logs.ForEach(context.Delete);
                    context.SaveChanges();
                }
            });
        }

        private async Task SaveLog(LogType logType, string message, string detail, string callerName, string callerFilePath, int callerLine) {
            detail = string.Format("Caller Name: {0}, Caller File Path: {1}, Caller Line: {2}", callerName, callerFilePath, callerLine)
                + Environment.NewLine + Environment.NewLine + detail;

            var log = new Log {
                LogType = logType,
                Message = message,
                Detail = detail,
                LogDate = DateTime.UtcNow
            };

            using (var context = _contextFactory.CreateContext()) {
                context.Insert(log);
                await context.SaveChanges();
            }
        }

    }
}
