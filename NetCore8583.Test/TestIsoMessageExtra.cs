// MIT License
//
// Copyright (c) 2020 - 2026 Arsene Tochemey Gandote
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NetCore8583.Test
{
    /// <summary>
    /// Additional IsoMessage tests targeting HasEveryField, HasAnyField, CopyFieldsFrom,
    /// RemoveFields, DebugString, Write/WriteToBuffer with length bytes, binary mode,
    /// BinBitmap, EnforceSecondBitmap, and SetField/SetValue edge cases.
    /// </summary>
    public class TestIsoMessageExtra
    {
        private static IsoMessage BuildMessage()
        {
            var msg = new IsoMessage { Encoding = Encoding.ASCII };
            msg.Type = 0x0200;
            msg.SetValue(3, "650000", null, IsoType.NUMERIC, 6);
            msg.SetValue(32, "123", null, IsoType.LLVAR, 0);
            msg.SetValue(49, "484", null, IsoType.ALPHA, 3);
            return msg;
        }

        // ── HasEveryField ─────────────────────────────────────────────────────────

        [Fact]
        public void HasEveryField_AllPresent_ReturnsTrue()
        {
            var msg = BuildMessage();
            Assert.True(msg.HasEveryField(3, 32, 49));
        }

        [Fact]
        public void HasEveryField_OneMissing_ReturnsFalse()
        {
            var msg = BuildMessage();
            Assert.False(msg.HasEveryField(3, 32, 99));
        }

        [Fact]
        public void HasEveryField_AllMissing_ReturnsFalse()
        {
            var msg = new IsoMessage();
            Assert.False(msg.HasEveryField(2, 3, 4));
        }

        // ── HasAnyField ───────────────────────────────────────────────────────────

        [Fact]
        public void HasAnyField_OnePresent_ReturnsTrue()
        {
            var msg = BuildMessage();
            Assert.True(msg.HasAnyField(3, 99, 100));
        }

        [Fact]
        public void HasAnyField_NonePresent_ReturnsFalse()
        {
            var msg = BuildMessage();
            Assert.False(msg.HasAnyField(10, 20, 99));
        }

        // ── CopyFieldsFrom ────────────────────────────────────────────────────────

        [Fact]
        public void CopyFieldsFrom_CopiesExistingFields()
        {
            var source = BuildMessage();
            var dest = new IsoMessage { Encoding = Encoding.ASCII, Type = 0x0210 };
            dest.CopyFieldsFrom(source, 3, 32);
            Assert.True(dest.HasField(3));
            Assert.True(dest.HasField(32));
            Assert.False(dest.HasField(49));
            Assert.Equal("650000", dest.GetObjectValue(3));
        }

        [Fact]
        public void CopyFieldsFrom_SkipsMissingFields()
        {
            var source = BuildMessage(); // has 3, 32, 49
            var dest = new IsoMessage { Encoding = Encoding.ASCII, Type = 0x0210 };
            // 99 doesn't exist in source — should not throw
            dest.CopyFieldsFrom(source, 3, 99);
            Assert.True(dest.HasField(3));
            Assert.False(dest.HasField(99));
        }

        // ── RemoveFields ──────────────────────────────────────────────────────────

        [Fact]
        public void RemoveFields_ClearsSpecifiedFields()
        {
            var msg = BuildMessage();
            Assert.True(msg.HasField(3));
            Assert.True(msg.HasField(32));
            msg.RemoveFields(3, 32);
            Assert.False(msg.HasField(3));
            Assert.False(msg.HasField(32));
            Assert.True(msg.HasField(49)); // untouched
        }

        // ── DebugString ───────────────────────────────────────────────────────────

        [Fact]
        public void DebugString_AsciiHeader_IncludesHeader()
        {
            var msg = new IsoMessage("ISO015000050") { Encoding = Encoding.ASCII };
            msg.Type = 0x0200;
            msg.SetValue(49, "484", null, IsoType.ALPHA, 3);
            var s = msg.DebugString();
            Assert.StartsWith("ISO015000050", s);
            Assert.Contains("0200", s);
        }

        [Fact]
        public void DebugString_BinaryHeader_IncludesHexHeader()
        {
            var binHeader = new sbyte[] { 0x01, 0x02, 0x03 };
            var msg = new IsoMessage(binHeader) { Encoding = Encoding.ASCII };
            msg.Type = 0x0800;
            var s = msg.DebugString();
            Assert.StartsWith("[0x", s);
            Assert.Contains("010203", s);
        }

        [Fact]
        public void DebugString_WithLlvar_IncludesLengthPrefix()
        {
            var msg = new IsoMessage { Encoding = Encoding.ASCII };
            msg.Type = 0x0200;
            msg.SetValue(32, "ABC", null, IsoType.LLVAR, 0);
            var s = msg.DebugString();
            Assert.Contains("03ABC", s);
        }

        [Fact]
        public void DebugString_WithLllvar_IncludesThreeDigitPrefix()
        {
            var msg = new IsoMessage { Encoding = Encoding.ASCII };
            msg.Type = 0x0200;
            msg.SetValue(48, "DATA", null, IsoType.LLLVAR, 0);
            var s = msg.DebugString();
            Assert.Contains("004DATA", s);
        }

        [Fact]
        public void DebugString_WithLlllvar_IncludesFourDigitPrefix()
        {
            var msg = new IsoMessage { Encoding = Encoding.ASCII };
            msg.Type = 0x0200;
            msg.SetValue(48, "DATA", null, IsoType.LLLLVAR, 0);
            var s = msg.DebugString();
            Assert.Contains("0004DATA", s);
        }

        // ── SetField out of range ─────────────────────────────────────────────────

        [Fact]
        public void SetField_BelowRange_Throws()
        {
            var msg = new IsoMessage();
            Assert.Throws<System.IndexOutOfRangeException>(() => msg.SetField(1, null));
        }

        [Fact]
        public void SetField_AboveRange_Throws()
        {
            var msg = new IsoMessage();
            Assert.Throws<System.IndexOutOfRangeException>(() => msg.SetField(129, null));
        }

        // ── SetFields (dictionary) ────────────────────────────────────────────────

        [Fact]
        public void SetFields_SetsAllFromDictionary()
        {
            var msg = new IsoMessage { Encoding = Encoding.ASCII };
            var dict = new Dictionary<int, IsoValue>
            {
                [3] = new IsoValue(IsoType.NUMERIC, "650000", 6),
                [49] = new IsoValue(IsoType.ALPHA, "484", 3)
            };
            msg.SetFields(dict);
            Assert.True(msg.HasField(3));
            Assert.True(msg.HasField(49));
        }

        // ── SetValue with null clears field ───────────────────────────────────────

        [Fact]
        public void SetValue_NullValue_ClearsField()
        {
            var msg = BuildMessage();
            Assert.True(msg.HasField(3));
            msg.SetValue(3, null, null, IsoType.NUMERIC, 6);
            Assert.False(msg.HasField(3));
        }

        // ── UpdateValue on missing field throws ───────────────────────────────────

        [Fact]
        public void UpdateValue_MissingField_Throws()
        {
            var msg = new IsoMessage();
            Assert.Throws<System.ArgumentException>(() => msg.UpdateValue(10, "value"));
        }

        // ── Write with length bytes ───────────────────────────────────────────────

        [Fact]
        public void Write_OneLengthByte_PrependsLength()
        {
            var msg = BuildMessage();
            var list = new System.Collections.Generic.List<sbyte>();
            msg.Write(list, 1);
            var data = msg.WriteData();
            // First byte is length of data
            Assert.Equal((sbyte)(data.Length & 0xff), list[0]);
            Assert.Equal(data.Length + 1, list.Count);
        }

        [Fact]
        public void Write_TwoLengthBytes_PrependsLength()
        {
            var msg = BuildMessage();
            var list = new System.Collections.Generic.List<sbyte>();
            msg.Write(list, 2);
            var data = msg.WriteData();
            Assert.Equal(data.Length + 2, list.Count);
        }

        [Fact]
        public void Write_ThreeLengthBytes_PrependsLength()
        {
            var msg = BuildMessage();
            var list = new System.Collections.Generic.List<sbyte>();
            msg.Write(list, 3);
            var data = msg.WriteData();
            Assert.Equal(data.Length + 3, list.Count);
        }

        [Fact]
        public void Write_FourLengthBytes_PrependsLength()
        {
            var msg = BuildMessage();
            var list = new System.Collections.Generic.List<sbyte>();
            msg.Write(list, 4);
            var data = msg.WriteData();
            Assert.Equal(data.Length + 4, list.Count);
        }

        [Fact]
        public void Write_ZeroLengthBytes_NoLengthPrefix()
        {
            var msg = BuildMessage();
            var list = new System.Collections.Generic.List<sbyte>();
            msg.Write(list, 0);
            var data = msg.WriteData();
            Assert.Equal(data.Length, list.Count);
        }

        [Fact]
        public void Write_WithEtx_AppendEtx()
        {
            var msg = BuildMessage();
            msg.Etx = 0x03;
            var list = new System.Collections.Generic.List<sbyte>();
            msg.Write(list, 0);
            Assert.Equal((sbyte)0x03, list[list.Count - 1]);
        }

        [Fact]
        public void Write_ExceedsMaxLengthBytes_Throws()
        {
            var msg = BuildMessage();
            var list = new System.Collections.Generic.List<sbyte>();
            Assert.Throws<System.ArgumentException>(() => msg.Write(list, 5));
        }

        // ── WriteToBuffer ─────────────────────────────────────────────────────────

        [Fact]
        public void WriteToBuffer_OneLengthByte_HasCorrectSize()
        {
            var msg = BuildMessage();
            var data = msg.WriteData();
            var buf = msg.WriteToBuffer(1);
            Assert.Equal(data.Length + 1, buf.Length);
        }

        [Fact]
        public void WriteToBuffer_FourLengthBytes_HasCorrectSize()
        {
            var msg = BuildMessage();
            var data = msg.WriteData();
            var buf = msg.WriteToBuffer(4);
            Assert.Equal(data.Length + 4, buf.Length);
        }

        [Fact]
        public void WriteToBuffer_WithEtx_AppendsEtx()
        {
            var msg = BuildMessage();
            msg.Etx = 0x0A;
            var buf = msg.WriteToBuffer(0);
            Assert.Equal((sbyte)0x0A, buf[buf.Length - 1]);
        }

        [Fact]
        public void WriteToBuffer_ExceedsMax_Throws()
        {
            var msg = BuildMessage();
            Assert.Throws<System.ArgumentException>(() => msg.WriteToBuffer(5));
        }

        // ── Binary message writing ────────────────────────────────────────────────

        [Fact]
        public void WriteData_BinaryMode_TypeAsBinaryBytes()
        {
            var msg = new IsoMessage { Encoding = Encoding.ASCII };
            msg.Type = 0x0200;
            msg.Binary = true;
            msg.SetValue(3, "650000", null, IsoType.NUMERIC, 6);
            var data = msg.WriteData();
            // First 2 bytes are binary-encoded type 0x0200
            Assert.Equal((sbyte)0x02, data[0]);
            Assert.Equal((sbyte)0x00, data[1]);
        }

        [Fact]
        public void WriteData_BinBitmap_WritesBinaryBitmap()
        {
            var msg = new IsoMessage { Encoding = Encoding.ASCII };
            msg.Type = 0x0200;
            msg.BinBitmap = true;
            msg.SetValue(3, "650000", null, IsoType.NUMERIC, 6);
            var data = msg.WriteData();
            Assert.NotNull(data);
            // Type is ASCII "0200" = 4 bytes, followed by 8 binary bitmap bytes
            Assert.True(data.Length >= 4 + 8);
        }

        // ── EnforceSecondBitmap ───────────────────────────────────────────────────

        [Fact]
        public void WriteData_EnforceSecondBitmap_WritesFullBitmap()
        {
            var msg = new IsoMessage { Encoding = Encoding.ASCII };
            msg.Type = 0x0200;
            msg.EnforceSecondBitmap = true;
            msg.SetValue(3, "650000", null, IsoType.NUMERIC, 6);
            var data = msg.WriteData();
            // ASCII bitmap = 32 hex chars for 128 bits
            Assert.NotNull(data);
            // "0200" (4) + 32-char hex bitmap + fields
            Assert.True(data.Length >= 4 + 32);
        }

        // ── Constructor variants ──────────────────────────────────────────────────

        [Fact]
        public void Constructor_StringHeader_SetsIsoHeader()
        {
            var msg = new IsoMessage("ISO015000050");
            Assert.Equal("ISO015000050", msg.IsoHeader);
        }

        [Fact]
        public void Constructor_BinaryHeader_SetsBinIsoHeader()
        {
            var header = new sbyte[] { 0x01, 0x02 };
            var msg = new IsoMessage(header);
            Assert.Equal(header, msg.BinIsoHeader);
        }

        [Fact]
        public void WriteData_WithBinaryHeader_IncludesHeader()
        {
            var header = new sbyte[] { 0x01, 0x02, 0x03 };
            var msg = new IsoMessage(header) { Encoding = Encoding.ASCII };
            msg.Type = 0x0200;
            var data = msg.WriteData();
            // First 3 bytes are the binary header
            Assert.Equal((sbyte)0x01, data[0]);
            Assert.Equal((sbyte)0x02, data[1]);
            Assert.Equal((sbyte)0x03, data[2]);
        }

        // ── ForceStringEncoding bitmap path ──────────────────────────────────────

        [Fact]
        public void WriteData_ForceStringEncoding_ProducesData()
        {
            var msg = new IsoMessage { Encoding = Encoding.ASCII };
            msg.Type = 0x0200;
            msg.ForceStringEncoding = true;
            msg.SetValue(3, "650000", null, IsoType.NUMERIC, 6);
            var data = msg.WriteData();
            Assert.NotNull(data);
            Assert.True(data.Length > 0);
        }
    }
}
