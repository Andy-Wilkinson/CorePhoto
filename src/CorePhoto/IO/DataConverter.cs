using System;

namespace CorePhoto.IO
{
    public static class DataConverter
    {
        public static Byte ToByte(byte[] bytes, int offset)
        {
            return bytes[offset];
        }

        public static SByte ToSByte(byte[] bytes, int offset)
        {
            return (sbyte)bytes[offset];
        }

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

        public static UInt16 ToUInt16(byte[] bytes, int offset, ByteOrder byteOrder)
        {
            return (ushort)ToInt16(bytes, offset, byteOrder);
        }

        public static UInt32 ToUInt32(byte[] bytes, int offset, ByteOrder byteOrder)
        {
            return (uint)ToInt32(bytes, offset, byteOrder);
        }

        public static Single ToSingle(byte[] bytes, int offset, ByteOrder byteOrder)
        {
            byte[] buffer = new byte[4];
            Array.Copy(bytes, offset, buffer, 0, 4);

            if ((!BitConverter.IsLittleEndian && byteOrder == ByteOrder.LittleEndian) || (BitConverter.IsLittleEndian && byteOrder == ByteOrder.BigEndian))
                Array.Reverse(buffer);

            return BitConverter.ToSingle(buffer, 0);
        }

        public static Double ToDouble(byte[] bytes, int offset, ByteOrder byteOrder)
        {
            byte[] buffer = new byte[8];
            Array.Copy(bytes, offset, buffer, 0, 8);

            if ((!BitConverter.IsLittleEndian && byteOrder == ByteOrder.LittleEndian) || (BitConverter.IsLittleEndian && byteOrder == ByteOrder.BigEndian))
                Array.Reverse(buffer);

            return BitConverter.ToDouble(buffer, 0);
        }

        public static byte[] ToBytes(byte[] bytes, int offset, int count)
        {
            byte[] buffer = new byte[count];
            Array.Copy(bytes, offset, buffer, 0, count);
            return buffer;
        }
    }
}