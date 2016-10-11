using System;
using System.IO;

namespace CorePhoto.IO
{
    public static class StreamEx
    {
        public static byte[] ReadBytes(this Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;

            while (count > 0)
            {
                int bytesRead = stream.Read(buffer, offset, count);

                if (bytesRead == 0)
                    break;

                offset += bytesRead;
                count -= bytesRead;
            }

            return buffer;
        }

        public static Int16 ReadInt16(this Stream stream, ByteOrder byteOrder)
        {
            byte[] bytes = stream.ReadBytes(2);

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

        public static Int32 ReadInt32(this Stream stream, ByteOrder byteOrder)
        {
            byte[] bytes = stream.ReadBytes(4);

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