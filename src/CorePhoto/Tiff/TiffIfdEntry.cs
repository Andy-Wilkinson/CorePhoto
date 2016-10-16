namespace CorePhoto.Tiff
{
    public struct TiffIfdEntry
    {
        public TiffTag Tag;
        public TiffType Type;
        public int Count;
        public byte[] Value;
    }
}