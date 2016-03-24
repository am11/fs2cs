using System;
using static fs2csLib.Impl;

public class Test10 {
    public static dynamic id(dynamic x) {
        return x;
    }

    public static dynamic y(Func<dynamic, dynamic> fn, dynamic b) {
        return fn(b) + 1;
    }

    public static dynamic Invoke() {
        return y((x) => id(x), 1);
    }
}