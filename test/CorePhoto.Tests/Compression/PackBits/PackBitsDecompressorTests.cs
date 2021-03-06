using System.Threading.Tasks;
using CorePhoto.IO;
using CorePhoto.Tests.Helpers;
using CorePhoto.Compression.PackBits;
using Xunit;

namespace CorePhoto.Tests.Compression.PackBits
{
    public class PackBitsDecompressorTests
    {
        [Theory]
        [InlineData(new byte[] { }, new byte[] { })]
        [InlineData(new byte[] { 0x00, 0x2A }, new byte[] { 0x2A })] // Read one byte
        [InlineData(new byte[] { 0x01, 0x15, 0x32 }, new byte[] { 0x15, 0x32 })] // Read two bytes
        [InlineData(new byte[] { 0xFF, 0x2A }, new byte[] { 0x2A, 0x2A })] // Repeat two bytes
        [InlineData(new byte[] { 0xFE, 0x2A }, new byte[] { 0x2A, 0x2A, 0x2A })] // Repeat three bytes
        [InlineData(new byte[] { 0x80 }, new byte[] { })] // Read a 'No operation' byte
        [InlineData(new byte[] { 0x01, 0x15, 0x32, 0x80, 0xFF, 0xA2 }, new byte[] { 0x15, 0x32, 0xA2, 0xA2 })] // Read two bytes, nop, repeat two bytes
        [InlineData(new byte[] { 0xFE, 0xAA, 0x02, 0x80, 0x00, 0x2A, 0xFD, 0xAA, 0x03, 0x80, 0x00, 0x2A, 0x22, 0xF7, 0xAA },
                new byte[] { 0xAA, 0xAA, 0xAA, 0x80, 0x00, 0x2A, 0xAA, 0xAA, 0xAA, 0xAA, 0x80, 0x00, 0x2A, 0x22, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA })] // Apple PackBits sample
        public async Task DecompressStreamAsync_DecompressData(byte[] compressedData, byte[] uncompressedData)
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian)
                                .WriteBytes(compressedData)
                                .ToStream();

            var data = await PackBitsDecompressor.DecompressStreamAsync(stream, compressedData.Length, uncompressedData.Length);

            Assert.Equal(uncompressedData, data);
        }
    }
}