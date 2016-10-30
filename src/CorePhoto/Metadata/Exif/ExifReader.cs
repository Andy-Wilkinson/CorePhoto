using CorePhoto.IO;
using CorePhoto.Tiff;

namespace CorePhoto.Metadata.Exif
{
    public static class ExifReader
    {
        public static TiffIfdReference? GetExifIfdReference(TiffIfd ifd, ByteOrder byteOrder)
        {
            var exifEntry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.ExifIFD);

            if (exifEntry == null)
                return null;
            else
                return TiffReader.GetIfdReference(exifEntry.Value, byteOrder);
        }
    }
}