using CorePhoto.Tiff;
using Xunit;

namespace CorePhoto.Tests.Helpers
{
    public static class AssertTiff
    {
        public static void Equal(TiffIfdEntry expected, TiffIfdEntry actual)
        {
            Assert.Equal(expected.Tag, actual.Tag);
            Assert.Equal(expected.Type, actual.Type);
            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected.Value, actual.Value);
        }
    }
}