#r "packages/FAKE.Core/tools/FakeLib.dll"

open Fake
open Fake.Testing.XUnit2

Target "Build" (fun _ ->
    let setParams (defaults:MSBuildParams) =
          { defaults with
              Verbosity = Some(Quiet)
              NoLogo = true
          }

    build setParams "Pather.sln"
)

Target "RunTests" (fun _ ->
    let setParams (defaults:XUnit2Params) =
        { defaults with
            ErrorLevel = Error
        }

    !! "Tests/bin/Debug/Tests.dll"
      |> xUnit2 setParams
)

Target "Watch" (fun _ ->
    log "Starting watcher!"

    use watcher = !! "**/*.fs" |> WatchChanges (fun changes ->
        Run "RunTests"
    )

    System.Console.ReadLine() |> ignore

    watcher.Dispose() |> ignore
)

"Build"
==> "RunTests"

RunTargetOrDefault "RunTests"
