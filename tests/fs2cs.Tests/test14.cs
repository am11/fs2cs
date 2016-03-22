using System;

public class Test14 {
    public static int mul(int m, int n, int o) {
        return m * n * o;
    }

    public static int y(Func<dynamic, dynamic, dynamic, dynamic> funkce, dynamic a, dynamic b, dynamic c) {
        return funkce(a, b, c) + 1;
    }

    public static int Invoke() {
        return y(mul, 2, 3, 4);
    }
}