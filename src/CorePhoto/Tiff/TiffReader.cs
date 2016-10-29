using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorePhoto.IO;
using CorePhoto.Numerics;

namespace CorePhoto.Tiff
{
    public static class TiffReader
    {
        private const int SIZEOF_HEADER = 8;
        private const int SIZEOF_IFDENTRY = 12;

        public static async Task<TiffHeader> ReadHeaderAsync(Stream stream)
        {
            var bytes = await stream.ReadBytesAsync(SIZEOF_HEADER);

            ByteOrder byteOrder = ReadHeader_ByteOrder(bytes);
            int magicNumber = DataConverter.ToInt16(bytes, 2, byteOrder);
            uint firstIfdOffset = DataConverter.ToUInt32(bytes, 4, byteOrder);

            if (magicNumber != 42)
                throw new ImageFormatException("The TIFF header does not contain the expected magic number.");

            return new TiffHeader { ByteOrder = byteOrder, FirstIfdOffset = firstIfdOffset };
        }

        private static ByteOrder ReadHeader_ByteOrder(byte[] bytes)
        {
            var byteOrderMarker = DataConverter.ToUInt16(bytes, 0, ByteOrder.LittleEndian);

            switch (byteOrderMarker)
            {
                case 0x4949:
                    return ByteOrder.LittleEndian;
                case 0x4D4D:
                    return ByteOrder.BigEndian;
                default:
                    throw new ImageFormatException("The TIFF byte order markers are invalid.");
            }
        }

        public static async Task<TiffIfd> ReadIfdAsync(Stream stream, ByteOrder byteOrder)
        {
            int entryCount = await stream.ReadInt16Async(byteOrder);

            var entries = new TiffIfdEntry[entryCount];
            var bytes = await stream.ReadBytesAsync(entryCount * SIZEOF_IFDENTRY);

            for (int i = 0; i < entryCount; i++)
            {
                entries[i] = ParseIfdEntry(bytes, i * SIZEOF_IFDENTRY, byteOrder);
            }

            uint nextIfdOffset = await stream.ReadUInt32Async(byteOrder);

            return new TiffIfd { Entries = entries, NextIfdOffset = nextIfdOffset };
        }

        public static Task<TiffIfd> ReadIfdAsync(Stream stream, ByteOrder byteOrder, uint offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            return ReadIfdAsync(stream, byteOrder);
        }

        public static Task<TiffIfd> ReadFirstIfdAsync(TiffHeader header, Stream stream, ByteOrder byteOrder)
        {
            var offset = header.FirstIfdOffset;
            return ReadIfdAsync(stream, byteOrder, offset);
        }

        public static async Task<TiffIfd?> ReadNextIfdAsync(TiffIfd ifd, Stream stream, ByteOrder byteOrder)
        {
            if (ifd.NextIfdOffset == 0)
                return null;

            var offset = ifd.NextIfdOffset;
            var nextIfd = await ReadIfdAsync(stream, byteOrder, offset);
            return nextIfd;
        }

        public static TiffIfdEntry ParseIfdEntry(byte[] bytes, int offset, ByteOrder byteOrder)
        {
            ushort tag = DataConverter.ToUInt16(bytes, offset + 0, byteOrder);
            TiffType type = (TiffType)DataConverter.ToUInt16(bytes, offset + 2, byteOrder);
            int count = DataConverter.ToInt32(bytes, offset + 4, byteOrder);
            byte[] value = DataConverter.ToBytes(bytes, offset + 8, 4);

            return new TiffIfdEntry { Tag = tag, Type = type, Count = count, Value = value };
        }

        public static Task<byte[]> ReadDataAsync(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            var sizeOfData = SizeOfData(entry);

            if (sizeOfData <= 4)
            {
                return Task.FromResult(entry.Value);
            }
            else
            {
                var dataOffset = DataConverter.ToUInt32(entry.Value, 0, byteOrder);
                stream.Seek(dataOffset, SeekOrigin.Begin);
                return stream.ReadBytesAsync(sizeOfData);
            }
        }

        public static int SizeOfHeader(TiffHeader header) => SIZEOF_HEADER;

        public static int SizeOfIfdEntry(TiffIfdEntry entry) => SIZEOF_IFDENTRY;

        public static int SizeOfIfd(TiffIfd ifd) => ifd.Entries.Length * SIZEOF_IFDENTRY + 6;

        public static int SizeOfDataType(TiffType type)
        {
            switch (type)
            {
                case TiffType.Byte:
                case TiffType.Ascii:
                case TiffType.SByte:
                case TiffType.Undefined:
                    return 1;
                case TiffType.Short:
                case TiffType.SShort:
                    return 2;
                case TiffType.Long:
                case TiffType.SLong:
                case TiffType.Float:
                    return 4;
                case TiffType.Rational:
                case TiffType.SRational:
                case TiffType.Double:
                    return 8;
                default:
                    return 0;
            }
        }

        public static int SizeOfData(TiffIfdEntry entry) => SizeOfDataType(entry.Type) * entry.Count;

