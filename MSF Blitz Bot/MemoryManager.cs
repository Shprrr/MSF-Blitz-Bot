using System;
using System.Runtime.InteropServices;

namespace MSFBlitzBot
{
    public static class MemoryManager
    {
        public static IntPtr Alloc(int cb)
        {
            return Marshal.AllocHGlobal(cb);
        }

        public static void Free(IntPtr ptr)
        {
            if (!(ptr == IntPtr.Zero))
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
