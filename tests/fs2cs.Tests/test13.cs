pusing System;

ublic class Test13 {
    public static dynamic id(dynamic m, dynamic n, dynamic o) {
        return m;
    }

    public static int y(Func<dynamic, dynamic, dynamic, dynamic> funkce, dynamic a, dynamic b, dynamic c) {
        return funkce(a, b, c) + 1;
    }

    public static int Invoke() {
        return y((m, n, o) => id(m, n, o), 2, 3, 4);
    }
}