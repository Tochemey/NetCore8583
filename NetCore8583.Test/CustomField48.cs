using System;
using System.Text;

namespace NetCore8583.Test
{
    public class CustomField48 : ICustomField
    {
        public string V1 { get; set; }
        public int V2 { get; set; }

        public object DecodeField(string value)
        {
            CustomField48 cf = null;
            if (value != null)
                if (value.Length == 1 && value[0] == '|')
                {
                    cf = new CustomField48();
                }
                else
                {
                    var idx = value.LastIndexOf('|');
                    if (idx < 0 || idx == value.Length - 1)
                        throw new ArgumentException($"Invalid data '{value}' for field 48");
                    cf = new CustomField48
                    {
                        V1 = value.Substring(0,
                            idx),
                        V2 = int.Parse(value.Substring(idx + 1))
                    };
                }
            return cf;
        }

        public string EncodeField(object value)
        {
            var val = (CustomField48) value;
            var sb = new StringBuilder();
            if (val.V1 != null) sb.Append(val.V1);
            sb.Append('|');
            sb.Append(val.V2);
            return sb.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var other = obj as CustomField48;
            if (other?.V2 != V2) return false;
            if (other.V1 == null) return V1 == null;
            return other.V1.Equals(V1);
        }

        public override int GetHashCode()
        {
            return (V1 == null ? 0 : V1.GetHashCode()) | V2;
        }
    }
}