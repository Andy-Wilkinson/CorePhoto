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
            var count = data.Length / TiffReader.SizeOfDataType(type);
            return GenerateTiffIfdEntry(type, data, paddingByteCount, byteOrder, count);
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

        public static TiffIfdEntry GenerateTiffIfdEntry(ushort tag, uint value, ByteOrder byteOrder)
        {
            return GenerateTiffIfdEntry(tag, TiffType.Long, value, byteOrder);
        }

        public static TiffIfdEntry GenerateTiffIfdEntry(ushort tag, TiffType type, uint value, ByteOrder byteOrder)
        {
            byte[] data = ByteArrayHelper.ToBytes(value, byteOrder);
            return new TiffIfdEntry { Tag = tag, Type = type, Count = 1, Value = data };
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