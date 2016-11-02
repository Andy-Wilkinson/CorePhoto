using System;

namespace CorePhoto.Tiff
{
    [Flags]
    public enum TiffNewSubfileType
    {
        // TIFF baseline subfile types

        FullImage = 0x0000,
        Preview = 0x0001,
        SinglePage = 0x0002,
        TransparencyMask = 0x0004,

        // DNG Specification subfile types

        AlternativePreview = 0x10000,

        // TIFF-F/FX Specification subfile types
        
        MixedRasterContent = 0x0008

    }
}