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
            int value = stream.ReadInt32(byteOrder);

            return new TiffIfdEntry { Tag = tag, Type = type, Count = count, Value = value };
        }
    }
}