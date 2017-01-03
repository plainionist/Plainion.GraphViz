// load dependencies from source folder to allow bootstrapping
#r "/bin/Plainion.CI/FAKE/FakeLib.dll"
#load "/bin/Plainion.CI/bits/PlainionCI.fsx"

open Fake
open PlainionCI

Target "CreatePackage" (fun _ ->
    !! ( outputPath </> "*.*Tests.*" )
    ++ ( outputPath </> "*nunit*" )
    ++ ( outputPath </> "*Moq*" )
    ++ ( outputPath </> "TestResult.xml" )
    ++ ( outputPath </> "**/*.pdb" )
    |> DeleteFiles

    PZip.PackRelease()
)

Target "DeployPackage" (fun _ ->
    let releaseDir = @"\bin\Plainion.GraphViz"

    CleanDir releaseDir

    let zip = PZip.GetReleaseFile()
    Unzip releaseDir zip

    PGitHub.Release [ zip ]
)

RunTarget()
