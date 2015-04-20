using System;
using System.Collections.Generic;
using System.Linq;

namespace Novaroma {

    public class QueryResult<T> {
        private static readonly Lazy<QueryResult<T>> _lazyEmpty = new Lazy<QueryResult<T>>(() => new QueryResult<T>(Enumerable.Empty<T>(), 0)); 

        private readonly IEnumerable<T> _results;
        private readonly int _inlineCount;

        public QueryResult(IEnumerable<T> results, int inlineCount) {
            _results = results;
            _inlineCount = inlineCount;
        }

        public IEnumerable<T> Results {
            get { return _results; }
        }

        public int InlineCount {
            get { return _inlineCount; }
        }

        public static QueryResult<T> Empty {
            get {
                return _lazyEmpty.Value;
            }
        }
    }

    public abstract class QueryResult {
        public abstract int InlineCount { get; }
    }
}
