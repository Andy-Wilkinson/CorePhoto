using System.Threading.Tasks;
using CorePhoto.IO;
using CorePhoto.Tests.Helpers;
using CorePhoto.Tiff;
using ImageSharp;
using Xunit;

namespace CorePhoto.Tests.Tiff
{
    public class TiffImageReaderTests
    {
        [Fact]
        public async Task GetImageDecoderAsync_Rgb_CanDecodeImage()
        {
            var tuple = new TiffIfdBuilder(ByteOrder.LittleEndian)
                            .WithIfdEntry(TiffTags.PhotometricInterpretation, TiffType.Short, (int?)TiffPhotometricInterpretation.Rgb)
                            .WithIfdEntry(TiffTags.SamplesPerPixel, TiffType.Short, 3)
                            .WithIfdEntry(TiffTags.BitsPerSample, TiffType.Short, new uint[] { 8, 8, 8 })
                            .ToIfdStreamTuple();
            var ifd = tuple.Ifd;
            var stream = tuple.Stream;

            var imageData = new byte[] { 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF,
                                         0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF };

            var expectedResult = new[] { new[] {Color.FromHex("FF0000"), Color.FromHex("00FF00"),Color.FromHex("0000FF")},
                                         new[] {Color.FromHex("FFFF00"), Color.FromHex("00FFFF"),Color.FromHex("FF00FF")}};

            int width = expectedResult[0].Length;
            int height = expectedResult.Length;

            var decoder = await TiffImageReader.GetImageDecoderAsync(ifd, stream, ByteOrder.LittleEndian);

            Image image = new Image(width, height);

            using (var pixels = image.Lock())
            {
                decoder(imageData, pixels, new Rectangle(0, 0, width, height));
            }

            using (var pixels = image.Lock())
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Assert.Equal(expectedResult[y][x], pixels[x, y]);
                    }
                }
            }
        }

        [Fact]
        public async Task GetImageDecoderAsync_Rgb_CanDecodeImage_WithExtraSamples()
        {
            var tuple = new TiffIfdBuilder(ByteOrder.LittleEndian)
                            .WithIfdEntry(TiffTags.PhotometricInterpretation, TiffType.Short, (int?)TiffPhotometricInterpretation.Rgb)
                            .WithIfdEntry(TiffTags.SamplesPerPixel, TiffType.Short, 5)
                            .WithIfdEntry(TiffTags.BitsPerSample, TiffType.Short, new uint[] { 8, 8, 8, 8, 8 })
                            .ToIfdStreamTuple();
            var ifd = tuple.Ifd;
            var stream = tuple.Stream;

            var imageData = new byte[] { 0xFF, 0x00, 0x00, 0x55, 0x55, 0x00, 0xFF, 0x00, 0x55, 0x55, 0x00, 0x00, 0xFF, 0x55, 0x55,
                                         0xFF, 0xFF, 0x00, 0x55, 0x55, 0x00, 0xFF, 0xFF, 0x55, 0x55, 0xFF, 0x00, 0xFF, 0x55, 0x55 };

            var expectedResult = new[] { new[] {Color.FromHex("FF0000"), Color.FromHex("00FF00"),Color.FromHex("0000FF")},
                                         new[] {Color.FromHex("FFFF00"), Color.FromHex("00FFFF"),Color.FromHex("FF00FF")}};

            int width = expectedResult[0].Length;
            int height = expectedResult.Length;

            var decoder = await TiffImageReader.GetImageDecoderAsync(ifd, stream, ByteOrder.LittleEndian);

            Image image = new Image(width, height);

            using (var pixels = image.Lock())
            {
                decoder(imageData, pixels, new Rectangle(0, 0, width, height));
            }

            using (var pixels = image.Lock())
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Assert.Equal(expectedResult[y][x], pixels[x, y]);
                    }
                }
            }
        }

        [Theory]
        [InlineData(TiffPhotometricInterpretation.WhiteIsZero, false)]
        [InlineData(TiffPhotometricInterpretation.BlackIsZero, false)]
        [InlineData(TiffPhotometricInterpretation.Rgb, true)]
        [InlineData(TiffPhotometricInterpretation.PaletteColor, false)]
        [InlineData(TiffPhotometricInterpretation.TransparencyMask, false)]
        [InlineData(TiffPhotometricInterpretation.Separated, false)]
        [InlineData(TiffPhotometricInterpretation.YCbCr, false)]
        [InlineData(TiffPhotometricInterpretation.CieLab, false)]
        [InlineData(TiffPhotometricInterpretation.IccLab, false)]
        [InlineData(TiffPhotometricInterpretation.ItuLab, false)]
        [InlineData(TiffPhotometricInterpretation.ColorFilterArray, false)]
        [InlineData(TiffPhotometricInterpretation.LinearRaw, false)]
        [InlineData((TiffCompression)99, false)]
        public void SupportsPhotometricInterpretation_ReturnsCorrectValue(TiffPhotometricInterpretation photometricInterpretation, bool expectedResult)
        {
            var supported = TiffImageReader.SupportsPhotometricInterpretation(photometricInterpretation);

            Assert.Equal(expectedResult, supported);
        }
    }
}