using System.IO;
using System.Threading.Tasks;
using CorePhoto.IO;

namespace CorePhoto.Tiff
{
    public static class TiffIfdReader
    {
        // Baseline TIFF fields

        public static Task<string> ReadArtist(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadStringAsync(ifd, TiffTags.Artist, stream, byteOrder);

        public static TiffCompression GetCompression(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.Compression);
            return entry == null ? TiffCompression.None : (TiffCompression)entry.Value.GetInteger(byteOrder);
        }

        public static uint? GetImageLength(this TiffIfd ifd, ByteOrder byteOrder) => GetInteger(ifd, TiffTags.ImageLength, byteOrder);

        public static uint? GetImageWidth(this TiffIfd ifd, ByteOrder byteOrder) => GetInteger(ifd, TiffTags.ImageWidth, byteOrder);

        public static TiffNewSubfileType GetNewSubfileType(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.NewSubfileType);
            return entry == null ? TiffNewSubfileType.FullImage : (TiffNewSubfileType)entry.Value.GetInteger(byteOrder);
        }

        public static TiffPhotometricInterpretation? GetPhotometricInterpretation(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.PhotometricInterpretation);
            return entry == null ? null : (TiffPhotometricInterpretation?)entry.Value.GetInteger(byteOrder);
        }

        // Helper functions

        private static uint? GetInteger(TiffIfd ifd, ushort tag, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, tag);
            return entry?.GetInteger(byteOrder);
        }

        private static Task<string> ReadStringAsync(TiffIfd ifd, ushort tag, Stream stream, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, tag);

            if (entry == null)
                return Task.FromResult<string>(null);
            else
                return entry.Value.ReadStringAsync(stream, byteOrder);
        }
    }
}