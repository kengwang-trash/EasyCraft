@echo off
echo Cleaning Output Directory
dotnet clean -c win64-native
echo Restore NuGet
dotnet restore 
echo Generating Windows x64 Native
dotnet publish -r win-x64 -c win64-native -v d
pause