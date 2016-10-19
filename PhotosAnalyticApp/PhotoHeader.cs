using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotosAnalytic
{
    public class Entry
    {
        public byte[] Tag = new byte[2];
        public int Format;
        public int NumberOfComponents;
        public byte[] Value = new byte[4];
    }

    public static class Component
    {
        public static class Tag
        {
            public static byte[] FocalLength = { 0x92, 0x0a };
            public static byte[] ExifOffset = { 0x87, 0x69 };
        }

        enum Format : int
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
        public int Size;
        public int BeginningOffset;
        public int IFD0Offset;
        public int IFD0EntriesNumber;
        public int IFD1Offset;
        public int IFD1EntriesNumber;
        public int ExifSubIFDOffset;
        public int ExifSubIFDEntriesNumber;
        public bool Format; // if false then motorola format. If true than intel.
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
