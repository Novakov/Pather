open System
open Pather


[<EntryPoint>]
let main argv =     
    let commandArgs = Cli.parse argv

    match commandArgs with   
    | Cli.Error(t) ->
        printf "%s" (t.ToString())

    | Cli.ExecuteCommand(command) ->
        let moduleType = command.GetType().DeclaringType

        let executeMethod = moduleType.GetMethod("execute")

        executeMethod.Invoke(null, [|command|]) |> ignore

    0