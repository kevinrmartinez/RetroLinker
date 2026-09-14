using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace RetroLinker.Models.Linux;

internal static partial class NativeMethods {
    [UnsupportedOSPlatform(App.PlatformWin)]
    [LibraryImport("libc")]
    public static partial uint geteuid();
}

public static class NativeAccess {
    private const uint RootUID = 0;
    [UnsupportedOSPlatform(App.PlatformWin)]
    public static bool IsUnixProcessElevated() => NativeMethods.geteuid() == RootUID;
}
