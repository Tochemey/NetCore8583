using System;
using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test.Extensions
{
    public class TestArrays
    {
        [Fact]
        public void FillSetsValuesInRange()
        {
            var arr = new int[5];
            Arrays.Fill(arr, 1, 3, 99);
            Assert.Equal(new[] { 0, 99, 99, 99, 0 }, arr);
        }

        [Fact]
        public void FillFromZero()
        {
            var arr = new string[4];
            Arrays.Fill(arr, 0, 3, "x");
            Assert.Equal(new[] { "x", "x", "x", null }, arr);
        }

        [Fact]
        public void FillNullArrayThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Arrays.Fill<int>(null, 0, 1, 0));
        }

        [Fact]
        public void FillNegativeCountThrows()
        {
            var arr = new int[5];
            Assert.Throws<ArgumentOutOfRangeException>(() => Arrays.Fill(arr, 0, -1, 0));
        }

        [Fact]
        public void FillOutOfRangeCountThrows()
        {
            var arr = new int[5];
            Assert.Throws<ArgumentOutOfRangeException>(() => Arrays.Fill(arr, 0, 5, 0));
        }
    }
}
