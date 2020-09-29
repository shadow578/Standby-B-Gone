using System;
using System.Runtime.InteropServices;

namespace StandbyBGone
{
    /// <summary>
    /// provides native functions for windows
    /// </summary>
    public static class Native
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [Flags]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001,

            [Obsolete("Legacy flag, should not be used")]
            ES_USER_PRESENT = 0x00000004
        }

    }
}
