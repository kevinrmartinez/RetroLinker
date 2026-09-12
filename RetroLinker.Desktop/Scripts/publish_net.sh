#!/bin/env sh
cd ..
dotnet publish --nologo -c Release -o ../Release/RetroLinker_net -p:DebugSymbols=false