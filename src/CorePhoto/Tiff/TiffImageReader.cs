using System;
using System.IO;
using System.Threading.Tasks;
using CorePhoto.IO;
using ImageSharp;

namespace CorePhoto.Tiff
{
    public static class TiffImageReader
    {
        public async static Task<Action<byte[], PixelAccessor<Color, uint>, Rectangle>> GetImageDecoderAsync(TiffIfd ifd, Stream stream, ByteOrder byteOrder)
        {
            var photometricInterpretation = ifd.GetPhotometricInterpretation(byteOrder);

            switch (photometricInterpretation)
            {
                case TiffPhotometricInterpretation.Rgb:
                    {
                        var samplesPerPixel = (int)ifd.GetSamplesPerPixel(byteOrder);
                        var bitsPerSample = await ifd.ReadBitsPerSampleAsync(stream, byteOrder);

                        if (bitsPerSample.Length >= 3 && bitsPerSample[0] == 8 && bitsPerSample[1] == 8 && bitsPerSample[2] == 8)
                            return (imageData, pixels, destination) => DecodeImageData_Rgb_888(imageData, pixels, destination, samplesPerPixel);
                    }
                    break;
                default:
                    throw new System.NotImplementedException();
            }

            throw new System.NotImplementedException();
        }

        public static bool SupportsPhotometricInterpretation(TiffPhotometricInterpretation photometricInterpretation)
        {
            switch (photometricInterpretation)
            {
                case TiffPhotometricInterpretation.Rgb:
                    return true;
                default:
                    return false;
            }
        }

        private static void DecodeImageData_Rgb_888(byte[] imageData, PixelAccessor<Color, uint> pixels, Rectangle destination, int bytesPerPixel)
        {
            var offset = 0;

            for (var y = 0; y < destination.Height; y++)
            {
                for (var x = 0; x < destination.Width; x++)
                {
                    var color = default(Color);

                    var r = imageData[offset];
                    var g = imageData[offset + 1];
                    var b = imageData[offset + 2];
                    color.PackFromBytes(r, g, b, 255);

                    pixels[x + destination.Left, y + destination.Top] = color;
                    offset += bytesPerPixel;
                }
            }
        }
    }
}