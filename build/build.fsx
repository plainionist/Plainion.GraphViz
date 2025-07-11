
open System.IO
open System.Diagnostics

let home = Path.Combine(__SOURCE_DIRECTORY__, "..") |> Path.GetFullPath
let binFolder = Path.Combine(home, "bin", "Release")

let exec exe (args:string) =
    let info = ProcessStartInfo(exe, args)
    info.WorkingDirectory <- home
    Process.Start(info).WaitForExit()

let delete pattern = 
    Directory.GetFiles(binFolder, pattern) |> Seq.iter File.Delete


exec "dotnet" "build -c Release"

delete "*.Tests.*"
delete "NUnit*"
delete ".msCoverage*"



