#r @"packages\build\FAKE\tools\FakeLib.dll"
open Fake
open Fake.Paket
open Fake.Testing
open Fake.AssemblyInfoFile
open Fake.DotCover

Restore id

Target "Clean" (fun _ ->
    for dir in !! "**/bin/" ++ "**/obj/"  do
        CleanDir dir
)

Target "Build" (fun _ ->
    !! "*.sln"
    |> MSBuildRelease "" "Build"
    |> Log "Build-Output: "
)
Target "Test" (fun _ ->
    !! "**/bin/release/Tests.dll"
    |> Seq.distinct
    |> xUnit2 (fun x -> { x with Parallel = ParallelMode.All })
)
Target "Cover" (fun _ ->
    !! "**/bin/release/Tests.dll"
        |> DotCoverXUnit2 
            (fun dotCoverOptions -> dotCoverOptions)
            (fun xUnitOptions -> {xUnitOptions with Parallel = ParallelMode.All}) 
)

Target "Benchmark" (fun _ ->
    for file in !! "**/bin/release/*.Benchmarks.exe" do
        ExecProcessWithLambdas  
            (fun si -> si.FileName <- file) 
            (System.TimeSpan.FromMinutes(10.0))  //time out
            false log log // silent error message
        |> ignore)

Target "ResharperInspect" (fun _ ->
    ExecProcessWithLambdas (fun si -> 
        si.FileName <- @"Packages\build\JetBrains.ReSharper.CommandLineTools\tools\inspectcode.exe"
        si.Arguments <- """Sample.Web.sln --swea --output=\"resharperInspectionResult.xml" """) 
        (System.TimeSpan.FromMinutes(10.0))  //time out
        false log log // silent error message
    |> ignore)

Target "Pack" (fun _ ->
    Pack  (fun cfg ->
       { cfg with Symbols = false; OutputPath = "." }
    ))

Target "Push" (fun _ ->
    Push (fun cfg -> 
        { cfg with 
            PublishUrl = "???"
            ApiKey = "???"}))

Target "Analyse" (fun _ ->
    log "analyse"
)

"Clean"
==> "Build"
==> "Test"
==> "Pack"
==> "Push"

"Clean"
==> "Build"
==> "Benchmark"
==> "Cover"
==> "ResharperInspect"
==> "Analyse"

RunTargetOrDefault "Pack"