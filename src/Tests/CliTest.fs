module CliTest

open Xunit

open Pather

//[<Fact>]
let xxx ()=
    let p args =
        let parsed = args |> Cli.parse
        printf "[%s] %A\n" (parsed.Value.GetType().Name) parsed.Value

    p [| "config-sys"; @"--file"; @"c:\my\file.pat" |] 
    p [| "dump-sys"; @"--file"; @"c:\my\file.pat" |] 

    ()
