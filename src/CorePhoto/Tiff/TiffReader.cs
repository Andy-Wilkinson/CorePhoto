using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CorePhoto.IO;

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
            TiffIfdReference firstIfdReference = new TiffIfdReference(DataConverter.ToUInt32(bytes, 4, byteOrder));

            if (magicNumber != 42)
                throw new ImageFormatException("The TIFF header does not contain the expected magic number.");

            return new TiffHeader { ByteOrder = byteOrder, FirstIfdReference = firstIfdReference };
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

            var nextIfdOffset = await stream.ReadUInt32Async(byteOrder);
            var nextIfdReference = nextIfdOffset != 0 ? new TiffIfdReference(nextIfdOffset) : (TiffIfdReference?)null;

            return new TiffIfd { Entries = entries, NextIfdReference = nextIfdReference };
        }

        public static Task<TiffIfd> ReadIfdAsync(TiffIfdReference ifdReference, Stream stream, ByteOrder byteOrder)
        {
            stream.Seek(ifdReference.Offset, SeekOrigin.Begin);
            return ReadIfdAsync(stream, byteOrder);
        }

        public static Task<TiffIfd> ReadFirstIfdAsync(TiffHeader header, Stream stream, ByteOrder byteOrder)
        {
            return ReadIfdAsync(header.FirstIfdReference, stream, byteOrder);
        }

        public static async Task<TiffIfd?> ReadNextIfdAsync(TiffIfd ifd, Stream stream, ByteOrder byteOrder)
        {
            if (ifd.NextIfdReference == null)
                return null;

            var nextIfd = await ReadIfdAsync(ifd.NextIfdReference.Value, stream, byteOrder);
            return nextIfd;
        }

        public static async Task<TiffIfdReference[]> ReadSubIfdReferencesAsync(TiffIfd ifd, Stream stream, ByteOrder byteOrder)
        {
            var subIfdsEntry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.SubIFDs);

            if (subIfdsEntry != null)
                return await subIfdsEntry.Value.ReadIfdReferenceArrayAsync(stream, byteOrder);
            else
                return Array.Empty<TiffIfdReference>();
        }

        public static TiffIfdEntry ParseIfdEntry(byte[] bytes, int offset, ByteOrder byteOrder)
        {
            ushort tag = DataConverter.ToUInt16(bytes, offset + 0, byteOrder);
            TiffType type = (TiffType)DataConverter.ToUInt16(bytes, offset + 2, byteOrder);
            int count = DataConverter.ToInt32(bytes, offset + 4, byteOrder);
            byte[] value = DataConverter.ToBytes(bytes, offset + 8, 4);

            return new TiffIfdEntry { Tag = tag, Type = type, Count = count, Value = value };
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
                case TiffType.Ifd:
                    return 4;
                case TiffType.Rational:
                case TiffType.SRational:
                case TiffType.Double:
                    return 8;
                default:
                    return 0;
            }
        }

        public static TiffIfdEntry? GetTiffIfdEntry(TiffIfd ifd, ushort tag)
        {
            var entry = ifd.Entries.FirstOrDefault<TiffIfdEntry>(e => e.Tag == tag);
            return entry.Tag == 0 ? (TiffIfdEntry?)null : entry;
        }
    }
}