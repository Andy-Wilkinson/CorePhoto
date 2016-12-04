using System;
using System.IO;
using System.Threading.Tasks;
using CorePhoto.IO;

namespace CorePhoto.Compression.PackBits
{
    public static class PackBitsDecompressor
    {
        public async static Task<byte[]> DecompressStreamAsync(Stream stream, int compressedLength, int uncompressedLength)
        {
            byte[] compressedData = await stream.ReadBytesAsync(compressedLength);
            byte[] decompressedData = new byte[uncompressedLength];
            int compressedOffset = 0;
            int decompressedOffset = 0;

            while (compressedOffset < compressedLength)
            {
                byte headerByte = compressedData[compressedOffset];

                if (IsHeaderLiteralData(headerByte))
                {
                    int literalOffset = compressedOffset + 1;
                    int literalLength = compressedData[compressedOffset] + 1;

                    Array.Copy(compressedData, literalOffset, decompressedData, decompressedOffset, literalLength);

                    compressedOffset += literalLength + 1;
                    decompressedOffset += literalLength;
                }
                else if (IsHeaderNoOperation(headerByte))
                {
                    compressedOffset += 1;
                }
                else if (IsHeaderRepeatData(headerByte))
                {
                    byte repeatData = compressedData[compressedOffset + 1];
                    int repeatLength = 257 - headerByte;

                    ArrayCopyRepeat(repeatData, decompressedData, decompressedOffset, repeatLength);

                    compressedOffset += 2;
                    decompressedOffset += repeatLength;
                }
            }

            return decompressedData;
        }

        private static bool IsHeaderLiteralData(byte headerByte) => headerByte <= (byte)127;
        private static bool IsHeaderRepeatData(byte headerByte) => headerByte > (byte)0x80;
        private static bool IsHeaderNoOperation(byte headerByte) => headerByte == (byte)0x80;

        private static void ArrayCopyRepeat(byte value, byte[] destinationArray, int destinationIndex, int length)
        {
            for (int i = 0; i < length; i++)
            {
                destinationArray[i + destinationIndex] = value;
            }
        }
    }
}