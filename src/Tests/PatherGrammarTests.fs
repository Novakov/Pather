module PatherGrammar

open System.IO
open System.Linq
open System.Collections
open System.Text
open Xunit
open Xunit.Abstractions
open Antlr4.Runtime
open Grammar
open Parsing

type PrintFileListener(out: ITestOutputHelper) =
    inherit Grammar.PatherParserBaseListener()

    let currentPath = new StringBuilder()

    override this.EnterGroup ctx =
        out.WriteLine("Group {0}:", ctx.GROUP_ID().GetText())

    override this.EnterLocalPath ctx =        
        currentPath.Clear() |> ignore
        currentPath.AppendFormat("\tLocal: {0}", ctx.DRIVE().GetText()) |> ignore
        ()  
        
    override this.EnterRemotePath ctx =        
        currentPath.Clear() |> ignore
        currentPath.AppendFormat("\tRemote: {0}", ctx.NET_SERVER().GetText()) |> ignore
        ()    

    override this.EnterNamePart ctx =
        currentPath.Append(ctx.PATH_NAME_FRAGMENT().GetText()) |> ignore

    override this.EnterName ctx =        
        currentPath.Append("\\") |> ignore
        ()

    override this.EnterInterpolation ctx =
        currentPath.Append("{") |> ignore

    override this.ExitInterpolation ctx =
        currentPath.Append("}") |> ignore

    override this.ExitLocalPath ctx =
        out.WriteLine(currentPath.ToString()) 
    
    override this.ExitRemotePath ctx =
        out.WriteLine(currentPath.ToString()) 
               

type Tests(out: ITestOutputHelper) =
    
    static member Inputs () =
        let assembly = typeof<Tests>.Assembly          
        
        use resourceStream = assembly.GetManifestResourceStream("Tests.g.resources")

        use reader = new System.Resources.ResourceReader(resourceStream)        

        reader
            |> Seq.cast<DictionaryEntry>
            |> Seq.filter (fun r -> r.Key.ToString().StartsWith("pathergrammarinputs/"))
            |> Seq.map (fun r -> 
                let stream = r.Value :?> Stream

                use streamReader = new StreamReader(stream)
                streamReader.ReadToEnd()
            )
            |> Seq.map (fun r -> [| r |])
            |> Seq.toArray

        
        
    [<TheoryAttribute()>]
    [<MemberDataAttribute("Inputs")>]
    member __.Tokens (path: string) =
        path
        |> createLexer
        |> lexMode PatherLexer.PATHS_FILE
        |> allTokens
        |> skipHiddenTokens
        |> Seq.filter (fun t -> t.Channel <> PatherLexer.Hidden)
        |> Seq.iter (fun t ->
            let typeName = PatherLexer.tokenNames.ElementAtOrDefault(t.Type)

            sprintf "(%d, %d) %s -> %s" t.Line t.Column t.Text typeName
            |> out.WriteLine            
        )
        |> ignore

    [<TheoryAttribute()>]
    [<MemberDataAttribute("Inputs")>]
    member __.ParseTree (path: string) =        
        path
        |> createLexer
        |> lexMode PatherLexer.PATHS_FILE
        |> createParser
        |> (fun p -> p.root())
        |> walk (PrintFileListener(out))      
        |> stringTree out.WriteLine
        |> ignore
    