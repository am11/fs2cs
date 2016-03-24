using System;
using static fs2csLib.Impl;

public class Test11 {
    public static readonly dynamic a = abs(-5);
    public static readonly dynamic b = acos(1);
    public static readonly dynamic c = asin(1);
    public static readonly dynamic d = atan(1);
    public static readonly dynamic e = atan2(1, 2);
    public static dynamic Invoke() {
        return a;
    }
}