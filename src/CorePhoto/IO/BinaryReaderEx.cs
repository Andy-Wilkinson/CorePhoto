using System;
using System.IO;
using CorePhoto.IO;

namespace CorePhoto.IO
{
    public static class BinaryReaderEx
    {
        public static Int16 ReadInt16(this BinaryReader reader, ByteOrder byteOrder)
        {
            byte[] bytes = reader.ReadBytes(2);
            
            switch (byteOrder)
            {
                case ByteOrder.LittleEndian:
                    return (short)(bytes[0] | (bytes[1] << 8));
                case ByteOrder.BigEndian:
                    return (short)((bytes[0] << 8) | bytes[1]);
                default:
                    throw new NotImplementedException();
            }
        }

        public static Int32 ReadInt32(this BinaryReader reader, ByteOrder byteOrder)
        {
            byte[] bytes = reader.ReadBytes(4);
            
            switch (byteOrder)
            {
                case ByteOrder.LittleEndian:
                    return (short)(bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24));
                case ByteOrder.BigEndian:
                    return (short)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}