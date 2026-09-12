# Publish Scripts
These scripts are used to publish the application with `dotnet publish`. 
They are made to be used from this directory ([RetroLinker.Desktop/Scripts](.)), and execute the publish command on [RetroLinker.Desktop.csproj](../RetroLinker.Desktop.csproj).
The binaries are build onto `{repo}/Release/{release_name}`.

### publish_aot
Publish with the Ahead-of-Time .NET feature, which, aside of skipping the .NET Runtime requirement, it gives a little performance boost and blazing fast start up time. 
It also publishes all the .NET libraries in a single executable file (The only files left are the C++ native libraries from SkiaSharp and Magick.NET).

The `.bat` publishes for Windows, and `.sh` publishes for Linux (glibc), both x86_64.

### publish_net
Publishes without any `publish` parameters, creating a portable application that can run on any .NET 10 out there...
Or it could, but SkiaSharp and Magick.NET restricts the available platforms by having native C++ libraries.
It is used to create the `_net` release.

The `.bat` publishes from Windows CMD, and `.sh` publishes from any POXIS compliant shell.