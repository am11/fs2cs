using System;
using static fs2csLib.Impl;

public class Test18 {
    public static readonly dynamic a = 5;
    public static dynamic Invoke() {
        return a > 6 ? new Func<dynamic>(() =>
        {
            dynamic b = 18;
            return a > 50 ? b + 8 : b - 9;
        }

        )() : new Func<dynamic>(() =>
        {
            dynamic c = a * 2;
            return c + 3;
        }

        )();
    }
}