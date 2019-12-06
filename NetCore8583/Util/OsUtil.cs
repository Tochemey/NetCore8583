using System;

namespace NetCore8583.Util
{
    public static class OsUtil
    {
        /// <summary>
        ///     check the version of OS at runtime
        /// </summary>
        /// <returns><c>true</c>, if linux was ised, <c>false</c> otherwise.</returns>
        public static bool IsLinux()
        {
            var p = (int) Environment.OSVersion.Platform;
            return p == 4 || p == 6 || p == 128;
        }
    }
}