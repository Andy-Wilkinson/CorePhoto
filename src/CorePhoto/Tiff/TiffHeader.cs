using CorePhoto.IO;

namespace CorePhoto.Tiff
{
    public struct TiffHeader
    {
        public ByteOrder byteOrder;
        public int magicNumber;
        public int firstIfdOffset;
    }
}