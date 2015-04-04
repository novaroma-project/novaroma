using System.Collections.Generic;

namespace Novaroma {

    internal class SeasonComparer : IComparer<int> {

        public int Compare(int x, int y) {
            if (x == y) return 0;

            if ((x > 0 && y > 0) || (x <= 0 && y <= 0))
                return DefaultCompare(x, y);

            return -1 * DefaultCompare(x, y);
        }

        private static int DefaultCompare(int x, int y) {
            if (x < y) return -1;
            return 1;
        }
    }
}
