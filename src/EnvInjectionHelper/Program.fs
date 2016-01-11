open System
open System.Text
open System.Runtime.InteropServices

let rec loop () = 
    let c = char (Console.Read())

    let response = match c with
                    | 'E' -> "echo"
                    | 'S' ->
                        let varName = Console.ReadLine()
                        let varValue = Console.ReadLine()
                        Environment.SetEnvironmentVariable(varName, varValue)
                        ""
                    | 'R' ->
                        let varName = Console.ReadLine()
                        Environment.GetEnvironmentVariable(varName) + "\n"              
                    | _ -> ""
    
    Console.WriteLine(response)

    loop ()

[<EntryPoint>]
let main argv = 
    loop ()
