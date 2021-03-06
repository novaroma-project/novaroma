﻿using System;
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

        protected virtual async Task LogException(Exception exception, string callerName, string callerFilePath, int callerLine) {
            // ReSharper disable ExplicitCallerInfoArgument
            await Logger.LogError(exception.Message, exception.ToString(), callerName, callerFilePath, callerLine);
            // ReSharper restore ExplicitCallerInfoArgument
        }
    }
}
