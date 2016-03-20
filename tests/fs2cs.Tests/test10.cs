public class Test10 {
    public static dynamic id(dynamic x) {
        return x;
    }

    public static int y(Func<dynamic, dynamic> fn, dynamic b) {
        return fn(b) + 1;
    }

    public static int Invoke() {
        return y((x) => id(x), 1);
    }
}