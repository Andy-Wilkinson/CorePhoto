namespace CorePhoto.Dng
{
    // TODO : Private IFD Tags (http://www.awaresystems.be/imaging/tiff/tifftags/privateifd.html)

    public class DngTags
    {
        // DNG Specification: DNG Specific Tags

        public const int DNGVersion = 50706;
        public const int DNGBackwardVersion = 50707;
        public const int UniqueCameraModel = 50708;
        public const int LocalizedCameraModel = 50709;
        public const int CFAPlaneColor = 50710;
        public const int CFALayout = 50711;
        public const int LinearizationTable = 50712;
        public const int BlackLevelRepeatDim = 50713;
        public const int BlackLevel = 50714;
        public const int BlackLevelDeltaH = 50715;
        public const int BlackLevelDeltaV = 50716;
        public const int WhiteLevel = 50717;
        public const int DefaultScale = 50718;
        public const int BestQualityScale = 50780;
        public const int DefaultCropOrigin = 50719;
        public const int DefaultCropSize = 50720;
        public const int CalibrationIlluminant1 = 50778;
        public const int CalibrationIlluminant2 = 50779;
        public const int ColorMatrix1 = 50721;
        public const int ColorMatrix2 = 50722;
        public const int CameraCalibration1 = 50723;
        public const int CameraCalibration2 = 50724;
        public const int ReductionMatrix1 = 50725;
        public const int ReductionMatrix2 = 50726;
        public const int AnalogBalance = 50727;
        public const int AsShotNeutral = 50728;
        public const int AsShotWhiteXY = 50729;
        public const int BaselineExposure = 50730;
        public const int BaselineNoise = 50731;
        public const int BaselineSharpness = 50732;
        public const int BayerGreenSplit = 50733;
        public const int LinearResponseLimit = 50734;
        public const int CameraSerialNumber = 50735;
        public const int LensInfo = 50736;
        public const int ChromaBlurRadius = 50737;
        public const int AntiAliasStrength = 50738;
        public const int ShadowScale = 50739;
        public const int DNGPrivateData = 50740;
        public const int MakerNoteSafety = 50741;
        public const int RawDataUniqueID = 50781;
        public const int OriginalRawFileName = 50827;
        public const int OriginalRawFileData = 50828;
        public const int ActiveArea = 50829;
        public const int MaskedAreas = 50830;
        public const int AsShotICCProfile = 50831;
        public const int AsShotPreProfileMatrix = 50832;
        public const int CurrentICCProfile = 50833;
        public const int CurrentPreProfileMatrix = 50834;

        // DNG Specification: DNG Specific Tags (v1.2.0.0)

        public const int ColorimetricReference = 50879;
        public const int CameraCalibrationSignature = 50931;
        public const int ProfileCalibrationSignature = 50932;
        public const int ExtraCameraProfiles = 50933;
        public const int AsShotProfileName = 50934;
        public const int NoiseReductionApplied = 50935;
        public const int ProfileName = 50936;
        public const int ProfileHueSatMapDims = 50937;
        public const int ProfileHueSatMapData1 = 50938;
        public const int ProfileHueSatMapData2 = 50939;
        public const int ProfileToneCurve = 50940;
        public const int ProfileEmbedPolicy = 50941;
        public const int ProfileCopyright = 50942;
        public const int ForwardMatrix1 = 50964;
        public const int ForwardMatrix2 = 50965;
        public const int PreviewApplicationName = 50966;
        public const int PreviewApplicationVersion = 50967;
        public const int PreviewSettingsName = 50968;
        public const int PreviewSettingsDigest = 50969;
        public const int PreviewColorSpace = 50970;
        public const int PreviewDateTime = 50971;
        public const int RawImageDigest = 50972;
        public const int OriginalRawFileDigest = 50973;
        public const int SubTileBlockSize = 50974;
        public const int RowInterleaveFactor = 50975;
        public const int ProfileLookTableDims = 50981;
        public const int ProfileLookTableData = 50982;

        // DNG Specification: DNG Specific Tags (v1.3.0.0)

        public const int OpcodeList1 = 51008;
        public const int OpcodeList2 = 51009;
        public const int OpcodeList3 = 51022;
        public const int NoiseProfile = 51041;

        // DNG Specification: DNG Specific Tags (v1.4.0.0)

        public const int DefaultUserCrop = 51125;
        public const int DefaultBlackRender = 51110;
        public const int BaselineExposureOffset = 51109;
        public const int ProfileLookTableEncoding = 51108;
        public const int ProfileHueSatMapEncoding = 51107;
        public const int OriginalDefaultFinalSize = 51089;
        public const int OriginalBestQualityFinalSize = 51090;
        public const int OriginalDefaultCropSize = 51091;
        public const int NewRawImageDigest = 51111;
        public const int RawToPreviewGain = 51112;
    }
}