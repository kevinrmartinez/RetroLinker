cd..
dotnet publish --nologo -o ..\Release\RetroLinker_win-x64 -c Release -r "win-x64" -p:PublishAot=true -p:DebugSymbols=false
