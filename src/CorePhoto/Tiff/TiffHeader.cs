using CorePhoto.IO;

namespace CorePhoto.Tiff
{
    public struct TiffHeader
    {
        public ByteOrder ByteOrder;
        public uint FirstIfdOffset;
    }
}