// include Fake lib
#r @"packages\FAKE\tools\FakeLib.dll"
open System
open System.IO
open System.Text

open Fake

RestorePackages()

let copyright = "Copyright © 2013-2015 Akka.NET Team"
let authors = [ "Akka.NET Team" ]

let binDir = "bin"
let testOutput = FullName "TestResults"

let nugetDir = binDir @@ "nuget"
let workingDir = binDir @@ "build"
let libDir = workingDir @@ @"lib\net45\"
let nugetExe = FullName @".nuget\NuGet.exe"
let docDir = "bin" @@ "doc"

let testDir  = binDir @@ "test"


let parsedRelease =
    File.ReadLines "RELEASE_NOTES.md"
    |> ReleaseNotesHelper.parseReleaseNotes

let envBuildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER")
let buildNumber = if String.IsNullOrWhiteSpace(envBuildNumber) then "0" else envBuildNumber

let version = parsedRelease.AssemblyVersion + "." + buildNumber
let preReleaseVersion = version + "-beta"

let isPreRelease = hasBuildParam "nugetprerelease"
let release = if isPreRelease then ReleaseNotesHelper.ReleaseNotes.New(version, version + "-beta", parsedRelease.Notes) else parsedRelease

printfn "Assembly version: %s\nNuget version; %s\n" release.AssemblyVersion release.NugetVersion


//--------------------------------------------------------------------------------
// Build
//--------------------------------------------------------------------------------

// Clean build directory
Target "Clean" (fun _ ->
    CleanDir binDir
)

