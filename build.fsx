#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open System
open System.Diagnostics
open System.IO
open System.Text
open System.Text.RegularExpressions

type TestRunResult =
 { TestCount:int; Passed:int; Ignored:int; Failed:int }

let parseWith (pattern:string) (f:Match->'t option) (log:string) =
  let opts = RegexOptions.Compiled ||| RegexOptions.IgnorePatternWhitespace
  let testsRegex = Regex(pattern, opts)
  let m = testsRegex.Match log
  if m.Success then f m else None

let parseTestRun =
  parseWith
    @"EXPECTO! \s (?<total>\d+) \s tests \s run .* - \s (?<passed>\d+) \s passed .* (?<ignored>\d+) \s ignored .* (?<failed>\d+) \s failed"
    (fun m -> 
      let getCount (name:string) =
        let g = m.Groups
        g.[name].Value |> Convert.ToInt32
      { TestCount = getCount "total"
        Passed = getCount "passed"
        Ignored = getCount "ignored"
        Failed = getCount "failed" } |> Some)
let isFailedTest =
  parseWith @"\[.* ERR\].*" (fun m -> Some())

let testFiles = !! "AsciiArtKata.Tests/*.fsx"
let fileIncludes = testFiles ++ "**/*.fs"

Target "Watch" (fun _ ->
    let run file =
      let testFile = getBuildParamOrDefault "file" file
      printfn "Running %s" file
      Fake.FSIHelper.executeFSI __SOURCE_DIRECTORY__ testFile []
      |> snd
      |> Seq.iter
          (fun l -> 
              match parseTestRun l.Message, isFailedTest l.Message with
              | Some result, _ when result.Failed > 0 -> traceError l.Message
              | _, Some _ -> traceError l.Message
              | _ -> trace l.Message)

    let runAllTests () =
      testFiles |> Seq.iter run

    let showAtEnd () = printfn "Press any key to quit ..."

    use watcher =
        fileIncludes
        |> WatchChanges (fun changes -> 
          changes |> Seq.iter (
            fun c -> 
              if testFiles.IsMatch c.FullPath
              then c.FullPath |> run
              else runAllTests ()
          )
          showAtEnd ())

    runAllTests()
    showAtEnd ()
    ignore <| Console.ReadKey true
)

Target "RestorePackages" (fun _ ->
    RestorePackages()
)

Target "Clean" (fun _ ->
    ensureDirectory "bin"
    CleanDirs ["bin"]
)

Target "Build" (fun _ ->
  !! "AsciiArtKata.sln"
  |> MSBuildRelease "" "Rebuild"
  |> ignore
)

Target "CopyBinaries" (fun _ ->
    !! "**/*.??proj"
    -- "**/*.shproj"
    |>  Seq.map (fun f -> ((System.IO.Path.GetDirectoryName f) @@ "bin/Release", "bin" @@ (System.IO.Path.GetFileNameWithoutExtension f)))
    |>  Seq.iter (fun (fromDir, toDir) -> CopyDir toDir fromDir (fun _ -> true))
)

Target "BuildAll" DoNothing

"RestorePackages"
  ==> "Clean"
  ==> "Build"
  ==> "CopyBinaries"
  ==> "BuildAll"

RunTargetOrDefault "BuildAll"
