using System;
using static fs2csLib.Impl;

public class Test16 {
    public static void Invoke() {
        () => new Func<dynamic>(() =>
        {
            var clo1 = fsFormat("%A")("%A",);
            return (arg10) => clo1(arg10);
        }

        )()(5);
    }
}