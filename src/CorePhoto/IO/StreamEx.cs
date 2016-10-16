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
            return DataConverter.ToInt16(bytes, 0, byteOrder);
        }

        public static Int32 ReadInt32(this Stream stream, ByteOrder byteOrder)
        {
            byte[] bytes = stream.ReadBytes(4);
            return DataConverter.ToInt32(bytes, 0, byteOrder);
        }
    }
}