// Build the whole solution
Target "Build" (fun _ ->
    !!"src/Hocon.sln"
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

// Copy the build output to project bin directories
Target "CopyOutput" <| fun _ ->

    let copyOutput project =
        let src = "src" @@ project @@ @"bin/Release/"
        let dst = binDir @@ project
        CopyDir dst src allFiles
    [ "core/Hocon"
      "core/Hocon.Tests"
      ]
    |> List.iter copyOutput

Target "BuildRelease" DoNothing



//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

// Clean test output
Target "CleanTests" <| fun _ ->
    DeleteDir testOutput

// Run tests
Target "RunTests" (fun _ ->
!! ("src/**/bin/Release/Hocon.Tests.dll")
    |> NUnit (fun p ->
        {p with
            DisableShadowCopy = true;
            OutputFile = testDir + "TestResults.xml" })
)



//--------------------------------------------------------------------------------
// Nuget
//--------------------------------------------------------------------------------


module Nuget = 
    // add Akka dependency for other projects
    let getAkkaDependency project =
        ["Newtonsoft.Json", GetPackageVersion "./packages/" "Newtonsoft.Json"]

open Nuget
open Fake.TaskRunnerHelper


// Clean nuget directory
Target "CleanNuget" <| fun _ ->
    CleanDir nugetDir

// Pack nuget
let createNugetPackages _ =
    let removeDir dir = 
        let del _ = 
            DeleteDir dir
            not (directoryExists dir)
        runWithRetries del 3 |> ignore

    CleanDir nugetDir
               
    let nuspec = "src/core/Hocon/Akka.Hocon.nuspec"

    let project = Path.GetFileNameWithoutExtension nuspec 
    let projectDir = Path.GetDirectoryName nuspec
    let projectFile = (!! (projectDir @@ project + ".*sproj")) |> Seq.head
    let releaseDir = projectDir @@ @"bin\Release"

    let tags = ["hocon";"hocon.net"]

    ensureDirectory libDir
    !! (releaseDir @@ project + ".dll")
    ++ (releaseDir @@ project + ".pdb")
    ++ (releaseDir @@ project + ".xml")
    ++ (releaseDir @@ project + ".ExternalAnnotations.xml")
    |> CopyFiles libDir


    let nugetSrcDir = workingDir @@ @"src/"

    let isCs = hasExt ".cs"
    let isAssemblyInfo f = (filename f).Contains("AssemblyInfo")
    let isSrc f = isCs f && not (isAssemblyInfo f) 
    CopyDir nugetSrcDir projectDir isSrc

    let nugetSrcDir = workingDir @@ @"src/"
    removeDir (nugetSrcDir @@ "obj")
    removeDir (nugetSrcDir @@ "bin")

    ensureDirectory nugetDir

    NuGet (fun p -> 
        {p with
            Authors = authors
            Copyright = copyright
            Project =  project
            Properties = ["Configuration", "Release"]
            ReleaseNotes = release.Notes |> String.concat "\n"
            Version = release.NugetVersion
            Tags = tags |> String.concat " "
            OutputPath = nugetDir
            SymbolPackage = NugetSymbolPackage.Nuspec
            Summary = "summary"
            WorkingDir = workingDir
             }) 
        nuspec

    trace "remove working dir"
    removeDir workingDir



let publishNugetPackages _ = 
    let rec publishPackage url accessKey trialsLeft packageFile =
        let tracing = enableProcessTracing
        enableProcessTracing <- false
        let args p =
            match p with
            | (pack, key, "") -> sprintf "push \"%s\" %s" pack key
            | (pack, key, url) -> sprintf "push \"%s\" %s -source %s" pack key url

        tracefn "Pushing %s Attempts left: %d" (FullName packageFile) trialsLeft
        try 
            let result = ExecProcess (fun info -> 
                    info.FileName <- nugetExe
                    info.WorkingDirectory <- (Path.GetDirectoryName (FullName packageFile))
                    info.Arguments <- args (packageFile, accessKey,url)) (System.TimeSpan.FromMinutes 1.0)
            enableProcessTracing <- tracing
            if result <> 0 then failwithf "Error during NuGet symbol push. %s %s" nugetExe (args (packageFile, "key omitted",url))
        with exn -> 
            if (trialsLeft > 0) then (publishPackage url accessKey (trialsLeft-1) packageFile)
            else raise exn


    let shouldPushNugetPackages = hasBuildParam "nugetkey"
    let shouldPushSymbolsPackages = (hasBuildParam "symbolspublishurl") && (hasBuildParam "symbolskey")
    
    if (shouldPushNugetPackages || shouldPushSymbolsPackages) then
        printfn "Pushing nuget packages"

        if shouldPushNugetPackages then
            let normalPackages= 
                !! (nugetDir @@ "*.nupkg") 
                -- (nugetDir @@ "*.symbols.nupkg") |> Seq.sortBy(fun x -> x.ToLower())
            
            for package in normalPackages do
                publishPackage (getBuildParamOrDefault "nugetpublishurl" "") (getBuildParam "nugetkey") 3 package

        if shouldPushSymbolsPackages then
            let symbolPackages= !! (nugetDir @@ "*.symbols.nupkg") |> Seq.sortBy(fun x -> x.ToLower())
            
            for package in symbolPackages do
                publishPackage (getBuildParam "symbolspublishurl") (getBuildParam "symbolskey") 3 package


Target "Nuget" <| fun _ -> 
    createNugetPackages()
    publishNugetPackages()

Target "CreateNuget" <| fun _ -> 
    createNugetPackages()

Target "PublishNuget" <| fun _ -> 
    publishNugetPackages()



//--------------------------------------------------------------------------------
// Help 
//--------------------------------------------------------------------------------

Target "Help" <| fun _ ->
    List.iter printfn [
      "usage:"
      "build [target]"
      ""
      " Targets for building:"
      " * BuildRelease   Builds"
      " * Nuget          Create and optionally publish nugets packages"
      " * RunTests       Runs tests"
      ""
      " Other Targets"
      " * Help       Display this help" 
      " * HelpNuget  Display help about creating and pushing nuget packages" 
      ""]


Target "HelpNuget" <| fun _ ->
    List.iter printfn [
      "usage: "
      "build Nuget [nugetkey=<key> [nugetpublishurl=<url>]] "
      "            [symbolskey=<key> symbolspublishurl=<url>] "
      "            [nugetprerelease=<prefix>]"
      ""
      "Arguments for Nuget target:"
      "   nugetprerelease=<prefix>   Creates a pre-release package."
      "                              The version will be version-prefix<date>"
      "                              Example: nugetprerelease=dev =>"
      "                                       0.6.3-dev1408191917"
      ""
      "In order to publish a nuget package, keys must be specified."
      "If a key is not specified the nuget packages will only be created on disk"
      "After a build you can find them in bin/nuget"
      ""
      "For pushing nuget packages to nuget.org and symbols to symbolsource.org"
      "you need to specify nugetkey=<key>"
      "   build Nuget nugetKey=<key for nuget.org>"
      ""
      "For pushing the ordinary nuget packages to another place than nuget.org specify the url"
      "  nugetkey=<key>  nugetpublishurl=<url>  "
      ""
      "For pushing symbols packages specify:"
      "  symbolskey=<key>  symbolspublishurl=<url> "
      ""
      "Examples:"
      "  build Nuget                      Build nuget packages to the bin/nuget folder"
      ""
      "  build Nuget nugetprerelease=dev  Build pre-release nuget packages"
      ""
      "  build Nuget nugetkey=123         Build and publish to nuget.org and symbolsource.org"
      ""
      "  build Nuget nugetprerelease=dev nugetkey=123 nugetpublishurl=http://abc"
      "              symbolskey=456 symbolspublishurl=http://xyz"
      "                                   Build and publish pre-release nuget packages to http://abc"
      "                                   and symbols packages to http://xyz"
      ""]


//--------------------------------------------------------------------------------
//  Target dependencies
//--------------------------------------------------------------------------------

// build app
"Clean" ==> "Build" ==> "CopyOutput" ==> "BuildRelease"

// run tests
"CleanTests" ==> "RunTests"

// nugetify
"CleanNuget" ==> "CreateNuget"
"CleanNuget" ==> "Build" ==> "Nuget"

// start build
RunTargetOrDefault "Help"