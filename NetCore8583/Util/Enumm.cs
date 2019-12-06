using System;

namespace NetCore8583.Util
{
    public static class Enumm
    {
        public static T? Parse<T>(string name) where T : struct
        {
            return (T) Enum.Parse(typeof(T),
                name,
                true);
        }
    }
}