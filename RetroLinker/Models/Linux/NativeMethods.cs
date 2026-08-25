using System.Runtime.InteropServices;

namespace RetroLinker.Models.Linux;

#if !WINDOWS
internal static partial class NativeMethods
{
    [LibraryImport("libc")]
    public static partial uint geteuid();
}

public static class NativeAccess
{
    private const uint RootUID = 0;
    public static bool IsUnixProcessElevated() => NativeMethods.geteuid() == RootUID;
}
#endif