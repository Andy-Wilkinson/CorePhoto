using System.IO;
using System.Threading.Tasks;
using CorePhoto.IO;

namespace CorePhoto.Tiff
{
    public static class TiffIfdReader
    {
        // Baseline TIFF fields

        public static Task<string> ReadArtist(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadString(ifd, TiffTags.Artist, stream, byteOrder);

        public static TiffCompression GetCompression(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.Compression);
            return entry == null ? TiffCompression.None : (TiffCompression)entry.Value.GetInteger(byteOrder);
        }

        // Helper functions

        private static Task<string> ReadString(TiffIfd ifd, ushort tag, Stream stream, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, tag);

            if (entry == null)
                return Task.FromResult<string>(null);
            else
                return entry.Value.ReadStringAsync(stream, byteOrder);
        }
    }
}