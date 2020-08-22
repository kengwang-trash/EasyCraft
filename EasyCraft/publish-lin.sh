#!/bin/bash
echo Cleaning Output Directory
dotnet clean -c lin64-native
echo Restore NuGet
dotnet restore
echo Generating Linux x64 Native
dotnet publish -r linux-x64 -c linux64-native