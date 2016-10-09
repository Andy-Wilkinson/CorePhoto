using System;
using System.IO;
using CorePhoto.IO;

namespace CorePhoto.Tiff
{
    public static class TiffReader
    {
        public static TiffHeader ReadHeader(BinaryReader reader)
        {
            ByteOrder byteOrder = ReadHeader_ByteOrder(reader);
            int magicNumber = reader.ReadInt16(byteOrder);
            int firstIfdOffset = reader.ReadInt32(byteOrder);

            return new TiffHeader() { byteOrder = byteOrder, magicNumber = magicNumber, firstIfdOffset = firstIfdOffset };
        }

        private static ByteOrder ReadHeader_ByteOrder(BinaryReader reader)
        {
            short byteOrderMarker = reader.ReadInt16();

            switch (byteOrderMarker)
            {
                case 0x4949:
                    return ByteOrder.LittleEndian;
                case 0x4D4D:
                    return ByteOrder.BigEndian;
                default:
                    // TODO : Throw a more relevant exception
                    throw new NotImplementedException();
            }
        }
    }
}