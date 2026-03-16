using System;
using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test.Extensions
{
    public class TestBitmap128
    {
        [Fact]
        public void DefaultBitmapIsAllZeros()
        {
            var bm = new Bitmap128();
            Assert.False(bm.Any());
            Assert.Equal(0, bm.PopCount());
        }

        [Fact]
        public void SetAndGetAllBitPositions()
        {
            for (var i = 0; i < 128; i++)
            {
                var bm = new Bitmap128();
                bm.Set(i, true);
                Assert.True(bm.Get(i), $"bit {i} should be set");
                Assert.False(bm.Get(i == 0 ? 1 : 0), $"bit 0 or 1 should not be set");
            }
        }

        [Fact]
        public void ClearResetsAllBits()
        {
            var bm = new Bitmap128();
            bm.Set(0, true);
            bm.Set(64, true);
            bm.Set(127, true);
            bm.Clear();
            Assert.False(bm.Any());
            Assert.Equal(0, bm.PopCount());
        }

        [Fact]
        public void AnyReturnsTrueWhenBitSet()
        {
            var bm = new Bitmap128();
            bm.Set(63, true);
            Assert.True(bm.Any());
        }

        [Fact]
        public void PopCountCounts()
        {
            var bm = new Bitmap128();
            bm.Set(0, true);
            bm.Set(7, true);
            bm.Set(64, true);
            bm.Set(127, true);
            Assert.Equal(4, bm.PopCount());
        }

        [Fact]
        public void OrCombinesBits()
        {
            var a = new Bitmap128();
            a.Set(0, true);
            a.Set(10, true);

            var b = new Bitmap128();
            b.Set(10, true);
            b.Set(20, true);

            a.Or(b);
            Assert.True(a.Get(0));
            Assert.True(a.Get(10));
            Assert.True(a.Get(20));
            Assert.Equal(3, a.PopCount());
        }

        [Fact]
        public void AndMasksBits()
        {
            var a = new Bitmap128();
            a.Set(0, true);
            a.Set(10, true);
            a.Set(20, true);

            var b = new Bitmap128();
            b.Set(10, true);
            b.Set(20, true);
            b.Set(30, true);

            a.And(b);
            Assert.False(a.Get(0));
            Assert.True(a.Get(10));
            Assert.True(a.Get(20));
            Assert.False(a.Get(30));
            Assert.Equal(2, a.PopCount());
        }

        [Fact]
        public void XorTogglesBits()
        {
            var a = new Bitmap128();
            a.Set(5, true);
            a.Set(10, true);

            var b = new Bitmap128();
            b.Set(10, true);
            b.Set(15, true);

            a.Xor(b);
            Assert.True(a.Get(5));
            Assert.False(a.Get(10));
            Assert.True(a.Get(15));
        }

        [Fact]
        public void ToUInt128AndFromUInt128RoundTrip()
        {
            var bm = new Bitmap128();
            bm.Set(0, true);
            bm.Set(63, true);
            bm.Set(127, true);

            var val = bm.ToUInt128();
            var restored = Bitmap128.FromUInt128(val);

            Assert.True(restored.Get(0));
            Assert.True(restored.Get(63));
            Assert.True(restored.Get(127));
            Assert.Equal(3, restored.PopCount());
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(128)]
        [InlineData(200)]
        public void OutOfRangeIndexThrows(int index)
        {
            var bm = new Bitmap128();
            Assert.Throws<IndexOutOfRangeException>(() => bm.Get(index));
            Assert.Throws<IndexOutOfRangeException>(() => bm.Set(index, true));
        }

        [Fact]
        public void IndexerGetAndSetMatchGetSet()
        {
            var bm = new Bitmap128();
            bm[42] = true;
            Assert.True(bm[42]);
            Assert.True(bm.Get(42));
            bm[42] = false;
            Assert.False(bm[42]);
        }
    }
}
