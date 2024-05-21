using System.Runtime.InteropServices;

namespace SpinBladeArena.Performance;

public partial class Native
{
    [LibraryImport("winmm.dll", SetLastError = true)]
    public static partial uint timeBeginPeriod(uint uPeriod);

    [LibraryImport("winmm.dll", SetLastError = true)]
    public static partial uint timeEndPeriod(uint uPeriod);
}
