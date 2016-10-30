using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorePhoto.IO;
using CorePhoto.Numerics;

namespace CorePhoto.Tiff
{
    public static class TiffIfdEntryReader
    {
        public static uint GetInteger(this TiffIfdEntry entry, ByteOrder byteOrder)
        {
            if (entry.Count != 1)
                throw new ImageFormatException("Cannot read a single value from an array of multiple items.");

            switch (entry.Type)
            {
                case TiffType.Byte:
                    return DataConverter.ToByte(entry.Value, 0);
                case TiffType.Short:
                    return DataConverter.ToUInt16(entry.Value, 0, byteOrder);
                case TiffType.Long:
                    return DataConverter.ToUInt32(entry.Value, 0, byteOrder);
                default:
                    throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to an unsigned integer.");
            }
        }

        public static int GetSignedInteger(this TiffIfdEntry entry, ByteOrder byteOrder)
        {
            if (entry.Count != 1)
                throw new ImageFormatException("Cannot read a single value from an array of multiple items.");

            switch (entry.Type)
            {
                case TiffType.SByte:
                    return DataConverter.ToSByte(entry.Value, 0);
                case TiffType.SShort:
                    return DataConverter.ToInt16(entry.Value, 0, byteOrder);
                case TiffType.SLong:
                    return DataConverter.ToInt32(entry.Value, 0, byteOrder);
                default:
                    throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a signed integer.");
            }
        }

        public static TiffIfdReference GetIfdReference(this TiffIfdEntry entry, ByteOrder byteOrder)
        {
            if (entry.Count != 1)
                throw new ImageFormatException("Cannot read a single value from an array of multiple items.");

            if (entry.Type != TiffType.Long && entry.Type != TiffType.Ifd)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to an IFD reference.");

            return new TiffIfdReference(DataConverter.ToUInt32(entry.Value, 0, byteOrder));
        }

        public static Task<uint[]> ReadIntegerArrayAsync(this TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            var type = entry.Type;

            if (type != TiffType.Byte && type != TiffType.Short && type != TiffType.Long)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to an unsigned integer.");

            return ReadIntegerArrayAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<uint[]> ReadIntegerArrayAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            switch (entry.Type)
            {
                case TiffType.Byte:
                    {
                        byte[] data = await entry.ReadDataAsync(stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => (uint)DataConverter.ToByte(data, index)).ToArray();
                    }
                case TiffType.Short:
                    {
                        byte[] data = await entry.ReadDataAsync(stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => (uint)DataConverter.ToUInt16(data, index * 2, byteOrder)).ToArray();
                    }
                case TiffType.Long:
                case TiffType.Ifd:
                    {
                        byte[] data = await entry.ReadDataAsync(stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => DataConverter.ToUInt32(data, index * 4, byteOrder)).ToArray();
                    }
                default:
                    throw new InvalidOperationException();
            }
        }

        public static Task<int[]> ReadSignedIntegerArrayAsync(this TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            var type = entry.Type;

            if (type != TiffType.SByte && type != TiffType.SShort && type != TiffType.SLong)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a signed integer.");

            return ReadSignedIntegerArrayAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<int[]> ReadSignedIntegerArrayAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            switch (entry.Type)
            {
                case TiffType.SByte:
                    {
                        byte[] data = await entry.ReadDataAsync(stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => (int)DataConverter.ToSByte(data, index)).ToArray();
                    }
                case TiffType.SShort:
                    {
                        byte[] data = await entry.ReadDataAsync(stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => (int)DataConverter.ToInt16(data, index * 2, byteOrder)).ToArray();
                    }
                case TiffType.SLong:
                    {
                        byte[] data = await entry.ReadDataAsync(stream, byteOrder);
                        return Enumerable.Range(0, entry.Count).Select(index => DataConverter.ToInt32(data, index * 4, byteOrder)).ToArray();
                    }
                default:
                    throw new InvalidOperationException();
            }
        }

        public static Task<string> ReadStringAsync(this TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.Ascii)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a string.");

            return ReadStringAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<string> ReadStringAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            byte[] data = await entry.ReadDataAsync(stream, byteOrder);

            if (data[data.Length - 1] != 0)
                throw new ImageFormatException("The retrieved string is not null terminated.");

            return Encoding.ASCII.GetString(data, 0, data.Length - 1);
        }

        public static Task<Rational> ReadRationalAsync(this TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.Rational)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a Rational.");

            if (entry.Count != 1)
                throw new ImageFormatException("Cannot read a single value from an array of multiple items.");

