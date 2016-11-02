namespace CorePhoto.Tiff
{
    public enum TiffPhotometricInterpretation
    {
        // TIFF baseline color spaces

        WhiteIsZero = 0,
        BlackIsZero = 1,
        Rgb = 2,
        PaletteColor = 3,
        TransparencyMask = 4,

        // TIFF Extension color spaces

        Separated = 5,
        YCbCr = 6,
        CieLab = 8,

        // TIFF TechNote 1

        IccLab = 9,

        // TIFF-F/FX Specification

        ItuLab = 10,

        // DNG Specification

        ColorFilterArray = 32803,
        LinearRaw = 34892
    }
}