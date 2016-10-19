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
        DataTable dtbase;

        public AnalyzePhotos(string[] files)
        {
            dtbase = new DataTable();
            foreach (var file in files)
                Analyze(file);
        }

        void Analyze(string file)
        {
            var header = new Header();
            var entries = new List<Entry>();
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
                    header.Size = Converter.ByteToInt(buffer, false);
                    buffer = new byte[6];
                    offset += stream.Read(buffer, 0, Marker.ExifHeader.Length);
                    if (!CompareArray.ByteArray(buffer, Marker.ExifHeader))
                        throw new FileFormatException("No ExifHeader!");
                    // Looking for IFD0
                    header.BeginningOffset = offset;
                    buffer = new byte[2];
                    offset += stream.Read(buffer, 0, Marker.IntelFormat.Length);
                    if (CompareArray.ByteArray(buffer, Marker.IntelFormat))
                        header.Format = true;
                    else
                        header.Format = false;
                    offset += stream.Read(buffer, 0, 2); // offset is 4 bytes
                    if (!CompareArray.ByteArray(buffer, Marker.MarkTag, header.Format))
                        throw new FileFormatException("Problem with format. Missing 0x002a!");
                    // Get IFD0 Offset
                    buffer = new byte[4];
                    offset += stream.Read(buffer, 0, 4); // offset is 4 bytes
                    header.IFD0Offset = Converter.ByteToInt(buffer, header.Format);
                    // Go to IFD0
                    stream.Seek(header.IFD0Offset + header.BeginningOffset, SeekOrigin.Begin);
                    // Get number of IFD0 entries
                    buffer = new byte[2];
                    offset += stream.Read(buffer, 0, 2);
                    header.IFD0EntriesNumber = Converter.ByteToInt(buffer, header.Format);
                    // Get Entries of IFD0
                    for (int i = 0; i < header.IFD0EntriesNumber; i++)
                    {
                        var newEntry = new Entry();
                        buffer = new byte[2];
                        offset += stream.Read(buffer, 0, 2);
                        buffer.CopyTo(newEntry.Tag, 0);
                        offset += stream.Read(buffer, 0, 2);
                        newEntry.Format = Converter.ByteToInt(buffer, header.Format);
                        buffer = new byte[4];
                        offset += stream.Read(buffer, 0, 4);
                        newEntry.NumberOfComponents = Converter.ByteToInt(buffer, header.Format);
                        offset += stream.Read(buffer, 0, 4);
                        buffer.CopyTo(newEntry.Value, 0);
                        entries.Add(newEntry);
                    }
                    buffer = new byte[4];
                    offset += stream.Read(buffer, 0, 4);
                    // Get offset of IFD1
                    header.IFD1Offset = Converter.ByteToInt(buffer, header.Format);
                    // (optional IFD1)
                    //// Go to IFD1
                    //stream.Seek(header.IFD1Offset + header.BeginningOffset, SeekOrigin.Begin);
                    //// IFD1 Entries
                    //buffer = new byte[2];
                    //offset += stream.Read(buffer, 0, 2);
                    //header.IFD1EntriesNumber = Converter.ByteToInt(buffer, header.Format);
                    //// Get Entries of IFD1
                    //for (int i = 0; i < header.IFD1EntriesNumber; i++)
                    //{
                    //    var newEntry = new Entry();
                    //    buffer = new byte[2];
                    //    offset += stream.Read(buffer, 0, 2);
                    //    buffer.CopyTo(newEntry.Tag, 0);
                    //    offset += stream.Read(buffer, 0, 2);
                    //    newEntry.Format = Converter.ByteToInt(buffer, header.Format);
                    //    buffer = new byte[4];
                    //    offset += stream.Read(buffer, 0, 4);
                    //    newEntry.NumberOfComponents = Converter.ByteToInt(buffer, header.Format);
                    //    offset += stream.Read(buffer, 0, 4);
                    //    buffer.CopyTo(newEntry.Value, 0);
                    //    entries.Add(newEntry);
                    //}
                    header.ExifSubIFDOffset = FindExifOffset(entries, header.Format);
                    if (header.ExifSubIFDOffset < 0)
                        throw new FileFormatException("Photo doesn't contain exif header.");
                    // Go to Exif SubIFD
                    stream.Seek(header.ExifSubIFDOffset + header.BeginningOffset, SeekOrigin.Begin);
                    // Exif SubIFD Entries
                    buffer = new byte[2];
                    offset += stream.Read(buffer, 0, 2);
                    header.ExifSubIFDEntriesNumber = Converter.ByteToInt(buffer, header.Format);
                    // Get Entries of Exif SubIFD
                    for (int i = 0; i < header.ExifSubIFDEntriesNumber; i++)
                    {
                        var newEntry = new Entry();
                        buffer = new byte[2];
                        offset += stream.Read(buffer, 0, 2);
                        buffer.CopyTo(newEntry.Tag, 0);
                        offset += stream.Read(buffer, 0, 2);
                        newEntry.Format = Converter.ByteToInt(buffer, header.Format);
                        buffer = new byte[4];
                        offset += stream.Read(buffer, 0, 4);
                        newEntry.NumberOfComponents = Converter.ByteToInt(buffer, header.Format);
                        offset += stream.Read(buffer, 0, 4);
                        buffer.CopyTo(newEntry.Value, 0);
                        entries.Add(newEntry);
                    }
                }
            }
            dtbase.Columns.Add("tag");
            dtbase.Columns.Add("value");
            foreach (var entry in entries)
            {
                var newRow = dtbase.NewRow();
                newRow["tag"] = entry.Tag[0].ToString("x") + entry.Tag[1].ToString("x");
                newRow["value"] = entry.Value[0].ToString("x") + entry.Value[1].ToString("x") + entry.Value[2].ToString("x") + entry.Value[3].ToString("x");
                dtbase.Rows.Add(newRow);
            }
        }

        int FindExifOffset(IEnumerable<Entry> entries, bool format = true)
        {
            foreach (var entry in entries)
                if (CompareArray.ByteArray(Component.Tag.ExifOffset, entry.Tag, format))
                    return Converter.ByteToInt(entry.Value, format);
            return -1;
        }

        public DataView Result()
        {
            return dtbase.DefaultView;
        }
    }
}