            return ReadRationalAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<Rational> ReadRationalAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            var array = await ReadRationalArrayAsync_Internal(entry, stream, byteOrder);
            return array[0];
        }

        public static Task<SignedRational> ReadSignedRationalAsync(this TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.SRational)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a SignedRational.");

            if (entry.Count != 1)
                throw new ImageFormatException("Cannot read a single value from an array of multiple items.");

            return ReadSignedRationalAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<SignedRational> ReadSignedRationalAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            var array = await ReadSignedRationalArrayAsync_Internal(entry, stream, byteOrder);
            return array[0];
        }

        public static Task<Rational[]> ReadRationalArrayAsync(this TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.Rational)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a Rational.");

            return ReadRationalArrayAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<Rational[]> ReadRationalArrayAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {


            byte[] data = await entry.ReadDataAsync(stream, byteOrder);

            return Enumerable.Range(0, entry.Count).Select(index =>
                       {
                           var numerator = DataConverter.ToUInt32(data, index * 8, byteOrder);
                           var denominator = DataConverter.ToUInt32(data, index * 8 + 4, byteOrder);
                           return new Rational(numerator, denominator);
                       }).ToArray();
        }

        public static Task<SignedRational[]> ReadSignedRationalArrayAsync(this TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.SRational)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a SignedRational.");

            return ReadSignedRationalArrayAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<SignedRational[]> ReadSignedRationalArrayAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            byte[] data = await entry.ReadDataAsync(stream, byteOrder);

            return Enumerable.Range(0, entry.Count).Select(index =>
                       {
                           var numerator = DataConverter.ToInt32(data, index * 8, byteOrder);
                           var denominator = DataConverter.ToInt32(data, index * 8 + 4, byteOrder);
                           return new SignedRational(numerator, denominator);
                       }).ToArray();
        }

        public static Task<float> ReadFloatAsync(this TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.Float)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a float.");

            if (entry.Count != 1)
                throw new ImageFormatException("Cannot read a single value from an array of multiple items.");

            return ReadFloatAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<float> ReadFloatAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            var array = await ReadFloatArrayAsync_Internal(entry, stream, byteOrder);
            return array[0];
        }

        public static Task<float[]> ReadFloatArrayAsync(this TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.Float)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a float.");

            return ReadFloatArrayAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<float[]> ReadFloatArrayAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            byte[] data = await entry.ReadDataAsync(stream, byteOrder);

            return Enumerable.Range(0, entry.Count).Select(index =>
                       {
                           return DataConverter.ToSingle(data, index * 4, byteOrder);
                       }).ToArray();
        }

        public static Task<double> ReadDoubleAsync(this TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.Double)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a double.");

            if (entry.Count != 1)
                throw new ImageFormatException("Cannot read a single value from an array of multiple items.");

            return ReadDoubleAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<double> ReadDoubleAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            var array = await ReadDoubleArrayAsync_Internal(entry, stream, byteOrder);
            return array[0];
        }

        public static Task<double[]> ReadDoubleArrayAsync(this TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            if (entry.Type != TiffType.Double)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to a double.");

            return ReadDoubleArrayAsync_Internal(entry, stream, byteOrder);
        }

        private static async Task<double[]> ReadDoubleArrayAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            byte[] data = await entry.ReadDataAsync(stream, byteOrder);

            return Enumerable.Range(0, entry.Count).Select(index =>
                       {
                           return DataConverter.ToDouble(data, index * 8, byteOrder);
                       }).ToArray();
        }

        public static Task<TiffIfdReference[]> ReadIfdReferenceArrayAsync(this TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            var type = entry.Type;

            if (type != TiffType.Long && type != TiffType.Ifd)
                throw new ImageFormatException($"A value of type '{entry.Type}' cannot be converted to an IFD reference.");

            return ReadIfdReferenceArrayAsync_Internal(entry, stream, byteOrder); ;
        }

        private static async Task<TiffIfdReference[]> ReadIfdReferenceArrayAsync_Internal(TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            byte[] data = await entry.ReadDataAsync(stream, byteOrder);
            return Enumerable.Range(0, entry.Count).Select(index => new TiffIfdReference(DataConverter.ToUInt32(data, index * 4, byteOrder))).ToArray();
        }

        public static Task<byte[]> ReadDataAsync(this TiffIfdEntry entry, Stream stream, ByteOrder byteOrder)
        {
            var sizeOfData = entry.SizeOfData();

            if (sizeOfData <= 4)
            {
                return Task.FromResult(entry.Value);
            }
            else
            {
                var dataOffset = DataConverter.ToUInt32(entry.Value, 0, byteOrder);
                stream.Seek(dataOffset, SeekOrigin.Begin);
                return stream.ReadBytesAsync(sizeOfData);
            }
        }

        public static int SizeOfData(this TiffIfdEntry entry) => TiffReader.SizeOfDataType(entry.Type) * entry.Count;
    }
}