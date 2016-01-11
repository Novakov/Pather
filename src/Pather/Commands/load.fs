module Pather.Commands.Load

open CommandLine


[<Verb("load")>]
type Args = {
  [<Option('f', "file", Required = true)>]File: string; 
}

let execute (args: Args) =    
    ()