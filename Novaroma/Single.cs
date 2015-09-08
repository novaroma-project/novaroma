using System;

namespace Novaroma {

    public static class Single<T> where T: new() {
        private static readonly Lazy<T> _instance = new Lazy<T>();

        public static T Instance {
            get { return _instance.Value; }
        }
    }
}
