using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

//
// Source: https://www.media.mit.edu/pia/Research/deepview/exif.html
//

namespace PhotosAnalytic
{
    public class AnalyzePhotos
    {
        DataTable _dtbase;

        public AnalyzePhotos(string[] files)
        {
            foreach (var file in files)
                Analyze(file);
        }

        void Analyze(string file)
        {
            List<Entry> entries = new List<Entry>();
            var header = new Header();
            using(var stream = new FileStream(file, FileMode.Open))
            {
                var offset = 0;
                byte[] buffer = new byte[2];
                offset += stream.Read(buffer, 0, Marker.Start.Length);
                if (!CompareArray.ByteArray(buffer, Marker.Start))
                    throw new FileFormatException("Not .jpg");
                offset += stream.Read(buffer, 0, Marker.APP1Marker.Length);
                if (CompareArray.ByteArray(buffer, Marker.APP1Marker))
                {
                    offset += stream.Read(buffer, 0, 2);
                    header.Size = Converter.ByteToInt(buffer);
                    buffer = new byte[6];
                    offset += stream.Read(buffer, 0, Marker.ExifHeader.Length);
                    if (!CompareArray.ByteArray(buffer, Marker.ExifHeader))
                        throw new FileFormatException("No ExifHeader!");
                    // Looking for IFD0
                    header.BeginningOffset = (uint)offset;
                    buffer = new byte[2];
                    stream.Read(buffer, 0, Marker.IntelFormat.Length);
                    if (CompareArray.ByteArray(buffer, Marker.IntelFormat))
                        header.Type = new Intel();
                    else
                        header.Type = new Motorola();

                    stream.Read(buffer, 0, 2); // offset is 4 bytes
                    header.Type.Format(ref buffer);
                    if (!CompareArray.ByteArray(buffer, Marker.MarkTag))
                        throw new FileFormatException("Problem with format. Missing 0x002a!");

                    // Get IFD0 Offset
                    buffer = new byte[4];
                    stream.Read(buffer, 0, 4); // offset is 4 bytes
                    header.Type.Format(ref buffer);
                    header.IFD0Offset = Converter.ByteToInt(buffer);
                    entries.AddRange(GetEntries(stream, header, header.IFD0Offset));

                    //// Get offset of IFD1
                    //buffer = new byte[4];
                    //stream.Read(buffer, 0, 4);
                    //header.Type.Format(ref buffer);
                    //// Get offset of IFD1
                    //header.IFD1Offset = Converter.ByteToInt(buffer);
                    //// (optional IFD1)

                    // Exif Header
                    header.ExifSubIFDOffset = FindExifOffset(entries);
                    if (header.ExifSubIFDOffset == uint.MaxValue)
                        throw new FileFormatException("Photo doesn't contain exif header.");
                    entries.AddRange(GetEntries(stream, header, header.ExifSubIFDOffset));

                    CheckValues(ref entries, stream, header);
                }
            }
            ComputeToHumanReadableFormat(entries);
        }

        // Parameter offset is a place where function start to read header.
        List<Entry> GetEntries(FileStream stream, Header header, uint offset)
        {
            List<Entry> entries = new List<Entry>();
            // Go to specific header (IFD0, IFD1 or Exif)
            stream.Seek(offset + header.BeginningOffset, SeekOrigin.Begin);
            // Get number of entries
            var buffer = new byte[2];
            stream.Read(buffer, 0, 2);
            header.Type.Format(ref buffer);
            var entriesNumber = Converter.ByteToInt(buffer);
            // Get Entries
            for (int i = 0; i < entriesNumber; i++)
            {
                var newEntry = new Entry();
                buffer = new byte[2];
                stream.Read(buffer, 0, 2);
                header.Type.Format(ref buffer);
                newEntry.Tag = (ushort)Converter.ByteToInt(buffer);
                stream.Read(buffer, 0, 2);
                header.Type.Format(ref buffer);
                newEntry.Format = Converter.ByteToInt(buffer);
                buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                header.Type.Format(ref buffer);
                newEntry.NumberOfComponents = Converter.ByteToInt(buffer);
                stream.Read(buffer, 0, 4);
                header.Type.Format(ref buffer);
                newEntry.Value = Converter.ByteToInt(buffer);
                entries.Add(newEntry);
            }
            return entries;
        }

        uint FindExifOffset(IEnumerable<Entry> entries)
        {
            foreach (var entry in entries)
                if (Components.Tag.ExifOffset == entry.Tag)
                    return (uint)entry.Value;
            return uint.MaxValue;
        }

        public DataView Result()
        {
            return _dtbase.DefaultView;
        }

        // Some value of entries contains offset to real value of entry.
        void CheckValues(ref List<Entry> entries, FileStream stream, Header header)
        {
            foreach (var entry in entries)
            {
                switch ((Components.Format)entry.Format)
                {
                    case Components.Format.doubleFloat:
                    case Components.Format.signedRational:
                    case Components.Format.unsignedRational:
                        {
                            stream.Seek((uint)entry.Value + header.BeginningOffset, SeekOrigin.Begin);
                            var bytes = new byte[entry.NumberOfComponents * 8];
                            stream.Read(bytes, 0, (int)entry.NumberOfComponents * 8);
                            header.Type.Format(ref bytes);
                            entry.dValue = BitConverter.ToDouble(bytes, 0);
                        }
                        break;

                    case Components.Format.asciiString:
                        if (entry.NumberOfComponents > 4)
                        {
                            stream.Seek((uint)entry.Value + header.BeginningOffset, SeekOrigin.Begin);
                            var bytes = new byte[entry.NumberOfComponents];
                            stream.Read(bytes, 0, (int)entry.NumberOfComponents);
                            header.Type.Format(ref bytes);
                            entry.sValue = BitConverter.ToString(bytes);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        void ComputeToHumanReadableFormat(List<Entry> entries)
        {
            _dtbase = new DataTable();
            _dtbase.Columns.Add("tag");
            _dtbase.Columns.Add("value");
            foreach (var entry in entries)
            {
                try
                {
                    var newRow = _dtbase.NewRow();
                    newRow["tag"] = Components.UserTag[entry.Tag];
                    switch ((Components.Format)entry.Format)
                    {
                        case Components.Format.unsignedByte:
                        case Components.Format.unsignedShort:
                        case Components.Format.unsignedLong:
                            newRow["value"] = ((uint)(entry.Value)).ToString();
                            break;

                        case Components.Format.signedByte:
                        case Components.Format.signedShort:
                        case Components.Format.signedLong:
                            newRow["value"] = ((int)(entry.Value)).ToString();
                            break;

                        case Components.Format.singleFloat:
                            newRow["value"] = BitConverter.ToSingle(BitConverter.GetBytes((uint)entry.Value), 0).ToString();
                            break;

                        case Components.Format.doubleFloat:
                        case Components.Format.signedRational:
                        case Components.Format.unsignedRational:
                            newRow["value"] = entry.dValue.ToString();
                            break;

                        case Components.Format.asciiString:
                            newRow["value"] = entry.sValue;
                            break;

                        default:
                            throw new Exception("Format is not supported!");
                    }
                    _dtbase.Rows.Add(newRow);
                }
                catch (Exception)
                {
                    ;// Avoid tags that are unknown.
                }
            }
        }
    }
}
