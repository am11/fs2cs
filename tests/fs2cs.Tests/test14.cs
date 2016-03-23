using System;

public class Test14 {
    public static dynamic mul(dynamic m, dynamic n, dynamic o) {
        return m * n * o;
    }

    public static dynamic y(Func<dynamic, dynamic, dynamic, dynamic> funkce, dynamic a, dynamic b, dynamic c) {
        return funkce(a, b, c) + 1;
    }

    public static dynamic Invoke() {
        return y(mul, 2, 3, 4);
    }
}