using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CorePhoto.IO;
using CorePhoto.Tiff;
using CorePhoto.Numerics;

namespace CorePhoto.Tests.Helpers
{
    public class TiffIfdBuilder
    {
        private ByteOrder _byteOrder;
        private List<TiffIfdEntryInfo> _entries = new List<TiffIfdEntryInfo>();

        public TiffIfdBuilder(ByteOrder byteOrder)
        {
            _byteOrder = byteOrder;
        }

        public TiffIfdBuilder WithIfdEntry(ushort tag, TiffType type, int? value)
        {
            if (value != null)
            {
                var data = ConvertToBytes(type, value.Value);
                _entries.Add(new TiffIfdEntryInfo(tag, type, 1, data));
            }

            return this;
        }

        public TiffIfdBuilder WithIfdEntry(ushort tag, TiffType type, uint? value)
        {
            if (value != null)
            {
                var data = ConvertToBytes(type, value.Value);
                _entries.Add(new TiffIfdEntryInfo(tag, type, 1, data));
            }

            return this;
        }

        public TiffIfdBuilder WithIfdEntry(ushort tag, TiffType type, int[] value)
        {
            if (value != null)
            {
                var data = value.SelectMany(v => ConvertToBytes(type, v)).ToArray();
                _entries.Add(new TiffIfdEntryInfo(tag, type, value.Length, data));
            }

            return this;
        }

        public TiffIfdBuilder WithIfdEntry(ushort tag, TiffType type, uint[] value)
        {
            if (value != null)
            {
                var data = value.SelectMany(v => ConvertToBytes(type, v)).ToArray();
                _entries.Add(new TiffIfdEntryInfo(tag, type, value.Length, data));
            }

            return this;
        }

        public TiffIfdBuilder WithIfdEntry(ushort tag, TiffType type, Rational? value)
        {
            if (value != null)
            {
                var rational = value.Value;
                var data = new[] {rational.Numerator, rational.Denominator}.SelectMany(v => ConvertToBytes(TiffType.Long, v)).ToArray();
                _entries.Add(new TiffIfdEntryInfo(tag, type, 1, data));
            }

            return this;
        }

        public TiffIfdBuilder WithIfdEntry(ushort tag, TiffType type, string value)
        {
            if (value != null)
            {
                var data = Encoding.ASCII.GetBytes((string)value);
                _entries.Add(new TiffIfdEntryInfo(tag, type, data.Length, data));
            }

            return this;
        }

        public TiffIfd GenerateIfd(StreamBuilder streamBuilder)
        {
            streamBuilder.WritePadding(6);

            var ifdEntries = _entries.Select(entryInfo =>
                            {
                                var entry = new TiffIfdEntry { Tag = entryInfo.Tag, Type = entryInfo.Type, Count = entryInfo.Count };

                                if (entryInfo.Data.Length <= 4)
                                {
                                    entry.Value = entryInfo.Data;
                                }
                                else
                                {
                                    // If the data is longer than four bytes, then write this to the stream and set the data to the offset

                                    var offset = (uint)streamBuilder.Stream.Position;
                                    streamBuilder.WriteBytes(entryInfo.Data);
                                    entry.Value = BitConverter.GetBytes(offset).WithByteOrder(_byteOrder);
                                }

                                return entry;
                            });

            var preEntries = new[] { new TiffIfdEntry { Tag = 1 }, new TiffIfdEntry { Tag = 2 }, new TiffIfdEntry { Tag = 3 } };
            var postEntries = new[] { new TiffIfdEntry { Tag = 4 }, new TiffIfdEntry { Tag = 5 }, new TiffIfdEntry { Tag = 6 } };
            var allIfdEntries = preEntries.Concat(ifdEntries).Concat(postEntries).ToArray();


            return new TiffIfd { Entries = allIfdEntries };
        }

        public TiffHelper.TiffIfd_Stream_Tuple ToIfdStreamTuple()
        {
            var streamBuilder = new StreamBuilder(_byteOrder);

            var ifd = GenerateIfd(streamBuilder);

            return new TiffHelper.TiffIfd_Stream_Tuple(ifd, streamBuilder.ToStream());
        }

