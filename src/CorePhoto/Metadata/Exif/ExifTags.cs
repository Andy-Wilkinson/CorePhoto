namespace CorePhoto.Metadata.Exif
{
    public class ExifTags
    {
        // Section A: Tags Relating to Version

        public const int ExifVersion = 36864;
        public const int FlashpixVersion = 40960;

        // Section B: Tags Relating to Image Data Characteristics

        public const int ColorSpace = 40961;
        public const int Gamma = 42240;

        // Section C: Tags Relating to Image Configuration

        public const int ComponentsConfiguration = 37121;
        public const int CompressedBitsPerPixel = 37122;
        public const int PixelXDimension = 40962;
        public const int PixelYDimension = 40963;

        // Section D: Tags Relating to User Information

        public const int MakerNote = 37500;
        public const int UserComment = 37510;

        // Section E: Tags Relating to Related File Information

        public const int RelatedSoundFile = 40964;

        // Section F: Tags Relating to Date and Time

        public const int DateTimeOriginal = 36867;
        public const int DateTimeDigitized = 36868;
        public const int SubSecTime = 37520;
        public const int SubSecOriginal = 37521;
        public const int SubSecDigitized = 37522;

        // Section G: Tags Relating to Picture Taking Conditions

        public const int ExposureTime = 33434;
        public const int FNumber = 33437;
        public const int ExposureProgram = 34850;
        public const int SpectralSensitivity = 34852;
        public const int PhotographicSensitivity = 34855;
        public const int OECF = 34856;
        public const int SensitivityType = 34864;
        public const int StandardOutputSensitivity = 34865;
        public const int RecommendedExposureIndex = 34866;
        public const int ISOSpeed = 34867;
        public const int ISOSpeedLatitudeyyy = 34868;
        public const int ISOSpeedLatitudezzz = 34869;
        public const int ShutterSpeedValue = 37377;
        public const int ApertureValue = 37378;
        public const int BrightnessValue = 37379;
        public const int ExposureBiasValue = 37380;
        public const int MaxApertureValue = 37381;
        public const int SubjectDistance = 37382;
        public const int MeteringMode = 37383;
        public const int LightSource = 37384;
        public const int Flash = 37385;
        public const int FocalLength = 37386;
        public const int SubjectArea = 37396;
        public const int FlashEnergy = 41483;
        public const int SpatialFrequencyResponse = 41484;
        public const int FocalPlaneXResolution = 41486;
        public const int FocalPlaneYResolution = 41487;
        public const int FocalPlaneResolutionUnit = 41488;
        public const int SubjectLocation = 41492;
        public const int ExposureIndex = 41493;
        public const int SensingMethod = 41495;
        public const int FileSource = 41728;
        public const int SceneType = 41729;
        public const int CFAPattern = 41730;
        public const int CustomRendered = 41985;
        public const int ExposureMode = 41986;
        public const int WhiteBalance = 41987;
        public const int DigitalZoomRatio = 41988;
        public const int FocalLengthIn35mmFilm = 41989;
        public const int SceneCaptureType = 41990;
        public const int GainControl = 41991;
        public const int Constrast = 41992;
        public const int Saturation = 41993;
        public const int Sharpness = 41994;
        public const int DeviceSettingDescription = 41995;
        public const int SubjectDistanceRange = 41996;

        // Section H: Other Tags

        public const int ImageUniqueID = 42016;
        public const int CameraOwnerName = 42032;
        public const int BodySerialNumber = 42033;
        public const int LensSpecification = 42034;
        public const int LensMake = 42035;
        public const int LensModel = 42036;
        public const int LensSerialNumber = 42037;
    }
}