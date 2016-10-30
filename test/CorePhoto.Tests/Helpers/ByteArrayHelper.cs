using System;
using CorePhoto.IO;

namespace CorePhoto.Tests.Helpers
{
    public static class ByteArrayHelper
    {
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