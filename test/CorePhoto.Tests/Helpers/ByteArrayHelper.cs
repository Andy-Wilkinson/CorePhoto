using System;
using System.Linq;
using CorePhoto.IO;

namespace CorePhoto.Tests.Helpers
{
    public static class ByteArrayHelper
    {
        public static byte[] ToBytes(uint value, ByteOrder byteOrder)
        {
            return BitConverter.GetBytes(value).WithByteOrder(byteOrder);
        }

        public static byte[] ToBytes(uint[] value, ByteOrder byteOrder)
        {
            byte[] data = value.Select(v => BitConverter.GetBytes(v).WithByteOrder(byteOrder))
                               .SelectMany(bytes => bytes)
                               .ToArray();

            return data;
        }

        public static byte[] WithByteOrder(this byte[] bytes, ByteOrder byteOrder)
        {
            if ((BitConverter.IsLittleEndian && byteOrder == ByteOrder.BigEndian)
                        || (!BitConverter.IsLittleEndian && byteOrder == ByteOrder.LittleEndian))
            {
                byte[] reversedBytes = new byte[bytes.Length];
                Array.Copy(bytes, reversedBytes, bytes.Length);
                Array.Reverse(reversedBytes);
                return reversedBytes;
            }
            else
            {
                return bytes;
            }

        }
    }
}