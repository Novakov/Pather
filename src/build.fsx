#r "packages/FAKE.Core/tools/FakeLib.dll"

open Fake
open Fake.Testing.XUnit2

let runTests errorLevel =
    let setParams (defaults:XUnit2Params) =
        { defaults with
            ErrorLevel = errorLevel
        }

    !! "Tests/bin/Debug/Tests.dll"
      |> xUnit2 setParams

Target "Build" (fun _ ->
    let setParams (defaults:MSBuildParams) =
          { defaults with
              Verbosity = Some(Quiet)
              NoLogo = true
          }

    build setParams "Pather.sln"
)


Target "RunTests" (fun _ -> runTests Error)

Target "WatchTests" (fun _ ->
    use watcher = !! "Tests/bin/Debug/Tests.dll" |> WatchChanges (fun changes ->
        if changes |> Seq.exists (fun i -> i.Status = FileStatus.Created || i.Status = Changed) then
            runTests DontFailBuild
    )

    System.Console.ReadLine() |> ignore

    watcher.Dispose() |> ignore
)

"Build"
==> "RunTests"

RunTargetOrDefault "RunTests"
