#!/bin/env sh
cd ..
dotnet publish --nologo -o ../Release/RetroLinker_linux-x64 -c ReleaseAoT -r linux-x64