        public static uint GetInteger(TiffIfdEntry entry, ByteOrder byteOrder)
        {
            if (entry.Count != 1)
                throw new ImageFormatException("Cannot read a single value from an array of multiple items.");

            switch (entry.Type)
            {
                case TiffType.Byte:
                    return DataConverter.ToByte(entry.Value, 0);
                case TiffType.Short:
                    return DataConverter.ToUInt16(entry.Value, 0, byteOrder);
                case TiffType.Long:
                    return DataConverter.ToUInt32(entry.Value, 0, byteOrder);
                default:
                    throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to an unsigned integer.");
            }
        }

        public static int GetSignedInteger(TiffIfdEntry entry, ByteOrder byteOrder)
        {
            if (entry.Count != 1)
                throw new ImageFormatException("Cannot read a single value from an array of multiple items.");

            switch (entry.Type)
            {
                case TiffType.SByte:
                    return DataConverter.ToSByte(entry.Value, 0);
                case TiffType.SShort:
                    return DataConverter.ToInt16(entry.Value, 0, byteOrder);
                case TiffType.SLong:
                    return DataConverter.ToInt32(entry.Value, 0, byteOrder);
                default:
                    throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a signed integer.");
            }
        }

        public static Task<uint[]> ReadIntegerArrayAsync(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            var type = entry.Type;

            if (type != TiffType.Byte && type != TiffType.Short && type != TiffType.Long)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to an unsigned integer.");

            return ReadIntegerArrayAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<uint[]> ReadIntegerArrayAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            switch (entry.Type)
            {
                case TiffType.Byte:
                    {
                        byte[] data = await ReadDataAsync(entry, stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => (uint)DataConverter.ToByte(data, index)).ToArray();
                    }
                case TiffType.Short:
                    {
                        byte[] data = await ReadDataAsync(entry, stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => (uint)DataConverter.ToUInt16(data, index * 2, byteOrder)).ToArray();
                    }
                case TiffType.Long:
                    {
                        byte[] data = await ReadDataAsync(entry, stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => DataConverter.ToUInt32(data, index * 4, byteOrder)).ToArray();
                    }
                default:
                    throw new InvalidOperationException();
            }
        }

        public static Task<int[]> ReadSignedIntegerArrayAsync(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            var type = entry.Type;

            if (type != TiffType.SByte && type != TiffType.SShort && type != TiffType.SLong)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a signed integer.");

            return ReadSignedIntegerArrayAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<int[]> ReadSignedIntegerArrayAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            switch (entry.Type)
            {
                case TiffType.SByte:
                    {
                        byte[] data = await ReadDataAsync(entry, stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => (int)DataConverter.ToSByte(data, index)).ToArray();
                    }
                case TiffType.SShort:
                    {
                        byte[] data = await ReadDataAsync(entry, stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => (int)DataConverter.ToInt16(data, index * 2, byteOrder)).ToArray();
                    }
                case TiffType.SLong:
                    {
                        byte[] data = await ReadDataAsync(entry, stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => DataConverter.ToInt32(data, index * 4, byteOrder)).ToArray();
                    }
                default:
                    throw new InvalidOperationException();
            }
        }

        public static Task<string> ReadStringAsync(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.Ascii)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a string.");

            return ReadStringAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<string> ReadStringAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            byte[] data = await ReadDataAsync(entry, stream, byteOrder);

            if (data[data.Length - 1] != 0)
                throw new ImageFormatException("The retrieved string is not null terminated.");

            return Encoding.ASCII.GetString(data, 0, data.Length - 1);
        }

        public static Task<Rational> ReadRationalAsync(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.Rational)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a Rational.");

            if (entry.Count != 1)
                throw new ImageFormatException("Cannot read a single value from an array of multiple items.");

            return ReadRationalAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<Rational> ReadRationalAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            var array = await ReadRationalArrayAsync_Internal(entry, stream, byteOrder);
            return array[0];
        }

        public static Task<SignedRational> ReadSignedRationalAsync(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.SRational)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a SignedRational.");

            if (entry.Count != 1)
                throw new ImageFormatException("Cannot read a single value from an array of multiple items.");

            return ReadSignedRationalAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<SignedRational> ReadSignedRationalAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            var array = await ReadSignedRationalArrayAsync_Internal(entry, stream, byteOrder);
            return array[0];
        }

        public static Task<Rational[]> ReadRationalArrayAsync(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.Rational)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a Rational.");

            return ReadRationalArrayAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<Rational[]> ReadRationalArrayAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {


            byte[] data = await ReadDataAsync(entry, stream, byteOrder);

            return Enumerable.Range(0, entry.Count).Select(index =>
                       {
                           var numerator = DataConverter.ToUInt32(data, index * 8, byteOrder);
                           var denominator = DataConverter.ToUInt32(data, index * 8 + 4, byteOrder);
                           return new Rational(numerator, denominator);
                       }).ToArray();
        }

        public static Task<SignedRational[]> ReadSignedRationalArrayAsync(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.SRational)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a SignedRational.");

            return ReadSignedRationalArrayAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<SignedRational[]> ReadSignedRationalArrayAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {


            byte[] data = await ReadDataAsync(entry, stream, byteOrder);

            return Enumerable.Range(0, entry.Count).Select(index =>
                       {
                           var numerator = DataConverter.ToInt32(data, index * 8, byteOrder);
                           var denominator = DataConverter.ToInt32(data, index * 8 + 4, byteOrder);
                           return new SignedRational(numerator, denominator);
                       }).ToArray();
        }
    }
}