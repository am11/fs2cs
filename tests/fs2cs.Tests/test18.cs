using System;
using static fs2csLib.Impl;

public class Test18 {
    public static readonly dynamic a = 5;
    public static dynamic Invoke() {
        return a > 6 ? a > 50 ? 8 : 9 : 2;
    }
}