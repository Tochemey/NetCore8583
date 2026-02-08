using System;

namespace NetCore8583.Extensions
{
    /// <summary>Hex encoding/decoding for binary ISO 8583 fields (e.g. BINARY, LLBIN).</summary>
    public static class HexCodec
    {
        private static readonly char[] Hex = "0123456789ABCDEF".ToCharArray();

        /// <summary>Encodes a slice of bytes to an uppercase hex string (two chars per byte).</summary>
        /// <param name="buffer">Source buffer.</param>
        /// <param name="start">Start index.</param>
        /// <param name="length">Number of bytes to encode.</param>
        /// <returns>Hex string.</returns>
        public static string HexEncode(sbyte[] buffer,
            int start,
            int length)
        {
            if (buffer.Length == 0) return string.Empty;
            var chars = new char[length * 2];
            var pos = -1;
            for (var i = start; i < start + length; i++)
            {
                var holder = (buffer[i] & 0xf0) >> 4;
                chars[++pos * 2] = Hex[holder];
                holder = buffer[i] & 0x0f;
                chars[pos * 2 + 1] = Hex[holder];
            }

            return new string(chars);
        }

        /// <summary>Decodes a hex string to a signed byte array. Accepts odd-length strings (leading zero implied).</summary>
        /// <param name="hex">Hex string (0-9, A-F, a-f).</param>
        /// <returns>Decoded bytes; empty array if null or empty.</returns>
        public static sbyte[] HexDecode(string hex)
        {
            //A null string returns an empty array
            if (string.IsNullOrEmpty(hex)) return new sbyte[0];
            if (hex.Length < 3)
                return new[]
                {
                    (sbyte) (Convert.ToInt32(hex,
                        16) & 0xff)
                };
            //Adjust accordingly for odd-length strings
            var count = hex.Length;
            var nibble = 0;
            if (count % 2 != 0)
            {
                count++;
                nibble = 1;
            }

            var buf = new sbyte[count / 2];
            var holder = 0;
            var pos = 0;
            for (var i = 0; i < buf.Length; i++)
            for (var z = 0; z < 2 && pos < hex.Length; z++)
            {
                int c = hex[pos++];
                switch (c)
                {
                    case >= 'A' and <= 'F':
                        c -= 55;
                        break;
                    case >= '0' and <= '9':
                        c -= 48;
                        break;
                    case >= 'a' and <= 'f':
                        c -= 87;
                        break;
                }
                if (nibble == 0)
                {
                    holder = c << 4;
                }
                else
                {
                    holder |= c;
                    buf[i] = (sbyte) holder;
                }

                nibble = 1 - nibble;
            }

            return buf;
        }
    }
}