namespace CorePhoto.Tiff
{
    public struct TiffIfdEntry
    {
        public ushort Tag;
        public TiffType Type;
        public int Count;
        public byte[] Value;
    }
}