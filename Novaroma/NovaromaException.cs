using System;

namespace Novaroma {

    public class NovaromaException : ApplicationException {

        public NovaromaException() {
        }

        public NovaromaException(string message): base(message) {
        }

        public NovaromaException(string message, Exception inner): base(message, inner) {
        }
    }
}
