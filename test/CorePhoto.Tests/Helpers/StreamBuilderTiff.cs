using CorePhoto.Tiff;

namespace CorePhoto.Tests.Helpers
{
    public static class StreamBuilderTiff
    {
        public static StreamBuilder WriteTiffIfdEntry(this StreamBuilder builder, short tag, TiffType type, int count, byte[] value)
        {
            return builder.WriteInt16(tag)
                          .WriteInt16((short)type)
                          .WriteInt32(count)
                          .WriteBytes(value);
        }
    }
}