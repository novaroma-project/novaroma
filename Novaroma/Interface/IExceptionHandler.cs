using System;
using System.Runtime.CompilerServices;

namespace Novaroma.Interface {

    public interface IExceptionHandler {

        void HandleException(Exception exception, 
            [CallerMemberName] string callerName = null, 
            [CallerFilePath] string callerFilePath = null, 
            [CallerLineNumber] int callerLine = -1
        );
    }
}
