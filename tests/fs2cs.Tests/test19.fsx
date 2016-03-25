let FunctionSample2() =
    let even n = (n%2 = 0) 
    let tick x = printf "tick %d\n" x 
    let tock x = printf "tock %d\n" x 
    let choose f g h x = if f x then g x else h x 
    let ticktock = choose even tick tock  // ticktock is a function built out of other functions using 'choose'
    for i = 0 to 10 do
        ticktock i