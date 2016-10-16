using System;

namespace CorePhoto.IO
{
    public static class DataConverter
    {
        public static Int16 ToInt16(byte[] bytes, int offset, ByteOrder byteOrder)
        {
            switch (byteOrder)
            {
                case ByteOrder.LittleEndian:
                    return (short)(bytes[offset + 0] | (bytes[offset + 1] << 8));
                case ByteOrder.BigEndian:
                    return (short)((bytes[offset + 0] << 8) | bytes[offset + 1]);
                default:
                    throw new NotImplementedException();
            }
        }

        public static Int32 ToInt32(byte[] bytes, int offset, ByteOrder byteOrder)
        {
            switch (byteOrder)
            {
                case ByteOrder.LittleEndian:
                    return bytes[offset + 0] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24);
                case ByteOrder.BigEndian:
                    return (bytes[offset + 0] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
                default:
                    throw new NotImplementedException();
            }
        }
    }
}