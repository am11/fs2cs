using System;
using static fs2csLib.Impl;

public class Test19 {
    public static dynamic FunctionSample2() {
        return new Func<dynamic>(() =>
        {
            var even = (n) => n % 2 == 0;
            var tick = (x) => () => new Func<dynamic>(() =>
            {
                var clo1 = fsFormat("tick %d\n")("tick %d\n", log);
                return (arg10) => clo1(arg10);
            }

            )()(x);
            var tock = (x) => () => new Func<dynamic>(() =>
            {
                var clo1 = fsFormat("tock %d\n")("tock %d\n", log);
                return (arg10) => clo1(arg10);
            }

            )()(x);
            var choose = (x) => f(x) ? g(x) : h(x);
            var ticktock = choose(even, tick, tock);
            for (dynamic i = 0; i < 10; i++)
            {
                ticktock(i);
            }
        }

        )();
    }
}