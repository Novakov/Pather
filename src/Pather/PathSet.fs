module Pather.PathSet

open System.Collections.Immutable
open System
open System.Linq

type MergeKind =
    | Leave
    | Replace
    | Append
    | Prepend

type PathSet private(paths: ImmutableList<PathName>) =
    static member Empty() = new PathSet(ImmutableList<PathName>.Empty)
    static member Of paths = new PathSet(ImmutableList<PathName>.Empty.AddRange(paths))
   

    member this.Items = paths.AsEnumerable() |> Seq.toList
    
    interface IEquatable<PathSet> with
        member this.Equals(other) =
            let rec cmp a b =
                match (a, b) with
                | ([], []) -> true
                | (ah::at, bh::bt) when ah = bh -> cmp at bt
                | (ah::at, bh::bt) -> false
            
            cmp this.Items other.Items

    override this.Equals(other) =
        match other with
        | :? PathSet as p -> (this :> IEquatable<PathSet>).Equals(p)
        | _ -> false

    override this.GetHashCode() =
        this.Items.GetHashCode()

    override this.ToString() = 
        this.Items |> Seq.map (fun i -> i.ToString()) |> String.concat ";"

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

let fromEnvVar (s:string) = s.Split(';') |> Seq.map (fun i -> new PathName(i)) |> fromSeq

let append (paths: seq<PathName>) (set:PathSet) =
    paths |> Seq.fold (fun s p -> add p s) set

let merge (second:PathSet) (kind: MergeKind) (first :PathSet) = 
    match kind with
    | Leave -> first
    | Replace -> second
    | Append -> first |> append second.Items
    | Prepend -> second |> append first.Items
        