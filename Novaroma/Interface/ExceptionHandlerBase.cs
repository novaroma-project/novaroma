using System;
using System.Threading.Tasks;

namespace Novaroma.Interface {

    public abstract class ExceptionHandlerBase : IExceptionHandler {
        protected readonly ILogger Logger;

        protected ExceptionHandlerBase(ILogger logger) {
            Logger = logger;
        }

        public virtual async void HandleException(Exception exception, string callerName = null, string callerFilePath = null, int callerLine = -1) {
            await LogException(exception, callerName, callerFilePath, callerLine);
        }

        protected async Task LogException(Exception exception, string callerName, string callerFilePath, int callerLine) {
            var aggregateException = exception as AggregateException;
            if (aggregateException != null) {
                var exceptions = aggregateException.Flatten();
                foreach (var innerException in exceptions.InnerExceptions)
                    await LogException(innerException, callerName, callerFilePath, callerLine);

                return;
            }

            // ReSharper disable ExplicitCallerInfoArgument
            await Logger.LogError(exception.Message, exception.StackTrace, callerName, callerFilePath, callerLine);
            // ReSharper restore ExplicitCallerInfoArgument
            if (exception.InnerException != null)
                await LogException(exception.InnerException, callerName, callerFilePath, callerLine);
        }
    }
}
