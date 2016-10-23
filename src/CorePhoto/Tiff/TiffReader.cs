using System.IO;
using System.Linq;
using System.Text;
using CorePhoto.IO;
using CorePhoto.Numerics;

namespace CorePhoto.Tiff
{
    public static class TiffReader
    {
        public static TiffHeader ReadHeader(Stream stream)
        {
            ByteOrder byteOrder = ReadHeader_ByteOrder(stream);
            int magicNumber = stream.ReadInt16(byteOrder);
            uint firstIfdOffset = stream.ReadUInt32(byteOrder);

            if (magicNumber != 42)
                throw new ImageFormatException("The TIFF header does not contain the expected magic number.");

            return new TiffHeader { ByteOrder = byteOrder, FirstIfdOffset = firstIfdOffset };
        }

        private static ByteOrder ReadHeader_ByteOrder(Stream stream)
        {
            short byteOrderMarker = stream.ReadInt16(ByteOrder.LittleEndian);

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

        public static TiffIfd ReadIfd(Stream stream, ByteOrder byteOrder)
        {
            int entryCount = stream.ReadInt16(byteOrder);
            TiffIfdEntry[] entries = new TiffIfdEntry[entryCount];

            for (int i = 0; i < entryCount; i++)
            {
                entries[i] = ReadIfdEntry(stream, byteOrder);
            }

            uint nextIfdOffset = stream.ReadUInt32(byteOrder);

            return new TiffIfd { Entries = entries, NextIfdOffset = nextIfdOffset };
        }

        public static TiffIfd ReadIfd(Stream stream, ByteOrder byteOrder, uint offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            return ReadIfd(stream, byteOrder);
        }

        public static TiffIfd ReadFirstIfd(TiffHeader header, Stream stream, ByteOrder byteOrder)
        {
            var offset = header.FirstIfdOffset;
            return ReadIfd(stream, byteOrder, offset);
        }

        public static TiffIfd? ReadNextIfd(TiffIfd ifd, Stream stream, ByteOrder byteOrder)
        {
            if (ifd.NextIfdOffset == 0)
                return null;

            var offset = ifd.NextIfdOffset;
            return ReadIfd(stream, byteOrder, offset);
        }

        public static TiffIfdEntry ReadIfdEntry(Stream stream, ByteOrder byteOrder)
        {
            TiffTag tag = (TiffTag)stream.ReadUInt16(byteOrder);
            TiffType type = (TiffType)stream.ReadUInt16(byteOrder);
            int count = stream.ReadInt32(byteOrder);
            byte[] value = stream.ReadBytes(4);

            return new TiffIfdEntry { Tag = tag, Type = type, Count = count, Value = value };
        }

        public static byte[] ReadData(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            var sizeOfData = SizeOfData(entry);

            if (sizeOfData <= 4)
            {
                return entry.Value;
            }
            else
            {
                var dataOffset = DataConverter.ToUInt32(entry.Value, 0, byteOrder);
                stream.Seek(dataOffset, SeekOrigin.Begin);
                return stream.ReadBytes(sizeOfData);
            }
        }

        public static int SizeOfHeader(TiffHeader header) => 8;

        public static int SizeOfIfdEntry(TiffIfdEntry entry) => 12;

        public static int SizeOfIfd(TiffIfd ifd) => ifd.Entries.Length * 12 + 6;

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

        public static uint[] ReadIntegerArray(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            switch (entry.Type)
            {
                case TiffType.Byte:
                    {
                        byte[] data = ReadData(entry, stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => (uint)DataConverter.ToByte(data, index)).ToArray();
                    }
                case TiffType.Short:
                    {
                        byte[] data = ReadData(entry, stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => (uint)DataConverter.ToUInt16(data, index * 2, byteOrder)).ToArray();
                    }
                case TiffType.Long:
                    {
                        byte[] data = ReadData(entry, stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => DataConverter.ToUInt32(data, index * 4, byteOrder)).ToArray();
                    }
                default:
                    throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to an unsigned integer.");
            }
        }

        public static int[] ReadSignedIntegerArray(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            switch (entry.Type)
            {
                case TiffType.SByte:
                    {
                        byte[] data = ReadData(entry, stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => (int)DataConverter.ToSByte(data, index)).ToArray();
                    }
                case TiffType.SShort:
                    {
                        byte[] data = ReadData(entry, stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => (int)DataConverter.ToInt16(data, index * 2, byteOrder)).ToArray();
                    }
                case TiffType.SLong:
                    {
                        byte[] data = ReadData(entry, stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => DataConverter.ToInt32(data, index * 4, byteOrder)).ToArray();
                    }
                default:
                    throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a signed integer.");
            }
        }

        public static string ReadString(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.Ascii)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a string.");

            byte[] data = ReadData(entry, stream, byteOrder);

            if (data[data.Length - 1] != 0)
                throw new ImageFormatException("The retrieved string is not null terminated.");

            return Encoding.ASCII.GetString(data, 0, data.Length - 1);
        }

        public static Rational ReadRational(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Count != 1)
                throw new ImageFormatException("Cannot read a single value from an array of multiple items.");

            var array = ReadRationalArray(entry, stream, byteOrder);
            return array[0];
        }

        public static SignedRational ReadSignedRational(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Count != 1)
                throw new ImageFormatException("Cannot read a single value from an array of multiple items.");

            var array = ReadSignedRationalArray(entry, stream, byteOrder);
            return array[0];
        }

        public static Rational[] ReadRationalArray(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.Rational)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a Rational.");

            byte[] data = ReadData(entry, stream, byteOrder);

            return Enumerable.Range(0, entry.Count).Select(index =>
                       {
                           var numerator = DataConverter.ToUInt32(data, index * 8, byteOrder);
                           var denominator = DataConverter.ToUInt32(data, index * 8 + 4, byteOrder);
                           return new Rational(numerator, denominator);
                       }).ToArray();
        }

        public static SignedRational[] ReadSignedRationalArray(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.SRational)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a SignedRational.");

            byte[] data = ReadData(entry, stream, byteOrder);

            return Enumerable.Range(0, entry.Count).Select(index =>
                       {
                           var numerator = DataConverter.ToInt32(data, index * 8, byteOrder);
                           var denominator = DataConverter.ToInt32(data, index * 8 + 4, byteOrder);
                           return new SignedRational(numerator, denominator);
                       }).ToArray();
        }
    }
}