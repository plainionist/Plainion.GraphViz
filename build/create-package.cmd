@echo off
setlocal

set PATH=%PATH%;C:\Program Files (x86)\MSBuild\12.0\Bin
set PATH=%PATH%;C:\Windows\Microsoft.NET\Framework64\v4.0.30319

msbuild /verbosity:normal /p:Version=1.9.0.0 packager.msbuild

pause
		
endlocal