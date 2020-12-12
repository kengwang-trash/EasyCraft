@echo off
echo Cleaning Output Directory
dotnet clean -c Windows-Release
::echo Restore NuGet
::dotnet restore 
echo Generating Windows x86 Native
dotnet publish -f netcoreapp3.1 -r win-x86 -c Windows-Release -v d
pause
