using System;
using static fs2csLib.Impl;

public class Test16 {
    public static void Invoke() {
        () => new Func<Func<dynamic, dynamic>>(() =>
        {
            Func<dynamic, dynamic> clo1 = fsFormat("%A")("%A", log);
            return (arg10) => clo1(arg10);
        }

        )()(5);
    }
}