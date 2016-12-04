using System;
using System.Threading.Tasks;
using CorePhoto.IO;
using CorePhoto.Tests.Helpers;
using CorePhoto.Tiff;
using Xunit;

namespace CorePhoto.Tests.Tiff
{
    public class TiffDecompressorTests
    {
        [Fact]
        public async Task DecompressStreamAsync_DecompressData_NoCompression()
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian)
                                .WriteBytes(new byte[] { 10, 20, 30, 40, 50, 40, 30, 20, 10, 20, 40, 60, 80 })
                                .ToStream();

            var data = await TiffDecompressor.DecompressStreamAsync(stream, TiffCompression.None, 11, 11);

            Assert.Equal(new byte[] { 10, 20, 30, 40, 50, 40, 30, 20, 10, 20, 40 }, data);
        }

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
        public async Task DecompressStreamAsync_DecompressData_PackBits(byte[] compressedData, byte[] uncompressedData)
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian)
                                .WriteBytes(compressedData)
                                .ToStream();

            var data = await TiffDecompressor.DecompressStreamAsync(stream, TiffCompression.PackBits, compressedData.Length, uncompressedData.Length);

            Assert.Equal(uncompressedData, data);
        }

        [Theory]
        [InlineData(TiffCompression.Ccitt1D)]
        [InlineData(TiffCompression.CcittGroup3Fax)]
        [InlineData(TiffCompression.CcittGroup4Fax)]
        [InlineData(TiffCompression.Lzw)]
        [InlineData(TiffCompression.OldJpeg)]
        [InlineData(TiffCompression.Jpeg)]
        [InlineData(TiffCompression.Deflate)]
        [InlineData(TiffCompression.OldDeflate)]
        [InlineData(TiffCompression.ItuTRecT43)]
        [InlineData(TiffCompression.ItuTRecT82)]
        [InlineData((TiffCompression)99)]
        public void DecompressStreamAsync_ThrowsException_WithUnsupportedCompression(TiffCompression compression)
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian).ToStream();

            var e = Assert.Throws<NotSupportedException>(() => { TiffDecompressor.DecompressStreamAsync(stream, compression, 100, 100); });

            Assert.Equal($"The compression format '{compression}' is not supported.", e.Message);
        }

        [Theory]
        [InlineData(TiffCompression.None, true)]
        [InlineData(TiffCompression.Ccitt1D, false)]
        [InlineData(TiffCompression.PackBits, true)]
        [InlineData(TiffCompression.CcittGroup3Fax, false)]
        [InlineData(TiffCompression.CcittGroup4Fax, false)]
        [InlineData(TiffCompression.Lzw, false)]
        [InlineData(TiffCompression.OldJpeg, false)]
        [InlineData(TiffCompression.Jpeg, false)]
        [InlineData(TiffCompression.Deflate, false)]
        [InlineData(TiffCompression.OldDeflate, false)]
        [InlineData(TiffCompression.ItuTRecT43, false)]
        [InlineData(TiffCompression.ItuTRecT82, false)]
        [InlineData((TiffCompression)99, false)]
        public void DecompressStreamAsync_ThrowsException_WithUnsupportedCompression(TiffCompression compression, bool expectedResult)
        {
            var supported = TiffDecompressor.SupportsCompression(compression);

            Assert.Equal(expectedResult, supported);
        }
    }
}