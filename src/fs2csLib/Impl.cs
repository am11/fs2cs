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

        public static IEnumerable<dynamic> append(dynamic[] a, dynamic[] b )
        {
            return a.Concat(b);
        }
        public static dynamic[] from(IEnumerable<dynamic> a)
        {
            return a.ToArray();
        }

        public static void log( dynamic p ) { Console.WriteLine(p); }

        public static void doFsFormat(string data, Action<dynamic> next)
        {
        }

        public static Action<string, Action<dynamic>> fsFormat( string format )
        {
            return doFsFormat;
        }
    }
}
