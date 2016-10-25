using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotosAnalytic
{
    public class Entry
    {
        public ushort Tag;
        public uint Format;
        public uint NumberOfComponents; // if number of components is bigger than 4 bytes 
                                        // than value is an offset to real value.
        public uint Offset;
        public uint Value;
        public double dValue;
        public string sValue;
    }

    public static class Components
    {
        public static class Tag
        {
            public static ushort ExifOffset = 0x8769;
        }

        public static Dictionary<ushort, string> UserTag = new Dictionary<ushort, string>()
        {
            { 0x829a, "Exposure Time" },
            { 0x829d, "FNumber" },
            { 0x8822, "Exposure Program"},
            { 0x8827, "ISOSpeedRatings" },
            { 0x9000, "ExifVersion" },
            { 0x9207, "MeteringMode"},
            { 0x9208, "Light Source"},
            { 0x9209, "Flash" },
            { 0x920a, "Focal Length" },
        };

        public enum Format : uint
        {
            unsignedByte = 1,
            asciiString,
            unsignedShort,
            unsignedLong,
            unsignedRational,
            signedByte,
            undefined,
            signedShort,
            signedLong,
            signedRational,
            singleFloat,
            doubleFloat
        }
    }

    public struct Header
    {
        public uint Size;
        public uint BeginningOffset;
        public uint IFD0Offset;
        public uint IFD0EntriesNumber;
        public uint IFD1Offset;
        public uint IFD1EntriesNumber;
        public uint ExifSubIFDOffset;
        public uint ExifSubIFDEntriesNumber;
        public Format Type; // Format type => intel or motorola.
    }

    public static class Marker
    {
        public static byte[] Start = { 0xFF, 0xD8 };
        public static byte[] APP1Marker = { 0xFF, 0xE1 };
        public static byte[] ExifHeader = { 0x45, 0x78, 0x69, 0x66, 0x00, 0x00 };
        public static byte[] IntelFormat = { 0x49, 0x49 };
        public static byte[] MotorolaFormat = { 0x4d, 0x4d };
        public static byte[] TiffHeader = { 0x20, 0x0f, 0x20, 0x0e, 0x01 };
        public static byte[] MarkTag = { 0x00, 0x2a };
        public static byte[] StartOfStream = { 0xFF, 0xDA };
        public static byte[] End = { 0xFF, 0xD9 };
    }
}
