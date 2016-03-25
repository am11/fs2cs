using System;
using static fs2csLib.Impl;

public class Test15 {
    public static dynamic SimpleArithmetic() {
        return new Func<dynamic>(() =>
        {
            var x = 10 + 12 - 3;
            var y = x * 2 + 1;
            var patternInput = new object[] { 1, 2 };
            var r2 = 1;
            var r1 = 0;
            return () => new Func<dynamic>(() =>
            {
                var clo1 = fsFormat("x = %d, y = %d, x/3 = %d, x%%3 = %d\n")("x = %d, y = %d, x/3 = %d, x%%3 = %d\n", log);
                return (arg10) => new Func<dynamic>(() =>
                {
                    var clo2 = clo1(arg10);
                    return (arg20) => new Func<dynamic>(() =>
                    {
                        var clo3 = clo2(arg20);
                        return (arg30) => new Func<dynamic>(() =>
                        {
                            var clo4 = clo3(arg30);
                            return (arg40) => clo4(arg40);
                        }

                        )();
                    }

                    )();
                }

                )();
            }

            )()(x)(x, y)(x, y, r1)(x, y, r1, r2);
        }

        )();
    }
}