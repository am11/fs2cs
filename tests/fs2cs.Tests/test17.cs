using System;
using static fs2csLib.Impl;

public class Test17 {
    public static readonly dynamic a = new dynamic[] { 1, 2, 3 };
    public static readonly dynamic b = from(append(new dynamic[] { 4, 5, 6 }, a));
    public static dynamic Invoke() {
        return b;
    }
}