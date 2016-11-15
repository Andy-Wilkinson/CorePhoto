using System;
using System.IO;
using System.Linq;
using System.Text;
using CorePhoto.IO;
using CorePhoto.Tiff;

namespace CorePhoto.Tests.Helpers
{
    public static class TiffHelper
    {
        public static TiffIfd GenerateTiffIfd(ushort tag, TiffType type, object value, ByteOrder byteOrder)
        {
            return GenerateTiffIfdWithStream(tag, type, value, byteOrder).Ifd;
        }

        public static TiffIfd_Stream_Tuple GenerateTiffIfdWithStream(ushort tag, TiffType type, object value, ByteOrder byteOrder)
        {
            if (value == null)
            {
                var ifd = TiffHelper.GenerateTiffIfd();
                var stream = new StreamBuilder(byteOrder).ToStream();
                return new TiffIfd_Stream_Tuple(ifd, stream);
            }
            else
            {
                var ifdEntryTuple = TiffHelper.GenerateTiffIfdEntry(tag, type, value, byteOrder);
                var ifd = TiffHelper.GenerateTiffIfd(ifdEntryTuple.Entry);
                var stream = ifdEntryTuple.Stream;
                return new TiffIfd_Stream_Tuple(ifd, stream);
            }
        }

        public static TiffIfd GenerateTiffIfd(params TiffIfdEntry[] requiredEntries)
        {
            var preEntries = new[] { new TiffIfdEntry { Tag = 1 }, new TiffIfdEntry { Tag = 2 }, new TiffIfdEntry { Tag = 3 } };
            var postEntries = new[] { new TiffIfdEntry { Tag = 4 }, new TiffIfdEntry { Tag = 5 }, new TiffIfdEntry { Tag = 6 } };
            var entries = preEntries.Concat(requiredEntries).Concat(postEntries).ToArray();

            return new TiffIfd { Entries = entries };
        }

        public static TiffIfdEntry_Stream_Tuple GenerateTiffIfdEntry(ushort tag, TiffType type, object value, ByteOrder byteOrder)
        {
            byte[] data = new byte[0];
            int count = 1;

            switch (type)
            {
                case TiffType.Byte:
                    if (value is int)
                        data = new byte[] { (byte)(int)value };
                    if (value is uint)
                        data = new byte[] { (byte)(uint)value };
                    break;
                case TiffType.Short:
                    if (value is int)
                        data = BitConverter.GetBytes((ushort)(int)value).WithByteOrder(byteOrder);
                    if (value is uint)
                        data = BitConverter.GetBytes((ushort)(uint)value).WithByteOrder(byteOrder);
                    break;
                case TiffType.Long:
                    if (value is int)
                        data = BitConverter.GetBytes((uint)(int)value).WithByteOrder(byteOrder);
                    if (value is uint)
                        data = BitConverter.GetBytes((uint)value).WithByteOrder(byteOrder);
                    break;
                case TiffType.SByte:
                    if (value is int)
                        data = BitConverter.GetBytes((sbyte)(int)value);
                    if (value is uint)
                        data = BitConverter.GetBytes((sbyte)(uint)value);
                    break;
                case TiffType.SShort:
                    if (value is int)
                        data = BitConverter.GetBytes((short)(int)value).WithByteOrder(byteOrder);
                    if (value is uint)
                        data = BitConverter.GetBytes((short)(uint)value).WithByteOrder(byteOrder);
                    break;
                case TiffType.SLong:
                    if (value is int)
                        data = BitConverter.GetBytes((int)value).WithByteOrder(byteOrder);
                    if (value is uint)
                        data = BitConverter.GetBytes((int)(uint)value).WithByteOrder(byteOrder);
                    break;
                case TiffType.Ascii:
                    data = Encoding.ASCII.GetBytes((string)value);
                    count = data.Length;
                    break;
            }

            return GenerateTiffIfdEntry(tag, type, data, 6, byteOrder, count);
        }

        public static TiffIfdEntry_Stream_Tuple GenerateTiffIfdEntry(TiffType type, byte[] data, byte paddingByteCount, ByteOrder byteOrder)
        {
            var count = data.Length / TiffReader.SizeOfDataType(type);
            return GenerateTiffIfdEntry(type, data, paddingByteCount, byteOrder, count);
        }

        public static TiffIfdEntry_Stream_Tuple GenerateTiffIfdEntry(TiffType type, byte[] data, byte paddingByteCount, ByteOrder byteOrder, int count)
        {
            return GenerateTiffIfdEntry(0, type, data, paddingByteCount, byteOrder, count);
        }

        public static TiffIfdEntry_Stream_Tuple GenerateTiffIfdEntry(ushort tag, TiffType type, byte[] data, byte paddingByteCount, ByteOrder byteOrder, int count)
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

            var entry = new TiffIfdEntry { Tag = tag, Type = type, Count = count, Value = data };

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

        public class TiffIfd_Stream_Tuple
        {
            public TiffIfd_Stream_Tuple(TiffIfd ifd, Stream stream)
            {
                Ifd = ifd;
                Stream = stream;
            }

            public TiffIfd Ifd
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