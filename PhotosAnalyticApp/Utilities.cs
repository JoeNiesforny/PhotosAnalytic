using System;

namespace PhotosAnalytic
{
    public static class CompareArray
    {
        public static bool ByteArray(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;
            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;
            return true;
        }
    }

    public static class Converter
    {
        public static uint ByteToInt(byte[] bytes)
        {
            uint value = 0;
                for (int i = 0; i < bytes.Length; i++)
                    value += bytes[i] * (uint)Math.Pow(2, 8 * (bytes.Length - i - 1));
            return value;
        }
    }

    public interface Format
    {
        string Name { get; }
        void Format(ref byte[] buffer);
    }

    public class Motorola : Format
    {
        public string Name { get { return "Motorola"; } }
        public void Format(ref byte[] buffer)
        {
            return;
        }
    }

    public class Intel : Format
    {
        public string Name { get { return "Intel"; } }
        public void Format(ref byte[] buffer)
        {
            var tmp = new byte[buffer.Length];
            buffer.CopyTo(tmp, 0);
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = tmp[buffer.Length - i - 1];
            return;
        }
    }
}
