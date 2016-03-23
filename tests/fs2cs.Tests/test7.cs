using System;

public class Test7 {
    public static readonly int a = 123;
    public static int fac(int b) {
        return b + 1;
    }

    public static int Invoke() {
        return fac(a);
    }
}