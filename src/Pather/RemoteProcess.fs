module Pather.RemoteProcess

open System
open System.Diagnostics
open Pather

let readPathSet (processId: int) =    
    let proc = Process.GetProcessById(processId)

    let environment = Native.readEnvironmentBlock proc.Handle
    let path = environment |> Map.tryPick (fun k v -> if k.Equals("Path", StringComparison.InvariantCultureIgnoreCase) then Some(v) else None)
    
    match path with
    | Some(p) -> PathSet.fromEnvVar p
    | None -> PathSet.fromSeq []

let setPath (processId: int) (path: PathSet.PathSet) =
    let proc = Process.GetProcessById(processId)

    Native.writeEnvVariable proc.Handle "PATH" (PathSet.toEnvVar path)
    ()