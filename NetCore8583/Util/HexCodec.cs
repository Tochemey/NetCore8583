using System;

namespace NetCore8583.Util
{
    public static class HexCodec
    {
        private static readonly char[] Hex = "0123456789ABCDEF".ToCharArray();

        public static string HexEncode(sbyte[] buffer,
            int start,
            int length)
        {
            if (buffer.Length == 0) return string.Empty;
            int holder;
            var chars = new char[length * 2];
            var pos = -1;
            for (var i = start; i < start + length; i++)
            {
                holder = (buffer[i] & 0xf0) >> 4;
                chars[++pos * 2] = Hex[holder];
                holder = buffer[i] & 0x0f;
                chars[pos * 2 + 1] = Hex[holder];
            }

            return new string(chars);
        }

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
            int c;
            var holder = 0;
            var pos = 0;
            for (var i = 0; i < buf.Length; i++)
            for (var z = 0; z < 2 && pos < hex.Length; z++)
            {
                c = hex[pos++];
                if (c >= 'A' && c <= 'F') c -= 55;
                else if (c >= '0' && c <= '9') c -= 48;
                else if (c >= 'a' && c <= 'f') c -= 87;
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