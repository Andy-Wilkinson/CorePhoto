namespace CorePhoto.Tiff
{
    public enum TiffTag
    {
        // Section 3: Bilevel Images

        Color = 262,
        Compression = 259,
        ImageLength = 257,
        ImageWidth = 256,
        ResolutionUnit = 296,
        XResolution = 282,
        YResolution = 283,
        RowsPerStrip = 278,
        StripOffsets = 273,
        StripByteCounts = 279,

        // Section 4: Grayscale Images

        BitsPerSample = 258,

        // Section 5: Palette-color Images

        ColorMap = 320,

        // Section 6: RGB Full Color Images

        SamplesPerPixel = 277
    }
}