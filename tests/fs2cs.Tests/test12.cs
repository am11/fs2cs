using System;
using static fs2csLib.Impl;

public class Test12 {
    public static dynamic id(dynamic m, dynamic n) {
        return m;
    }

    public static dynamic y(Func<dynamic, dynamic, dynamic> fn, dynamic a, dynamic b) {
        return fn(a, b) + 1;
    }

    public static dynamic Invoke() {
        return y((m, n) => id(m, n), 2, 3);
    }
}