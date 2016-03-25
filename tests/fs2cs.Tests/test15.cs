using System;
using static fs2csLib.Impl;

public class Test15 {
    public static dynamic SimpleArithmetic() {
        return new Func<dynamic>(() =>
        {
            dynamic x = 10 + 12 - 3;
            dynamic y = x * 2 + 1;
            dynamic patternInput = new object[] { 1, 2 };
            dynamic r2 = 1;
            dynamic r1 = 0;
            return () => new Func<dynamic>(() =>
            {
                Func<dynamic, dynamic, dynamic, dynamic, dynamic> clo1 = fsFormat("x = %d, y = %d, x/3 = %d, x%%3 = %d\n")("x = %d, y = %d, x/3 = %d, x%%3 = %d\n", log);
                return (arg10) => new Func<dynamic>(() =>
                {
                    Func<dynamic, dynamic, dynamic, dynamic> clo2 = clo1(arg10);
                    return (arg20) => new Func<dynamic>(() =>
                    {
                        Func<dynamic, dynamic, dynamic> clo3 = clo2(arg20);
                        return (arg30) => new Func<dynamic>(() =>
                        {
                            Func<dynamic, dynamic> clo4 = clo3(arg30);
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