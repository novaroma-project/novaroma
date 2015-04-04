using System.Collections.Generic;

namespace Novaroma {

    public class QueryResult<T> {
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
    }

    public abstract class QueryResult {
        public abstract int InlineCount { get; }
    }
}
