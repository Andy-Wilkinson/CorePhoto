using CorePhoto.IO;

namespace CorePhoto.Tiff
{
    public static class TiffIfdReader
    {
        public static TiffCompression GetCompression(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.Compression);
            return entry == null ? TiffCompression.None : (TiffCompression)entry.Value.GetInteger(byteOrder);
        }
    }
}