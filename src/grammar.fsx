#r "packages/FAKE.Core/tools/FakeLib.dll"
#r "packages/Antlr4.Runtime/lib/net45/Antlr4.Runtime.net45.dll"
#r "Grammar/bin/Debug/Grammar.dll"

open Fake

Target "PathsFile" (fun _ ->
//    let buildParams (defaults:MSBuildParams) =
//        { defaults with
//            Verbosity = Some(Minimal)
//        }
//
//    build buildParams "Grammar\Grammar.csproj"

    let input = ""

    let lexer = Grammar.PathsFileLexer()

    ()
)

RunTarget ()