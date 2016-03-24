using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fs2csLib
{
    public static class Impl
    {
        public static dynamic abs( dynamic p ) { return Math.Abs(p); }
        public static dynamic acos(dynamic p) { return Math.Acos(p); }
        public static dynamic asin(dynamic p) { return Math.Asin(p); }
        public static dynamic atan(dynamic p) { return Math.Atan(p); }
        public static dynamic atan2(dynamic p, dynamic q) { return Math.Atan2(p, q); }
    }
}
