using System;
using static fs2csLib.Impl;

public class Test19 {
    public static dynamic FunctionSample2() {
        return new Func<dynamic>(() =>
        {
            Func<dynamic, dynamic> even = (n) => n % 2 == 0;
            Func<dynamic, dynamic> tick = (x) => () => new Func<dynamic>(() =>
            {
                Func<dynamic, dynamic> clo1 = fsFormat("tick %d\n")("tick %d\n", log);
                return (arg10) => clo1(arg10);
            }

            )()(x);
            Func<dynamic, dynamic> tock = (x) => () => new Func<dynamic>(() =>
            {
                Func<dynamic, dynamic> clo1 = fsFormat("tock %d\n")("tock %d\n", log);
                return (arg10) => clo1(arg10);
            }

            )()(x);
            Func<dynamic, dynamic, dynamic, dynamic, dynamic> choose = (x) => f(x) ? g(x) : h(x);
            Func<dynamic, dynamic> ticktock = choose(even, tick, tock);
            for (dynamic i = 0; i < 10; i++)
            {
                ticktock(i);
            }
        }

        )();
    }
}