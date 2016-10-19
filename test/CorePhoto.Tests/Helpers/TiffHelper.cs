using System;
using System.IO;
using System.Linq;
using CorePhoto.IO;
using CorePhoto.Tiff;

namespace CorePhoto.Tests.Helpers
{
    public static class TiffHelper
    {
        public static TiffIfdEntry_Stream_Tuple GenerateTiffIfdEntry(TiffType type, byte[] data, byte paddingByteCount, ByteOrder byteOrder)
        {
            return GenerateTiffIfdEntry(type, data, paddingByteCount, byteOrder, data.Length);
        }

        public static TiffIfdEntry_Stream_Tuple GenerateTiffIfdEntry(TiffType type, byte[] data, byte paddingByteCount, ByteOrder byteOrder, int count)
        {
            // Create a stream with padding bytes as required

            var streamBuilder = new StreamBuilder(byteOrder);
            byte[] paddingBytes = Enumerable.Repeat((byte)42, paddingByteCount).ToArray();
            streamBuilder.WriteBytes(paddingBytes);

            // If the data is longer than four bytes, then write this to the stream and set the data to the offset

            if (data.Length > 4)
            {
                streamBuilder.WriteBytes(data);
                data = byteOrder == ByteOrder.LittleEndian ? new byte[] { paddingByteCount, 0, 0, 0 } : new byte[] { 0, 0, 0, paddingByteCount };
            }

            var stream = streamBuilder.ToStream();

            // Create the IFD entry and test reading the value

            var entry = new TiffIfdEntry { Type = type, Count = count, Value = data };

            return new TiffIfdEntry_Stream_Tuple(entry, stream);
        }

        // Sub-classes

        public class TiffIfdEntry_Stream_Tuple
        {
            public TiffIfdEntry_Stream_Tuple(TiffIfdEntry entry, Stream stream)
            {
                Entry = entry;
                Stream = stream;
            }

            public TiffIfdEntry Entry
            {
                get;
            }

            public Stream Stream
            {
                get;
            }
        }
    }
}