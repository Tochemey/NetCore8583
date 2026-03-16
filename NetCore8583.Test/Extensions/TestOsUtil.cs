using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test.Extensions
{
    public class TestOsUtil
    {
        [Fact]
        public void IsLinuxReturnsBoolWithoutThrowing()
        {
            // Just verify it runs and returns a bool without throwing.
            // The value depends on the platform running the test.
            var result = OsUtil.IsLinux();
            Assert.IsType<bool>(result);
        }

        [Fact]
        public void IsLinuxMatchesPlatform()
        {
            // OsUtil.IsLinux() returns true for any Unix-like platform (Linux, macOS)
            // by checking Environment.OSVersion.Platform against Unix platform IDs (4, 6, 128).
            var p = (int) System.Environment.OSVersion.Platform;
            var expectedUnixLike = p is 4 or 6 or 128;
            Assert.Equal(expectedUnixLike, OsUtil.IsLinux());
        }
    }
}
