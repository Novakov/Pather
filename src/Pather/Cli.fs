module Pather.Cli

open CommandLine
open System.Reflection
open System

type CommandType =
    | ExecuteCommand of Args: obj    
    | Error of Text: CommandLine.Text.HelpText

let private parser = 
    new CommandLine.Parser(fun settings ->
                            settings.IgnoreUnknownArguments <- false
                        )

let hasAttribute<'t when 't :> Attribute> (t:MemberInfo) =
    let a = t.GetCustomAttribute<'t>()
    not (obj.ReferenceEquals(a, null))

let isModule (t:Type) =     
    match t.GetCustomAttribute<CompilationMappingAttribute> () with
    | a when obj.ReferenceEquals(a, null) -> false
    | a -> a.SourceConstructFlags = SourceConstructFlags.Module

let extractArgs (t: Type) =       
    t.GetMember("Args") 
    |> Seq.tryHead
    |> Option.filter (hasAttribute<CommandLine.VerbAttribute>)     
    |> Option.map (fun t -> t :?> Type)   
        

let parse (args: string[]) =
    let argTypes =  
        Reflection.Assembly.GetExecutingAssembly().GetTypes()          
        |> Seq.filter isModule
        |> Seq.map extractArgs
        |> Seq.filter Option.isSome
        |> Seq.map Option.get       
        |> Seq.toArray

    let result = parser.ParseArguments(args, argTypes)
    match result with    
    | :? NotParsed<obj> as p -> Error(CommandLine.Text.HelpText.AutoBuild(p))
    | :? Parsed<obj> as p -> ExecuteCommand(p.Value)
