namespace Pather

open System

type PathName(path:string) =
    member this.Path = path

    interface IEquatable<PathName> with
        member this.Equals(other) =
            let thisPath = this.Normalize().Path
            let thatPath = other.Normalize().Path
            thisPath.Equals(thatPath, StringComparison.OrdinalIgnoreCase)

    override this.Equals(other) =
        match other with
        | :? PathName as p -> (this :> IEquatable<PathName>).Equals(p)
        | _ -> false

    override this.GetHashCode() =
        path.GetHashCode()

    override this.ToString() = this.Path

    member this.Normalize() =
        let newPath = path.Replace("/", "\\").TrimEnd('\\')

        let parts = newPath.Split('\\');

        let rec simplify (acc:string list) input =
            match input with
            | [] -> acc
            | head::tail -> 
                match head with
                | "." -> simplify acc tail
                | ".." -> simplify (List.tail acc) tail 
                | p -> simplify (p::acc) tail

        let stack = new System.Collections.Generic.Stack<string>()

        new PathName(List.ofArray parts |> simplify [] |> Seq.rev |> String.concat "\\")
