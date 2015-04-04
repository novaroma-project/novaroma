using System;
using System.Runtime.CompilerServices;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Novaroma.Interface;

namespace Novaroma.Win.Utilities {

    public class BalloonExceptionHandler : ExceptionHandlerBase {

        public BalloonExceptionHandler(ILogger logger): base(logger) {
        }

        public override async void HandleException(Exception exception,
                [CallerMemberName] string callerName = null,
                [CallerFilePath] string callerFilePath = null,
                [CallerLineNumber] int callerLine = -1) {
            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
                exception = aggregateException.InnerException;

            Application.Current.Dispatcher.Invoke(() => App.NotifyIcon.ShowBalloonTip("Novaroma", exception.Message, BalloonIcon.Error));

            await LogException(exception, callerName, callerFilePath, callerLine);
        }
    }
}
