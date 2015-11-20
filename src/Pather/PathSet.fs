module Pather.PathSet

open System.Collections.Immutable
open System.Linq

type PathSet private(paths: ImmutableList<PathName>) =
    static member Empty() = new PathSet(ImmutableList<PathName>.Empty)
    static member Of paths = new PathSet(ImmutableList<PathName>.Empty.AddRange(paths))
    
    member this.Items = paths.AsEnumerable() |> Seq.toList
    
    member this.Contains path =
        paths.Contains(path)

    member this.Add path =
       this.Insert paths.Count path

    member this.Insert index path =
        match this.Contains(path) with
        | true -> this
        | false -> new PathSet(paths.Insert(index, path))



let fromSeq (source:seq<PathName>) = PathSet.Of(source)
let add (path:PathName) (set:PathSet) = set.Add(path)
let insert (index:int) (path:PathName) (set:PathSet) = set.Insert index path
let toSeq (set:PathSet) = set.Items
let toEnvVar (set:PathSet) = set.Items |> Seq.map (fun i -> i.ToString()) |> String.concat ";"