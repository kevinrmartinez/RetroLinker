#!/bin/env sh
cd ..
dotnet publish --nologo -o ../Release/RetroLinker_linux-x64 -c Release -r linux-x64 -p:PublishAot=true -p:DebugSymbols=false
