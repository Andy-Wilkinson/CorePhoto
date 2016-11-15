using System;
using System.IO;
using System.Threading.Tasks;
using CorePhoto.IO;

namespace CorePhoto.Tiff
{
    public static class TiffDecompressor
    {
        public static Task<byte[]> DecompressStreamAsync(Stream stream, int length, TiffCompression compression)
        {
            switch (compression)
            {
                case TiffCompression.None:
                    return DecompressStreamAsync_None(stream, length);
                default:
                    throw new NotSupportedException($"The compression format '{compression}' is not supported.");
            }
        }

        public static bool SupportsCompression(TiffCompression compression)
        {
            switch (compression)
            {
                case TiffCompression.None:
                    return true;
                default:
                    return false;
            }
        }

        private static Task<byte[]> DecompressStreamAsync_None(Stream stream, int length)
        {
            return stream.ReadBytesAsync(length);
        }
    }
}