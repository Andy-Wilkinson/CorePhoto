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

            return new TiffHeader() { ByteOrder = byteOrder, FirstIfdOffset = firstIfdOffset };
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
    }
}