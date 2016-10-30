using System;
using System.Linq;
using System.Threading.Tasks;
using CorePhoto.IO;
using CorePhoto.Numerics;
using CorePhoto.Tests.Helpers;
using CorePhoto.Tiff;
using Xunit;

namespace CorePhoto.Tests.Tiff
{
    public class TiffIfdEntryReaderTests
    {
        public static object[][] ByteOrderValues = new[] { new object[] { ByteOrder.LittleEndian }, new object[] { ByteOrder.BigEndian } };

        [Theory]
        [InlineDataAttribute(TiffType.Byte, 1, 1)]
        [InlineDataAttribute(TiffType.Ascii, 1, 1)]
        [InlineDataAttribute(TiffType.Short, 1, 2)]
        [InlineDataAttribute(TiffType.Long, 1, 4)]
        [InlineDataAttribute(TiffType.Rational, 1, 8)]
        [InlineDataAttribute(TiffType.SByte, 1, 1)]
        [InlineDataAttribute(TiffType.Undefined, 1, 1)]
        [InlineDataAttribute(TiffType.SShort, 1, 2)]
        [InlineDataAttribute(TiffType.SLong, 1, 4)]
        [InlineDataAttribute(TiffType.SRational, 1, 8)]
        [InlineDataAttribute(TiffType.Float, 1, 4)]
        [InlineDataAttribute(TiffType.Double, 1, 8)]
        [InlineDataAttribute(TiffType.Ifd, 1, 4)]
        [InlineDataAttribute((TiffType)999, 1, 0)]

        public void SizeOfData_SingleItem_ReturnsCorrectSize(TiffType type, int count, int expectedSize)
        {
            var entry = new TiffIfdEntry { Type = type, Count = count };

            var size = entry.SizeOfData();

            Assert.Equal(expectedSize, size);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte, 15, 15)]
        [InlineDataAttribute(TiffType.Ascii, 20, 20)]
        [InlineDataAttribute(TiffType.Short, 18, 36)]
        [InlineDataAttribute(TiffType.Long, 4, 16)]
        [InlineDataAttribute(TiffType.Rational, 9, 72)]
        [InlineDataAttribute(TiffType.SByte, 5, 5)]
        [InlineDataAttribute(TiffType.Undefined, 136, 136)]
        [InlineDataAttribute(TiffType.SShort, 12, 24)]
        [InlineDataAttribute(TiffType.SLong, 15, 60)]
        [InlineDataAttribute(TiffType.SRational, 10, 80)]
        [InlineDataAttribute(TiffType.Float, 2, 8)]
        [InlineDataAttribute(TiffType.Double, 2, 16)]
        [InlineDataAttribute(TiffType.Ifd, 10, 40)]
        [InlineDataAttribute((TiffType)999, 1050, 0)]

        public void SizeOfData_Array_ReturnsCorrectSize(TiffType type, int count, int expectedSize)
        {
            var entry = new TiffIfdEntry { Type = type, Count = count };

            var size = entry.SizeOfData();

            Assert.Equal(expectedSize, size);
        }

