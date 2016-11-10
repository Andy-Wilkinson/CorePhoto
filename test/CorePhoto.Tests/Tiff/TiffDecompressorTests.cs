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

            var data = await TiffDecompressor.DecompressStreamAsync(stream, 11, TiffCompression.None);

            Assert.Equal(new byte[] { 10, 20, 30, 40, 50, 40, 30, 20, 10, 20, 40 }, data);
        }

        [Theory]
        [InlineData(TiffCompression.ItuTRecT43)]
        [InlineData((TiffCompression)99)]
        public void DecompressStreamAsync_ThrowsException_WithUnsupportedCompression(TiffCompression compression)
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian).ToStream();

            var e = Assert.Throws<NotSupportedException>(() => { TiffDecompressor.DecompressStreamAsync(stream, 100, compression); });

            Assert.Equal("The compression format '{compression}' is not supported.", e.Message);
        }
    }
}