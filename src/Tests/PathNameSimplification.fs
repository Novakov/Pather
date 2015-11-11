module PathNameSimplification

open Xunit
open Xunit.Abstractions
open FsUnit.Xunit
open FsCheck
open FsCheck.Xunit
open Pather

type TreeNode(name:string) =
    member this.Name = name
    member val Parent:TreeNode option = None with get, set
    member this.Path
        with get() =
            match this.Parent with
            | None -> this.Name
            | Some(p) -> p.Path + "\\" + this.Name

    member val Children: TreeNode list = [] with get, set

    override this.ToString() = this.Path

    member this.Add(node:TreeNode) =
        node.Parent <- Some(this)
        this.Children <- node :: this.Children

    member this.GenerateDescendants(levels:int, parentName:string) =
        match levels with
        | 0 -> this
        | n ->
            seq {0 .. n}
                |> Seq.map (fun i -> parentName + "F" + i.ToString())
                |> Seq.map (fun n -> (new TreeNode(n)).GenerateDescendants(levels - 1, n))
                |> Seq.iter this.Add
                |> ignore
            this

type TreeStep(node:TreeNode, steps: string list) =
    member this.Node = node
    member this.Steps = steps

    member this.StepsPath = steps |> String.concat @"\"

    member this.Current() =
        new TreeStep(this.Node, List.append steps ["."])

    member this.Parent() =
        new TreeStep(this.Node.Parent.Value, List.append steps [".."])

    member this.Child(child) =
        new TreeStep(child, List.append steps [child.Name])

    member this.PossibleSteps() =
        seq {
            yield this.Current()
            if this.Node.Parent.IsSome then
                yield this.Parent()

            yield! (this.Node.Children |> Seq.map this.Child)
        }

type Generators =
    static member TreeStep() =
        let roots = [
            (new TreeNode(@"C:")).GenerateDescendants(4, "")
            (new TreeNode(@"\\remote-server-name")).GenerateDescendants(4, "")
            (new TreeNode(@"relative-path")).GenerateDescendants(4, "")
        ]

        let rootGen = gen {
            let! i = Gen.elements(roots)
            return new TreeStep(i, [i.Name])
        }

        let rec pathGenHelper (size:int) (parent:TreeStep) =
            match size with
            | 0 -> Gen.constant(parent.Current())
            | n -> parent.PossibleSteps() |> Seq.map (fun i -> pathGenHelper (size/2) i) |> Gen.oneof


        let gen = rootGen.SelectMany(fun root -> Gen.sized(fun size -> pathGenHelper size  root))
        Arb.fromGen(gen)


[<PropertyAttribute(Arbitrary = [| typeof<Generators> |])>]
let ``Path is simplified correctly`` (step:TreeStep) =      
    let path = new PathName(step.StepsPath)
    let normalized = path.Normalize()
    normalized.Path = step.Node.Path
