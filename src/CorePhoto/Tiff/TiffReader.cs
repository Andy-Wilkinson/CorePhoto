using System.IO;
using CorePhoto.IO;

namespace CorePhoto.Tiff
{
    public static class TiffReader
    {
        public static TiffHeader ReadHeader(Stream stream)
        {
            ByteOrder byteOrder = ReadHeader_ByteOrder(stream);
            int magicNumber = stream.ReadInt16(byteOrder);
            int firstIfdOffset = stream.ReadInt32(byteOrder);

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

            int nextIfdOffset = stream.ReadInt32(byteOrder);

            return new TiffIfd { Entries = entries, NextIfdOffset = nextIfdOffset };
        }

        public static TiffIfd ReadIfd(Stream stream, ByteOrder byteOrder, int offset)
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
            short tag = stream.ReadInt16(byteOrder);
            TiffType type = (TiffType)stream.ReadInt16(byteOrder);
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
                var dataOffset = DataConverter.ToInt32(entry.Value, 0, byteOrder);
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
    }
}