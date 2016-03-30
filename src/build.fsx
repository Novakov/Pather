#r "packages/FAKE.Core/tools/FakeLib.dll"
#r "packages/ILRepack.Lib/lib/net40/ILRepack.dll"

open Fake
open Fake.Testing.XUnit2

let binaryDir = currentDirectory @@ ".." @@ "bin"

let configuration = getBuildParamOrDefault "Configuration" "Release"

let runTests errorLevel =
    let setParams (defaults:XUnit2Params) =
        { defaults with
            ErrorLevel = errorLevel
        }

    !! ("Tests/bin" @@ configuration @@ "Tests.dll")
      |> xUnit2 setParams

Target "Build" (fun _ ->
    let setParams (defaults:MSBuildParams) =
          { defaults with
              Verbosity = Some(Quiet)
              NoLogo = true
              Properties = 
              [
                ("Configuration", configuration)
              ]
          }

    build setParams "Pather.sln"
)

Target "Pack" (fun _ ->    
    let options = new ILRepacking.RepackOptions()    

    let bin = currentDirectory @@ "Pather" @@ "bin" @@ configuration

    options.OutputFile <- binaryDir @@ "Pather.exe"

    options.InputAssemblies <- [|
        bin @@ "Pather.exe"
        bin @@ "*.dll"
    |]    

    options.SearchDirectories <- [ bin ]
    options.AllowWildCards <- true
    options.Internalize <- true
    options.DebugInfo <- true    

    let repack = ILRepacking.ILRepack(options)

    repack.Repack()
)


Target "RunTests" (fun _ -> runTests Error)

Target "Default" ignore

Target "WatchTests" (fun _ ->
    use watcher = !! "Tests/bin/Debug/Tests.dll" |> WatchChanges (fun changes ->
        if changes |> Seq.exists (fun i -> i.Status = FileStatus.Created || i.Status = Changed) then
            runTests DontFailBuild
    )

    System.Console.ReadLine() |> ignore

    watcher.Dispose() |> ignore
)

"Build" ==> "RunTests"

"Build" ==> "Pack" ==> "Default"

"RunTests" ==> "Default"

RunTargetOrDefault "Default"
