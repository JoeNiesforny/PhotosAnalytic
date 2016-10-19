using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotosAnalytic
{
    public static class CompareArray
    {
        public static bool ByteArray(byte[] a1, byte[] a2, bool format = false)
        {
            if (a1.Length != a2.Length)
                return false;
            if (format)
            {
                for (int i = 0; i < a1.Length; i++)
                    if (a1[i] != a2[a1.Length - i - 1])
                        return false;
            }
            else
            {
                for (int i = 0; i < a1.Length; i++)
                    if (a1[i] != a2[i])
                        return false;
            }
            return true;
        }
    }

    public static class Converter
    {
        public static int ByteToInt(byte[] bytes, bool intelFormat = true)
        {
            var value = 0;
            if (intelFormat)
                for (int i = 0; i < bytes.Length; i++)
                    value += bytes[i] * (int)Math.Pow(2, 8 * i);
            else
                for (int i = 0; i < bytes.Length; i++)
                    value += bytes[i] * (int)Math.Pow(2, 8 * (bytes.Length - i - 1));
            return value;
        }
    }
}