        // Static Methods

        public static TiffHelper.TiffIfd_Stream_Tuple GenerateIfd(ushort tag, TiffType type, int? value, ByteOrder byteOrder)
        {
            return new TiffIfdBuilder(byteOrder).WithIfdEntry(tag, type, value).ToIfdStreamTuple();
        }

        public static TiffHelper.TiffIfd_Stream_Tuple GenerateIfd(ushort tag, TiffType type, uint? value, ByteOrder byteOrder)
        {
            return new TiffIfdBuilder(byteOrder).WithIfdEntry(tag, type, value).ToIfdStreamTuple();
        }

        public static TiffHelper.TiffIfd_Stream_Tuple GenerateIfd(ushort tag, TiffType type, int[] value, ByteOrder byteOrder)
        {
            return new TiffIfdBuilder(byteOrder).WithIfdEntry(tag, type, value).ToIfdStreamTuple();
        }

        public static TiffHelper.TiffIfd_Stream_Tuple GenerateIfd(ushort tag, TiffType type, uint[] value, ByteOrder byteOrder)
        {
            return new TiffIfdBuilder(byteOrder).WithIfdEntry(tag, type, value).ToIfdStreamTuple();
        }

        public static TiffHelper.TiffIfd_Stream_Tuple GenerateIfd(ushort tag, TiffType type, Rational? value, ByteOrder byteOrder)
        {
            return new TiffIfdBuilder(byteOrder).WithIfdEntry(tag, type, value).ToIfdStreamTuple();
        }

        public static TiffHelper.TiffIfd_Stream_Tuple GenerateIfd(ushort tag, TiffType type, string value, ByteOrder byteOrder)
        {
            return new TiffIfdBuilder(byteOrder).WithIfdEntry(tag, type, value).ToIfdStreamTuple();
        }

        // Helper Methods

        public byte[] ConvertToBytes(TiffType type, int value)
        {
            switch (type)
            {
                case TiffType.Byte:
                    return new byte[] { (byte)value };
                case TiffType.Short:
                    return BitConverter.GetBytes((ushort)value).WithByteOrder(_byteOrder);
                case TiffType.Long:
                    return BitConverter.GetBytes((uint)value).WithByteOrder(_byteOrder);
                case TiffType.SByte:
                    return BitConverter.GetBytes((sbyte)value);
                case TiffType.SShort:
                    return BitConverter.GetBytes((short)value).WithByteOrder(_byteOrder);
                case TiffType.SLong:
                    return BitConverter.GetBytes(value).WithByteOrder(_byteOrder);
                default:
                    throw new InvalidOperationException("TiffIfdBuilder cannot convert this data type.");
            }
        }

        public byte[] ConvertToBytes(TiffType type, uint value)
        {
            switch (type)
            {
                case TiffType.Byte:
                    return new byte[] { (byte)value };
                case TiffType.Short:
                    return BitConverter.GetBytes((ushort)value).WithByteOrder(_byteOrder);
                case TiffType.Long:
                    return BitConverter.GetBytes((uint)value).WithByteOrder(_byteOrder);
                case TiffType.SByte:
                    return BitConverter.GetBytes((sbyte)value);
                case TiffType.SShort:
                    return BitConverter.GetBytes((short)value).WithByteOrder(_byteOrder);
                case TiffType.SLong:
                    return BitConverter.GetBytes((int)value).WithByteOrder(_byteOrder);
                default:
                    throw new InvalidOperationException("TiffIfdBuilder cannot convert this data type.");
            }
        }

        // Sub-classes

        private class TiffIfdEntryInfo
        {
            public TiffIfdEntryInfo(ushort tag, TiffType type, int count, byte[] data)
            {
                Tag = tag;
                Type = type;
                Count = count;
                Data = data;
            }

            public ushort Tag;
            public TiffType Type;
            public int Count;
            public byte[] Data;
        }
    }
}