namespace CorePhoto.Tiff
{
    public struct TiffIfdEntry
    {
        public short Tag;
        public TiffType Type;
        public int Count;
        public byte[] Value;
    }
}