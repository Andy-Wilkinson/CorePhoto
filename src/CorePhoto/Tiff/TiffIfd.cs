namespace CorePhoto.Tiff
{
    public struct TiffIfd
    {
        public TiffIfdEntry[] Entries;
        public TiffIfdReference? NextIfdReference;
    }
}