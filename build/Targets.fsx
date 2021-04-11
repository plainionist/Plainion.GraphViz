// load dependencies from source folder to allow bootstrapping
#r "/bin/Plainion.CI/Fake.Core.Target.dll"
#r "/bin/Plainion.CI/Fake.IO.FileSystem.dll"
#r "/bin/Plainion.CI/Fake.IO.Zip.dll"
#r "/bin/Plainion.CI/Plainion.CI.Tasks.dll"

open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Plainion.CI

Target.create "CreatePackage" (fun _ ->
    !! ( outputPath </> "*.*Tests.*" )
    ++ ( outputPath </> "*nunit*" )
    ++ ( outputPath </> "*Moq*" )
    ++ ( outputPath </> "TestResult.xml" )
    ++ ( outputPath </> "**/*.pdb" )
    |> File.deleteAll

    PZip.PackRelease()
)

Target.create "Deploy" (fun _ ->
    let releaseDir = @"\bin\Plainion.GraphViz"

    Shell.cleanDir releaseDir

    let zip = PZip.GetReleaseFile()
    Zip.unzip releaseDir zip
)

Target.create "Publish" (fun _ ->
    let zip = PZip.GetReleaseFile()
    PGitHub.Release [ zip ]
)

Target.runOrDefault ""