        [Theory]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 1, 2, 3, 4 }, 3, new byte[] { }, new byte[] { 1, 2, 3, 4 })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4 }, 3, new byte[] { }, new byte[] { 1, 2, 3, 4 })]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 1, 2, 3, 4 }, 4, new byte[] { }, new byte[] { 1, 2, 3, 4 })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4 }, 4, new byte[] { }, new byte[] { 1, 2, 3, 4 })]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 4, 0, 0, 0 }, 5, new byte[] { 0, 0, 0, 0, 1, 2, 3, 4, 5 }, new byte[] { 1, 2, 3, 4, 5 })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 0, 0, 0, 4 }, 5, new byte[] { 0, 0, 0, 0, 1, 2, 3, 4, 5 }, new byte[] { 1, 2, 3, 4, 5 })]
        public async Task ReadDataAsync_ReturnsExpectedData_Byte(ByteOrder byteOrder, byte[] ifdValue, int count, byte[] data, byte[] expectedValue)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WriteBytes(data)
                                    .ToStream();

            var entry = new TiffIfdEntry { Type = TiffType.Byte, Count = count, Value = ifdValue };
            var value = await entry.ReadDataAsync(stream, byteOrder);

            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte, ByteOrder.LittleEndian, new byte[] { 0, 1, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.Byte, ByteOrder.LittleEndian, new byte[] { 1, 2, 3, 4 }, 1)]
        [InlineDataAttribute(TiffType.Byte, ByteOrder.LittleEndian, new byte[] { 255, 2, 3, 4 }, 255)]
        [InlineDataAttribute(TiffType.Byte, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.Byte, ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4 }, 1)]
        [InlineDataAttribute(TiffType.Byte, ByteOrder.BigEndian, new byte[] { 255, 2, 3, 4 }, 255)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.LittleEndian, new byte[] { 0, 0, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.LittleEndian, new byte[] { 1, 0, 2, 3 }, 1)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.LittleEndian, new byte[] { 0, 1, 2, 3 }, 256)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.LittleEndian, new byte[] { 2, 1, 2, 3 }, 258)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.LittleEndian, new byte[] { 255, 255, 2, 3 }, UInt16.MaxValue)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.BigEndian, new byte[] { 0, 0, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3 }, 1)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.BigEndian, new byte[] { 1, 0, 2, 3 }, 256)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.BigEndian, new byte[] { 1, 2, 2, 3 }, 258)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.BigEndian, new byte[] { 255, 255, 2, 3 }, UInt16.MaxValue)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 0 }, 0)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 1, 0, 0, 0 }, 1)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 0, 1, 0, 0 }, 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 0, 0, 1, 0 }, 256 * 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 1 }, 256 * 256 * 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 1, 2, 3, 4 }, 67305985)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 255, 255, 255, 255 }, UInt32.MaxValue)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 0, 0, 0, 0 }, 0)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 0, 0, 0, 1 }, 1)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 0, 0, 1, 0 }, 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 0, 1, 0, 0 }, 256 * 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 1, 0, 0, 0 }, 256 * 256 * 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 4, 3, 2, 1 }, 67305985)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 255, 255, 255, 255 }, UInt32.MaxValue)]
        public void GetInteger_ReturnsValue(TiffType type, ByteOrder byteOrder, byte[] data, uint expectedValue)
        {
            var entry = new TiffIfdEntry { Type = type, Count = 1, Value = data };

            var value = entry.GetInteger(byteOrder);

            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Ascii)]
        [InlineDataAttribute(TiffType.Rational)]
        [InlineDataAttribute(TiffType.SByte)]
        [InlineDataAttribute(TiffType.Undefined)]
        [InlineDataAttribute(TiffType.SShort)]
        [InlineDataAttribute(TiffType.SLong)]
        [InlineDataAttribute(TiffType.SRational)]
        [InlineDataAttribute(TiffType.Float)]
        [InlineDataAttribute(TiffType.Double)]
        [InlineDataAttribute(TiffType.Ifd)]
        [InlineDataAttribute((TiffType)99)]
        public void GetInteger_ThrowsExceptionIfInvalidType(TiffType type)
        {
            var entry = new TiffIfdEntry { Type = type, Count = 1 };

            var e = Assert.Throws<ImageFormatException>(() => entry.GetInteger(ByteOrder.LittleEndian));

            Assert.Equal($"A value of type '{type}' cannot be converted to an unsigned integer.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte, ByteOrder.LittleEndian)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.LittleEndian)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian)]
        [InlineDataAttribute(TiffType.Byte, ByteOrder.BigEndian)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.BigEndian)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian)]
        public void GetInteger_ThrowsExceptionIfCountIsNotOne(TiffType type, ByteOrder byteOrder)
        {
            var entry = new TiffIfdEntry { Type = type, Count = 2 };

            var e = Assert.Throws<ImageFormatException>(() => entry.GetInteger(byteOrder));

            Assert.Equal($"Cannot read a single value from an array of multiple items.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(TiffType.SByte, ByteOrder.LittleEndian, new byte[] { 0, 1, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.SByte, ByteOrder.LittleEndian, new byte[] { 1, 2, 3, 4 }, 1)]
        [InlineDataAttribute(TiffType.SByte, ByteOrder.LittleEndian, new byte[] { 255, 2, 3, 4 }, -1)]
        [InlineDataAttribute(TiffType.SByte, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.SByte, ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4 }, 1)]
        [InlineDataAttribute(TiffType.SByte, ByteOrder.BigEndian, new byte[] { 255, 2, 3, 4 }, -1)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.LittleEndian, new byte[] { 0, 0, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.LittleEndian, new byte[] { 1, 0, 2, 3 }, 1)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.LittleEndian, new byte[] { 0, 1, 2, 3 }, 256)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.LittleEndian, new byte[] { 2, 1, 2, 3 }, 258)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.LittleEndian, new byte[] { 255, 255, 2, 3 }, -1)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.BigEndian, new byte[] { 0, 0, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3 }, 1)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.BigEndian, new byte[] { 1, 0, 2, 3 }, 256)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.BigEndian, new byte[] { 1, 2, 2, 3 }, 258)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.BigEndian, new byte[] { 255, 255, 2, 3 }, -1)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 0 }, 0)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian, new byte[] { 1, 0, 0, 0 }, 1)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian, new byte[] { 0, 1, 0, 0 }, 256)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian, new byte[] { 0, 0, 1, 0 }, 256 * 256)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 1 }, 256 * 256 * 256)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian, new byte[] { 1, 2, 3, 4 }, 67305985)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian, new byte[] { 255, 255, 255, 255 }, -1)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian, new byte[] { 0, 0, 0, 0 }, 0)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian, new byte[] { 0, 0, 0, 1 }, 1)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian, new byte[] { 0, 0, 1, 0 }, 256)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian, new byte[] { 0, 1, 0, 0 }, 256 * 256)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian, new byte[] { 1, 0, 0, 0 }, 256 * 256 * 256)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian, new byte[] { 4, 3, 2, 1 }, 67305985)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian, new byte[] { 255, 255, 255, 255 }, -1)]
        public void GetSignedInteger_ReturnsValue(TiffType type, ByteOrder byteOrder, byte[] data, int expectedValue)
        {
            var entry = new TiffIfdEntry { Type = type, Count = 1, Value = data };

            var value = entry.GetSignedInteger(byteOrder);

            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte)]
        [InlineDataAttribute(TiffType.Ascii)]
        [InlineDataAttribute(TiffType.Short)]
        [InlineDataAttribute(TiffType.Long)]
        [InlineDataAttribute(TiffType.Rational)]
        [InlineDataAttribute(TiffType.Undefined)]
        [InlineDataAttribute(TiffType.SRational)]
        [InlineDataAttribute(TiffType.Float)]
        [InlineDataAttribute(TiffType.Double)]
        [InlineDataAttribute(TiffType.Ifd)]
        [InlineDataAttribute((TiffType)99)]
        public void GetSignedInteger_ThrowsExceptionIfInvalidType(TiffType type)
        {
            var entry = new TiffIfdEntry { Type = type, Count = 1 };

            var e = Assert.Throws<ImageFormatException>(() => entry.GetSignedInteger(ByteOrder.LittleEndian));

            Assert.Equal($"A value of type '{type}' cannot be converted to a signed integer.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(TiffType.SByte, ByteOrder.LittleEndian)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.LittleEndian)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian)]
        [InlineDataAttribute(TiffType.SByte, ByteOrder.BigEndian)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.BigEndian)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian)]
        public void GetSignedInteger_ThrowsExceptionIfCountIsNotOne(TiffType type, ByteOrder byteOrder)
        {
            var entry = new TiffIfdEntry { Type = type, Count = 2 };

            var e = Assert.Throws<ImageFormatException>(() => entry.GetSignedInteger(byteOrder));

            Assert.Equal($"Cannot read a single value from an array of multiple items.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 0 }, 0)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 1, 0, 0, 0 }, 1)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 0, 1, 0, 0 }, 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 0, 0, 1, 0 }, 256 * 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 1 }, 256 * 256 * 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 1, 2, 3, 4 }, 67305985)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 255, 255, 255, 255 }, UInt32.MaxValue)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 0, 0, 0, 0 }, 0)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 0, 0, 0, 1 }, 1)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 0, 0, 1, 0 }, 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 0, 1, 0, 0 }, 256 * 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 1, 0, 0, 0 }, 256 * 256 * 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 4, 3, 2, 1 }, 67305985)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 255, 255, 255, 255 }, UInt32.MaxValue)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 0 }, 0)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.LittleEndian, new byte[] { 1, 0, 0, 0 }, 1)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.LittleEndian, new byte[] { 0, 1, 0, 0 }, 256)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.LittleEndian, new byte[] { 0, 0, 1, 0 }, 256 * 256)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 1 }, 256 * 256 * 256)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.LittleEndian, new byte[] { 1, 2, 3, 4 }, 67305985)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.LittleEndian, new byte[] { 255, 255, 255, 255 }, UInt32.MaxValue)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.BigEndian, new byte[] { 0, 0, 0, 0 }, 0)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.BigEndian, new byte[] { 0, 0, 0, 1 }, 1)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.BigEndian, new byte[] { 0, 0, 1, 0 }, 256)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.BigEndian, new byte[] { 0, 1, 0, 0 }, 256 * 256)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.BigEndian, new byte[] { 1, 0, 0, 0 }, 256 * 256 * 256)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.BigEndian, new byte[] { 4, 3, 2, 1 }, 67305985)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.BigEndian, new byte[] { 255, 255, 255, 255 }, UInt32.MaxValue)]
        public void GetIfdReference_ReturnsValue(TiffType type, ByteOrder byteOrder, byte[] data, uint expectedOffset)
        {
            var entry = new TiffIfdEntry { Type = type, Count = 1, Value = data };

            var value = entry.GetIfdReference(byteOrder);

            Assert.Equal(new TiffIfdReference(expectedOffset), value);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte)]
        [InlineDataAttribute(TiffType.Ascii)]
        [InlineDataAttribute(TiffType.Short)]
        [InlineDataAttribute(TiffType.Rational)]
        [InlineDataAttribute(TiffType.SByte)]
        [InlineDataAttribute(TiffType.Undefined)]
        [InlineDataAttribute(TiffType.SShort)]
        [InlineDataAttribute(TiffType.SLong)]
        [InlineDataAttribute(TiffType.SRational)]
        [InlineDataAttribute(TiffType.Float)]
        [InlineDataAttribute(TiffType.Double)]
        [InlineDataAttribute((TiffType)99)]
        public void GetIfdReference_ThrowsExceptionIfInvalidType(TiffType type)
        {
            var entry = new TiffIfdEntry { Type = type, Count = 1 };

            var e = Assert.Throws<ImageFormatException>(() => entry.GetIfdReference(ByteOrder.LittleEndian));

            Assert.Equal($"A value of type '{type}' cannot be converted to an IFD reference.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.LittleEndian)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.BigEndian)]
        public void GetIfdReference_ThrowsExceptionIfCountIsNotOne(TiffType type, ByteOrder byteOrder)
        {
            var entry = new TiffIfdEntry { Type = type, Count = 2 };

            var e = Assert.Throws<ImageFormatException>(() => entry.GetIfdReference(byteOrder));

            Assert.Equal($"Cannot read a single value from an array of multiple items.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte, 1, ByteOrder.LittleEndian, new byte[] { 0, 1, 2, 3 }, new uint[] { 0 })]
        [InlineDataAttribute(TiffType.Byte, 3, ByteOrder.LittleEndian, new byte[] { 0, 1, 2, 3 }, new uint[] { 0, 1, 2 })]
        [InlineDataAttribute(TiffType.Byte, 7, ByteOrder.LittleEndian, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new uint[] { 0, 1, 2, 3, 4, 5, 6 })]
        [InlineDataAttribute(TiffType.Byte, 1, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3 }, new uint[] { 0 })]
        [InlineDataAttribute(TiffType.Byte, 3, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3 }, new uint[] { 0, 1, 2 })]
        [InlineDataAttribute(TiffType.Byte, 7, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new uint[] { 0, 1, 2, 3, 4, 5, 6 })]
        [InlineDataAttribute(TiffType.Short, 1, ByteOrder.LittleEndian, new byte[] { 1, 0, 3, 2 }, new uint[] { 1 })]
        [InlineDataAttribute(TiffType.Short, 2, ByteOrder.LittleEndian, new byte[] { 1, 0, 3, 2 }, new uint[] { 1, 515 })]
        [InlineDataAttribute(TiffType.Short, 3, ByteOrder.LittleEndian, new byte[] { 1, 0, 3, 2, 5, 4, 6, 7, 8 }, new uint[] { 1, 515, 1029 })]
        [InlineDataAttribute(TiffType.Short, 1, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3 }, new uint[] { 1 })]
        [InlineDataAttribute(TiffType.Short, 2, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3 }, new uint[] { 1, 515 })]
        [InlineDataAttribute(TiffType.Short, 3, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new uint[] { 1, 515, 1029 })]
        [InlineDataAttribute(TiffType.Long, 1, ByteOrder.LittleEndian, new byte[] { 4, 3, 2, 1 }, new uint[] { 0x01020304 })]
        [InlineDataAttribute(TiffType.Long, 2, ByteOrder.LittleEndian, new byte[] { 4, 3, 2, 1, 6, 5, 4, 3, 99, 99 }, new uint[] { 0x01020304, 0x03040506 })]
        [InlineDataAttribute(TiffType.Long, 1, ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4 }, new uint[] { 0x01020304 })]
        [InlineDataAttribute(TiffType.Long, 2, ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4, 3, 4, 5, 6, 99, 99 }, new uint[] { 0x01020304, 0x03040506 })]
        public async Task ReadIntegerArrayAsync_ReturnsValue(TiffType type, int count, ByteOrder byteOrder, byte[] data, uint[] expectedValue)
        {
            var entryTuple = TiffHelper.GenerateTiffIfdEntry(type, data, 6, byteOrder, count);
            var entry = entryTuple.Entry;
            var stream = entryTuple.Stream;

            var value = await entry.ReadIntegerArrayAsync(stream, byteOrder);

            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Ascii)]
        [InlineDataAttribute(TiffType.Rational)]
        [InlineDataAttribute(TiffType.SByte)]
        [InlineDataAttribute(TiffType.Undefined)]
        [InlineDataAttribute(TiffType.SShort)]
        [InlineDataAttribute(TiffType.SLong)]
        [InlineDataAttribute(TiffType.SRational)]
        [InlineDataAttribute(TiffType.Float)]
        [InlineDataAttribute(TiffType.Double)]
        [InlineDataAttribute(TiffType.Ifd)]
        [InlineDataAttribute((TiffType)99)]
        public void ReadIntegerArrayAsync_ThrowsExceptionIfInvalidType(TiffType type)
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian).ToStream();
            var entry = new TiffIfdEntry { Type = type, Count = 10 };

            var e = Assert.Throws<ImageFormatException>(() => { entry.ReadIntegerArrayAsync(stream, ByteOrder.LittleEndian); });

            Assert.Equal($"A value of type '{type}' cannot be converted to an unsigned integer.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(TiffType.SByte, 1, ByteOrder.LittleEndian, new byte[] { 0, 1, 2, 3 }, new int[] { 0 })]
        [InlineDataAttribute(TiffType.SByte, 3, ByteOrder.LittleEndian, new byte[] { 0, 255, 2, 3 }, new int[] { 0, -1, 2 })]
        [InlineDataAttribute(TiffType.SByte, 7, ByteOrder.LittleEndian, new byte[] { 0, 255, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, -1, 2, 3, 4, 5, 6 })]
        [InlineDataAttribute(TiffType.SByte, 1, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3 }, new int[] { 0 })]
        [InlineDataAttribute(TiffType.SByte, 3, ByteOrder.BigEndian, new byte[] { 0, 255, 2, 3 }, new int[] { 0, -1, 2 })]
        [InlineDataAttribute(TiffType.SByte, 7, ByteOrder.BigEndian, new byte[] { 0, 255, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, -1, 2, 3, 4, 5, 6 })]
        [InlineDataAttribute(TiffType.SShort, 1, ByteOrder.LittleEndian, new byte[] { 1, 0, 3, 2 }, new int[] { 1 })]
        [InlineDataAttribute(TiffType.SShort, 2, ByteOrder.LittleEndian, new byte[] { 1, 0, 255, 255 }, new int[] { 1, -1 })]
        [InlineDataAttribute(TiffType.SShort, 3, ByteOrder.LittleEndian, new byte[] { 1, 0, 255, 255, 5, 4, 6, 7, 8 }, new int[] { 1, -1, 1029 })]
        [InlineDataAttribute(TiffType.SShort, 1, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3 }, new int[] { 1 })]
        [InlineDataAttribute(TiffType.SShort, 2, ByteOrder.BigEndian, new byte[] { 0, 1, 255, 255 }, new int[] { 1, -1 })]
        [InlineDataAttribute(TiffType.SShort, 3, ByteOrder.BigEndian, new byte[] { 0, 1, 255, 255, 4, 5, 6, 7, 8 }, new int[] { 1, -1, 1029 })]
        [InlineDataAttribute(TiffType.SLong, 1, ByteOrder.LittleEndian, new byte[] { 4, 3, 2, 1 }, new int[] { 0x01020304 })]
        [InlineDataAttribute(TiffType.SLong, 2, ByteOrder.LittleEndian, new byte[] { 4, 3, 2, 1, 255, 255, 255, 255, 99, 99 }, new int[] { 0x01020304, -1 })]
        [InlineDataAttribute(TiffType.SLong, 1, ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4 }, new int[] { 0x01020304 })]
        [InlineDataAttribute(TiffType.SLong, 2, ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4, 255, 255, 255, 255, 99, 99 }, new int[] { 0x01020304, -1 })]
        public async Task ReadSignedIntegerArrayAsync_ReturnsValue(TiffType type, int count, ByteOrder byteOrder, byte[] data, int[] expectedValue)
        {
            var entryTuple = TiffHelper.GenerateTiffIfdEntry(type, data, 6, byteOrder, count);
            var entry = entryTuple.Entry;
            var stream = entryTuple.Stream;

            var value = await entry.ReadSignedIntegerArrayAsync(stream, byteOrder);

            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte)]
        [InlineDataAttribute(TiffType.Ascii)]
        [InlineDataAttribute(TiffType.Short)]
        [InlineDataAttribute(TiffType.Long)]
        [InlineDataAttribute(TiffType.Rational)]
        [InlineDataAttribute(TiffType.Undefined)]
        [InlineDataAttribute(TiffType.SRational)]
        [InlineDataAttribute(TiffType.Float)]
        [InlineDataAttribute(TiffType.Double)]
        [InlineDataAttribute(TiffType.Ifd)]
        [InlineDataAttribute((TiffType)99)]
        public void ReadSignedIntegerArrayAsync_ThrowsExceptionIfInvalidType(TiffType type)
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian).ToStream();
            var entry = new TiffIfdEntry { Type = type, Count = 10 };

            var e = Assert.Throws<ImageFormatException>(() => { entry.ReadSignedIntegerArrayAsync(stream, ByteOrder.LittleEndian); });

            Assert.Equal($"A value of type '{type}' cannot be converted to a signed integer.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 0 }, "")]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { (byte)'A', (byte)'B', (byte)'C', 0 }, "ABC")]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', 0 }, "ABCDEF")]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { (byte)'A', (byte)'B', (byte)'C', (byte)'D', 0, (byte)'E', (byte)'F', (byte)'G', (byte)'H', 0 }, "ABCD\0EFGH")]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 0 }, "")]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { (byte)'A', (byte)'B', (byte)'C', 0 }, "ABC")]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', 0 }, "ABCDEF")]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { (byte)'A', (byte)'B', (byte)'C', (byte)'D', 0, (byte)'E', (byte)'F', (byte)'G', (byte)'H', 0 }, "ABCD\0EFGH")]
        public async Task ReadStringAsync_ReturnsValue(ByteOrder byteOrder, byte[] data, string expectedValue)
        {
            var entryTuple = TiffHelper.GenerateTiffIfdEntry(TiffType.Ascii, data, 6, byteOrder);
            var entry = entryTuple.Entry;
            var stream = entryTuple.Stream;

            var value = await entry.ReadStringAsync(stream, byteOrder);

            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte)]
        [InlineDataAttribute(TiffType.Short)]
        [InlineDataAttribute(TiffType.Long)]
        [InlineDataAttribute(TiffType.Rational)]
        [InlineDataAttribute(TiffType.SByte)]
        [InlineDataAttribute(TiffType.Undefined)]
        [InlineDataAttribute(TiffType.SShort)]
        [InlineDataAttribute(TiffType.SLong)]
        [InlineDataAttribute(TiffType.SRational)]
        [InlineDataAttribute(TiffType.Float)]
        [InlineDataAttribute(TiffType.Double)]
        [InlineDataAttribute(TiffType.Ifd)]
        [InlineDataAttribute((TiffType)99)]
        public void ReadStringAsync_ThrowsExceptionIfInvalidType(TiffType type)
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian).ToStream();
            var entry = new TiffIfdEntry { Type = type, Count = 10 };

            var e = Assert.Throws<ImageFormatException>(() => { entry.ReadStringAsync(stream, ByteOrder.LittleEndian); });

            Assert.Equal($"A value of type '{type}' cannot be converted to a string.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { (byte)'A' })]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { (byte)'A', (byte)'B', (byte)'C' })]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F' })]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { (byte)'A', (byte)'B', (byte)'C', (byte)'D', 0, (byte)'E', (byte)'F', (byte)'G', (byte)'H' })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { (byte)'A' })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { (byte)'A', (byte)'B', (byte)'C' })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F' })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { (byte)'A', (byte)'B', (byte)'C', (byte)'D', 0, (byte)'E', (byte)'F', (byte)'G', (byte)'H' })]
        public async Task ReadStringAsync_ThrowsExceptionIfStringIsNotNullTerminated(ByteOrder byteOrder, byte[] data)
        {
            var entryTuple = TiffHelper.GenerateTiffIfdEntry(TiffType.Ascii, data, 6, byteOrder);
            var entry = entryTuple.Entry;
            var stream = entryTuple.Stream;

            var e = await Assert.ThrowsAsync<ImageFormatException>(() => entry.ReadStringAsync(stream, byteOrder));

            Assert.Equal($"The retrieved string is not null terminated.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 0, 2, 0, 0, 0 }, 0, 2)]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 1, 0, 0, 0, 2, 0, 0, 0 }, 1, 2)]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 0, 0, 0, 0, 0, 0, 0, 2 }, 0, 2)]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 0, 0, 0, 1, 0, 0, 0, 2 }, 1, 2)]
        public async Task ReadRationalAsync_ReturnsValue(ByteOrder byteOrder, byte[] data, uint expectedNumerator, uint expectedDenominator)
        {
            var entryTuple = TiffHelper.GenerateTiffIfdEntry(TiffType.Rational, data, 6, byteOrder);
            var entry = entryTuple.Entry;
            var stream = entryTuple.Stream;

            var value = await entry.ReadRationalAsync(stream, byteOrder);

            Assert.Equal(expectedNumerator, value.Numerator);
            Assert.Equal(expectedDenominator, value.Denominator);
        }

        [Theory]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 0, 2, 0, 0, 0 }, 0, 2)]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 1, 0, 0, 0, 2, 0, 0, 0 }, 1, 2)]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 255, 255, 255, 255, 2, 0, 0, 0 }, -1, 2)]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 0, 0, 0, 0, 0, 0, 0, 2 }, 0, 2)]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 0, 0, 0, 1, 0, 0, 0, 2 }, 1, 2)]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 255, 255, 255, 255, 0, 0, 0, 2 }, -1, 2)]
        public async Task ReadSignedRationalAsync_ReturnsValue(ByteOrder byteOrder, byte[] data, int expectedNumerator, int expectedDenominator)
        {
            var entryTuple = TiffHelper.GenerateTiffIfdEntry(TiffType.SRational, data, 6, byteOrder);
            var entry = entryTuple.Entry;
            var stream = entryTuple.Stream;

            var value = await entry.ReadSignedRationalAsync(stream, byteOrder);

            Assert.Equal(expectedNumerator, value.Numerator);
            Assert.Equal(expectedDenominator, value.Denominator);
        }

        [Theory]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 0, 2, 0, 0, 0 }, new uint[] { 0 }, new uint[] { 2 })]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 1, 0, 0, 0, 2, 0, 0, 0 }, new uint[] { 1 }, new uint[] { 2 })]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 1, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0 }, new uint[] { 1, 2 }, new uint[] { 2, 3 })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 0, 0, 0, 0, 0, 0, 0, 2 }, new uint[] { 0 }, new uint[] { 2 })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 0, 0, 0, 1, 0, 0, 0, 2 }, new uint[] { 1 }, new uint[] { 2 })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 3 }, new uint[] { 1, 2 }, new uint[] { 2, 3 })]
        public async Task ReadRationalArrayAsync_ReturnsValue(ByteOrder byteOrder, byte[] data, uint[] expectedNumerators, uint[] expectedDenominators)
        {
            var entryTuple = TiffHelper.GenerateTiffIfdEntry(TiffType.Rational, data, 6, byteOrder);
            var entry = entryTuple.Entry;
            var stream = entryTuple.Stream;

            var value = await entry.ReadRationalArrayAsync(stream, byteOrder);

            var expectedValues = Enumerable.Range(0, expectedNumerators.Length).Select(i => new Rational(expectedNumerators[i], expectedDenominators[i])).ToArray();

            Assert.Equal(expectedValues, value);
        }

        [Theory]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 0, 2, 0, 0, 0 }, new int[] { 0 }, new int[] { 2 })]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 1, 0, 0, 0, 2, 0, 0, 0 }, new int[] { 1 }, new int[] { 2 })]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 255, 255, 255, 255, 2, 0, 0, 0 }, new int[] { -1 }, new int[] { 2 })]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 255, 255, 255, 255, 2, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0 }, new int[] { -1, 2 }, new int[] { 2, 3 })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 0, 0, 0, 0, 0, 0, 0, 2 }, new int[] { 0 }, new int[] { 2 })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 0, 0, 0, 1, 0, 0, 0, 2 }, new int[] { 1 }, new int[] { 2 })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 255, 255, 255, 255, 0, 0, 0, 2 }, new int[] { -1 }, new int[] { 2 })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 255, 255, 255, 255, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 3 }, new int[] { -1, 2 }, new int[] { 2, 3 })]
        public async Task ReadSignedRationalArrayAsync_ReturnsValue(ByteOrder byteOrder, byte[] data, int[] expectedNumerators, int[] expectedDenominators)
        {
            var entryTuple = TiffHelper.GenerateTiffIfdEntry(TiffType.SRational, data, 6, byteOrder);
            var entry = entryTuple.Entry;
            var stream = entryTuple.Stream;

            var value = await entry.ReadSignedRationalArrayAsync(stream, byteOrder);

            var expectedValues = Enumerable.Range(0, expectedNumerators.Length).Select(i => new SignedRational(expectedNumerators[i], expectedDenominators[i])).ToArray();

            Assert.Equal(expectedValues, value);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte)]
        [InlineDataAttribute(TiffType.Ascii)]
        [InlineDataAttribute(TiffType.Short)]
        [InlineDataAttribute(TiffType.Long)]
        [InlineDataAttribute(TiffType.SByte)]
        [InlineDataAttribute(TiffType.Undefined)]
        [InlineDataAttribute(TiffType.SShort)]
        [InlineDataAttribute(TiffType.SLong)]
        [InlineDataAttribute(TiffType.SRational)]
        [InlineDataAttribute(TiffType.Float)]
        [InlineDataAttribute(TiffType.Double)]
        [InlineDataAttribute(TiffType.Ifd)]
        [InlineDataAttribute((TiffType)99)]
        public void ReadRationalAsync_ThrowsExceptionIfInvalidType(TiffType type)
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian).ToStream();
            var entry = new TiffIfdEntry { Type = type, Count = 1 };

            var e = Assert.Throws<ImageFormatException>(() => { entry.ReadRationalAsync(stream, ByteOrder.LittleEndian); });

            Assert.Equal($"A value of type '{type}' cannot be converted to a Rational.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte)]
        [InlineDataAttribute(TiffType.Ascii)]
        [InlineDataAttribute(TiffType.Short)]
        [InlineDataAttribute(TiffType.Long)]
        [InlineDataAttribute(TiffType.SByte)]
        [InlineDataAttribute(TiffType.Rational)]
        [InlineDataAttribute(TiffType.Undefined)]
        [InlineDataAttribute(TiffType.SShort)]
        [InlineDataAttribute(TiffType.SLong)]
        [InlineDataAttribute(TiffType.Float)]
        [InlineDataAttribute(TiffType.Double)]
        [InlineDataAttribute(TiffType.Ifd)]
        [InlineDataAttribute((TiffType)99)]
        public void ReadSignedRationalAsync_ThrowsExceptionIfInvalidType(TiffType type)
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian).ToStream();
            var entry = new TiffIfdEntry { Type = type, Count = 1 };

            var e = Assert.Throws<ImageFormatException>(() => { entry.ReadSignedRationalAsync(stream, ByteOrder.LittleEndian); });

            Assert.Equal($"A value of type '{type}' cannot be converted to a SignedRational.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte)]
        [InlineDataAttribute(TiffType.Ascii)]
        [InlineDataAttribute(TiffType.Short)]
        [InlineDataAttribute(TiffType.Long)]
        [InlineDataAttribute(TiffType.SByte)]
        [InlineDataAttribute(TiffType.Undefined)]
        [InlineDataAttribute(TiffType.SShort)]
        [InlineDataAttribute(TiffType.SLong)]
        [InlineDataAttribute(TiffType.SRational)]
        [InlineDataAttribute(TiffType.Float)]
        [InlineDataAttribute(TiffType.Double)]
        [InlineDataAttribute(TiffType.Ifd)]
        [InlineDataAttribute((TiffType)99)]
        public void ReadRationalArrayAsync_ThrowsExceptionIfInvalidType(TiffType type)
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian).ToStream();
            var entry = new TiffIfdEntry { Type = type, Count = 10 };

            var e = Assert.Throws<ImageFormatException>(() => { entry.ReadRationalArrayAsync(stream, ByteOrder.LittleEndian); });

            Assert.Equal($"A value of type '{type}' cannot be converted to a Rational.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte)]
        [InlineDataAttribute(TiffType.Ascii)]
        [InlineDataAttribute(TiffType.Short)]
        [InlineDataAttribute(TiffType.Long)]
        [InlineDataAttribute(TiffType.Rational)]
        [InlineDataAttribute(TiffType.SByte)]
        [InlineDataAttribute(TiffType.Undefined)]
        [InlineDataAttribute(TiffType.SShort)]
        [InlineDataAttribute(TiffType.SLong)]
        [InlineDataAttribute(TiffType.Float)]
        [InlineDataAttribute(TiffType.Double)]
        [InlineDataAttribute(TiffType.Ifd)]
        [InlineDataAttribute((TiffType)99)]
        public void ReadSignedRationalArrayAsync_ThrowsExceptionIfInvalidType(TiffType type)
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian).ToStream();
            var entry = new TiffIfdEntry { Type = type, Count = 10 };

            var e = Assert.Throws<ImageFormatException>(() => { entry.ReadSignedRationalArrayAsync(stream, ByteOrder.LittleEndian); });

            Assert.Equal($"A value of type '{type}' cannot be converted to a SignedRational.", e.Message);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public void ReadRationalAsync_ThrowsExceptionIfCountIsNotOne(ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian).ToStream();
            var entry = new TiffIfdEntry { Type = TiffType.Rational, Count = 2 };

            var e = Assert.Throws<ImageFormatException>(() => { entry.ReadRationalAsync(stream, byteOrder); });

            Assert.Equal($"Cannot read a single value from an array of multiple items.", e.Message);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public void ReadSignedRationalAsync_ThrowsExceptionIfCountIsNotOne(ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian).ToStream();
            var entry = new TiffIfdEntry { Type = TiffType.SRational, Count = 2 };

            var e = Assert.Throws<ImageFormatException>(() => { entry.ReadSignedRationalAsync(stream, byteOrder); });

            Assert.Equal($"Cannot read a single value from an array of multiple items.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Long, 1, ByteOrder.LittleEndian, new byte[] { 4, 3, 2, 1 }, new uint[] { 0x01020304 })]
        [InlineDataAttribute(TiffType.Long, 2, ByteOrder.LittleEndian, new byte[] { 4, 3, 2, 1, 6, 5, 4, 3, 99, 99 }, new uint[] { 0x01020304, 0x03040506 })]
        [InlineDataAttribute(TiffType.Long, 1, ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4 }, new uint[] { 0x01020304 })]
        [InlineDataAttribute(TiffType.Long, 2, ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4, 3, 4, 5, 6, 99, 99 }, new uint[] { 0x01020304, 0x03040506 })]
        [InlineDataAttribute(TiffType.Ifd, 1, ByteOrder.LittleEndian, new byte[] { 4, 3, 2, 1 }, new uint[] { 0x01020304 })]
        [InlineDataAttribute(TiffType.Ifd, 2, ByteOrder.LittleEndian, new byte[] { 4, 3, 2, 1, 6, 5, 4, 3, 99, 99 }, new uint[] { 0x01020304, 0x03040506 })]
        [InlineDataAttribute(TiffType.Ifd, 1, ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4 }, new uint[] { 0x01020304 })]
        [InlineDataAttribute(TiffType.Ifd, 2, ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4, 3, 4, 5, 6, 99, 99 }, new uint[] { 0x01020304, 0x03040506 })]
        public async Task ReadIfdReferenceArrayAsync_ReturnsValue(TiffType type, int count, ByteOrder byteOrder, byte[] data, uint[] expectedOffsets)
        {
            var entryTuple = TiffHelper.GenerateTiffIfdEntry(type, data, 6, byteOrder, count);
            var entry = entryTuple.Entry;
            var stream = entryTuple.Stream;

            var value = await entry.ReadIfdReferenceArrayAsync(stream, byteOrder);

            var expectedValue = expectedOffsets.Select(offset => new TiffIfdReference(offset));
            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte)]
        [InlineDataAttribute(TiffType.Ascii)]
        [InlineDataAttribute(TiffType.Short)]
        [InlineDataAttribute(TiffType.Rational)]
        [InlineDataAttribute(TiffType.SByte)]
        [InlineDataAttribute(TiffType.Undefined)]
        [InlineDataAttribute(TiffType.SShort)]
        [InlineDataAttribute(TiffType.SLong)]
        [InlineDataAttribute(TiffType.SRational)]
        [InlineDataAttribute(TiffType.Float)]
        [InlineDataAttribute(TiffType.Double)]
        [InlineDataAttribute((TiffType)99)]
        public void ReadIfdReferenceArrayAsync_ThrowsExceptionIfInvalidType(TiffType type)
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian).ToStream();
            var entry = new TiffIfdEntry { Type = type, Count = 10 };

            var e = Assert.Throws<ImageFormatException>(() => { entry.ReadIfdReferenceArrayAsync(stream, ByteOrder.LittleEndian); });

            Assert.Equal($"A value of type '{type}' cannot be converted to an IFD reference.", e.Message);
        }
    }
}