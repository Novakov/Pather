module Extensions

open NHamcrest

let match' (check: 'a -> unit) =    
    CustomMatcher<obj>("a", fun v ->
        check (v :?> 'a)
        true
    )