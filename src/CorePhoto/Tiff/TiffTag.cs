namespace CorePhoto.Tiff
{
    // TODO : Private IFD Tags (http://www.awaresystems.be/imaging/tiff/tifftags/privateifd.html)

    public enum TiffTag
    {
        // Section 8: Baseline Fields

        Artist = 315,
        BitsPerSample = 258,
        CellLength = 265,
        CellWidth = 264,
        ColorMap = 320,
        Compression = 259,
        Copyright = 33432,
        DateTime = 306,
        ExtraSamples = 338,
        FillOrder = 266,
        FreeByteCounts = 289,
        FreeOffsets = 288,
        GrayResponseCurve = 291,
        GrayResponseUnit = 290,
        HostComputer = 316,
        ImageDescription = 270,
        ImageLength = 257,
        ImageWidth = 256,
        Make = 271,
        MaxSampleValue = 281,
        MinSampleValue = 280,
        Model = 272,
        NewSubfileType = 254,
        Orientation = 274,
        PhotometricInterpretation = 262,
        PlanarConfiguraion = 284,
        ResolutionUnit = 296,
        RowsPerStrip = 278,
        SamplesPerPixel = 277,
        Software = 305,
        StripByteCounts = 279,
        StripOffsets = 273,
        SubfileType = 255,
        Threshholding = 263,
        XResolution = 282,
        YResolution = 283,

        // Section 11: CCITT Bilevel Encodings

        T4Options = 292,
        T6Options = 293,

        // Section 12: Document Storage and Retrieval

        DocumentName = 269,
        PageName = 285,
        PageNumber = 297,
        XPosition = 286,
        YPosition = 287,

        // Section 14: Differencing Predictor

        Predictor = 317,

        // Section 15: Tiled Images

        TileWidth = 322,
        TileLength = 323,
        TileOffsets = 324,
        TileByteCounts = 325,

        // Section 16: CMYK Images

        InkSet = 332,
        NumberOfInks = 334,
        InkNames = 333,
        DotRange = 336,
        TargetPrinter = 337,

        // Section 17: Halftone Hints

        HalftoneHints = 321,

        // Section 19: Data Sample Format

        SampleFormat = 339,
        SMinSampleValue = 340,
        SMaxSampleValue = 341,

        // Section 20: RGB Image Colorimetry

        WhitePoint = 318,
        PrimaryChromaticities = 319,
        TransferFunction = 301,
        TransferRange = 342,
        ReferenceBlackWhite = 532,

        // Section 21: YCbCr Images

        YCbCrCoefficients = 529,
        YCbCrSubSampling = 530,
        YCbCrPositioning = 531,

        // Section 22: JPEG Compression

        JpegProc = 512,
        JpegInterchangeFormat = 513,
        JpegInterchangeFormatLength = 514,
        JpegRestartInterval = 515,
        JpegLosslessPredictors = 517,
        JpegPointTransforms = 518,
        JpegQTables = 519,
        JpegDCTables = 520,
        JpegACTables = 521,

        // TIFF Supplement 1: Adobe Pagemaker 6.0

        SubIFDs = 330,
        ClipPath = 343,
        XClipPathUnits = 344,
        YClipPathUnits = 345,
        Indexed = 346,
        ImageID = 32781,
        OpiProxy = 351,

        // TIFF Supplement 2: Adobe Photoshop

        ImageSourceData = 37724,

        // TIFF/EP Specification: Additional Tags

        JPEGTables = 0x015B,
        CFARepeatPatternDim = 0x828D,
        BatteryLevel = 0x828F,
        InterColorProfile = 0x8773,
        Interlace = 0x8829,
        TimeZoneOffset = 0x882A,
        SelfTimerMode = 0x882B,
        Noise = 0x920D,
        ImageNumber = 0x9211,
        SecurityClassification = 0x9212,
        ImageHistory = 0x9213,
        TiffEPStandardID = 0x9216,

        // TIFF/EP Specification: Redefined EXIF tags

        FlashEnergy = 0x920B,
        SpatialFrequencyResponse = 0x920C,
        FocalPlaneXResolution = 0x920E,
        FocalPlaneYResolution = 0x920F,
        FocalPlaneResolutionUnit = 0x9210,
        SubjectLocation = 0x9214,
        ExposureIndex = 0x9215,
        SensingMethod = 0x9216,
        CFAPattern = 0x828E,

        // TIFF-F/FX Specification (http://www.ietf.org/rfc/rfc2301.txt)

        BadFaxLines = 326,
        CleanFaxData = 327,
        ConsecutiveBadFaxLines = 328,
        GlobalParametersIFD = 400,
        ProfileType = 401,
        FaxProfile = 402,
        CodingMethod = 403,
        VersionYear = 404,
        ModeNumber = 405,
        Decode = 433,
        DefaultImageColor = 434,
        StripRowCounts = 559,
        ImageLayer = 34732,

        // DNG Specification: DNG Specific Tags

        DNGVersion = 50706,
        DNGBackwardVersion = 50707,
        UniqueCameraModel = 50708,
        LocalizedCameraModel = 50709,
        CFAPlaneColor = 50710,
        CFALayout = 50711,
        LinearizationTable = 50712,
        BlackLevelRepeatDim = 50713,
        BlackLevel = 50714,
        BlackLevelDeltaH = 50715,
        BlackLevelDeltaV = 50716,
        WhiteLevel = 50717,
        DefaultScale = 50718,
        BestQualityScale = 50780,
        DefaultCropOrigin = 50719,
        DefaultCropSize = 50720,
        CalibrationIlluminant1 = 50778,
        CalibrationIlluminant2 = 50779,
        ColorMatrix1 = 50721,
        ColorMatrix2 = 50722,
        CameraCalibration1 = 50723,
        CameraCalibration2 = 50724,
        ReductionMatrix1 = 50725,
        ReductionMatrix2 = 50726,
        AnalogBalance = 50727,
        AsShotNeutral = 50728,
        AsShotWhiteXY = 50729,
        BaselineExposure = 50730,
        BaselineNoise = 50731,
        BaselineSharpness = 50732,
        BayerGreenSplit = 50733,
        LinearResponseLimit = 50734,
        CameraSerialNumber = 50735,
        LensInfo = 50736,
        ChromaBlurRadius = 50737,
        AntiAliasStrength = 50738,
        ShadowScale = 50739,
        DNGPrivateData = 50740,
        MakerNoteSafety = 50741,
        RawDataUniqueID = 50781,
        OriginalRawFileName = 50827,
        OriginalRawFileData = 50828,
        ActiveArea = 50829,
        MaskedAreas = 50830,
        AsShotICCProfile = 50831,
        AsShotPreProfileMatrix = 50832,
        CurrentICCProfile = 50833,
        CurrentPreProfileMatrix = 50834,

        // DNG Specification: DNG Specific Tags (v1.2.0.0)

        ColorimetricReference = 50879,
        CameraCalibrationSignature = 50931,
        ProfileCalibrationSignature = 50932,
        ExtraCameraProfiles = 50933,
        AsShotProfileName = 50934,
        NoiseReductionApplied = 50935,
        ProfileName = 50936,
        ProfileHueSatMapDims = 50937,
        ProfileHueSatMapData1 = 50938,
        ProfileHueSatMapData2 = 50939,
        ProfileToneCurve = 50940,
        ProfileEmbedPolicy = 50941,
        ProfileCopyright = 50942,
        ForwardMatrix1 = 50964,
        ForwardMatrix2 = 50965,
        PreviewApplicationName = 50966,
        PreviewApplicationVersion = 50967,
        PreviewSettingsName = 50968,
        PreviewSettingsDigest = 50969,
        PreviewColorSpace = 50970,
        PreviewDateTime = 50971,
        RawImageDigest = 50972,
        OriginalRawFileDigest = 50973,
        SubTileBlockSize = 50974,
        RowInterleaveFactor = 50975,
        ProfileLookTableDims = 50981,
        ProfileLookTableData = 50982,

        // DNG Specification: DNG Specific Tags (v1.3.0.0)

        OpcodeList1 = 51008,
        OpcodeList2 = 51009,
        OpcodeList3 = 51022,
        NoiseProfile = 51041,

        // DNG Specification: DNG Specific Tags (v1.4.0.0)

        DefaultUserCrop = 51125,
        DefaultBlackRender = 51110,
        BaselineExposureOffset = 51109,
        ProfileLookTableEncoding = 51108,
        ProfileHueSatMapEncoding = 51107,
        OriginalDefaultFinalSize = 51089,
        OriginalBestQualityFinalSize = 51090,
        OriginalDefaultCropSize = 51091,
        NewRawImageDigest = 51111,
        RawToPreviewGain = 51112,

        // Embedded Metadata

        Xmp = 700,
        Iptc = 33723,
        Photoshop = 34377,
        ExifIFD = 34665,
        GpsIFD = 34853,
        InteroperabilityIFD = 40965,

        // Other Private TIFF tags (http://www.awaresystems.be/imaging/tiff/tifftags/private.html)

        WangAnnotation = 32932,
        MDFileTag = 33445,
        MDScalePixel = 33446,
        MDColorTable = 33447,
        MDLabName = 33448,
        MDSampleInfo = 33449,
        MDPrepDate = 33450,
        MDPrepTime = 33451,
        MDFileUnits = 33452,
        ModelPixelScaleTag = 33550,
        IngrPacketDataTag = 33918,
        IngrFlagRegisters = 33919,
        IrasBTransformationMatrix = 33920,
        ModelTiePointTag = 33922,
        ModelTransformationTag = 34264,
        IccProfile = 34675,
        GeoKeyDirectoryTag = 34735,
        GeoDoubleParamsTag = 34736,
        GeoAsciiParamsTag = 34737,
        HylaFAXFaxRecvParams = 34908,
        HylaFAXFaxSubAddress = 34909,
        HylaFAXFaxRecvTime = 34910,
        GdalMetadata = 42112,
        GdalNodata = 42113,
        OceScanjobDescription = 50215,
        OceApplicationSelector = 50216,
        OceIdentificationNumber = 50217,
        OceImageLogicCharacteristics = 50218,
        AliasLayerMetadata = 50784
    }
}