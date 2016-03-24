using System;
using static fs2csLib.Impl;

public class Test7 {
    public static readonly dynamic a = 123;
    public static dynamic fac(dynamic b) {
        return b + 1;
    }

    public static dynamic Invoke() {
        return fac(a);
    }
}