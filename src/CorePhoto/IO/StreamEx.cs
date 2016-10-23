using System;
using System.IO;
using System.Threading.Tasks;

namespace CorePhoto.IO
{
    public static class StreamEx
    {
        public static async Task<byte[]> ReadBytesAsync(this Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;

            while (count > 0)
            {
                int bytesRead = await stream.ReadAsync(buffer, offset, count);

                if (bytesRead == 0)
                    break;

                offset += bytesRead;
                count -= bytesRead;
            }

            return buffer;
        }

        public static async Task<Int16> ReadInt16Async(this Stream stream, ByteOrder byteOrder)
        {
            byte[] bytes = await stream.ReadBytesAsync(2);
            return DataConverter.ToInt16(bytes, 0, byteOrder);
        }

        public static async Task<UInt16> ReadUInt16Async(this Stream stream, ByteOrder byteOrder)
        {
            byte[] bytes = await stream.ReadBytesAsync(2);
            return DataConverter.ToUInt16(bytes, 0, byteOrder);
        }

        public static async Task<Int32> ReadInt32Async(this Stream stream, ByteOrder byteOrder)
        {
            byte[] bytes = await stream.ReadBytesAsync(4);
            return DataConverter.ToInt32(bytes, 0, byteOrder);
        }

        public static async Task<UInt32> ReadUInt32Async(this Stream stream, ByteOrder byteOrder)
        {
            byte[] bytes = await stream.ReadBytesAsync(4);
            return DataConverter.ToUInt32(bytes, 0, byteOrder);
        }
    }
}