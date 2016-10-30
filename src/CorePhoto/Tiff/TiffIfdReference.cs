namespace CorePhoto.Tiff
{
    public struct TiffIfdReference
    {
        public TiffIfdReference(uint offset)
        {
            Offset = offset;
        }

        public uint Offset { get; }
    }
}