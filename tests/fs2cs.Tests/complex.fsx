let SimpleArithmetic( )
    = let x = 10 + 12 - 3
       in let y = x * 2 + 1 in
          let r1,r2 = x/3, x%3
           in printf "x = %d, y = %d, x/3 = %d, x%%3 = %d\n"
                           x       y        r1         r2;;
