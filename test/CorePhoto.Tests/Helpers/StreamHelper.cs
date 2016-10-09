using System;
using System.IO;

namespace CorePhoto.Tests.Helpers
{
    public static class StreamHelper
    {
        public static TestStreamWriter CreateStream()
        {
            return CreateStreamLittleEndian();
        }

        public static TestStreamWriter CreateStreamLittleEndian()
        {
            return CreateStream(true);
        }

        public static TestStreamWriter CreateStreamBigEndian()
        {
            return CreateStream(false);
        }

        public static TestStreamWriter CreateStream(bool isLittleEndian)
        {
            var stream = new MemoryStream();
            return new TestStreamWriter(stream, isLittleEndian);
        }

        public static BinaryReader ToReader(this TestStreamWriter streamWriter)
        {
            var stream = streamWriter.Stream;
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return new BinaryReader(stream);
        }

        public static TestStreamWriter WithByte(this TestStreamWriter streamWriter, byte value)
        {
            streamWriter.Stream.WriteByte(value);
            return streamWriter;
        }

        public static TestStreamWriter WithInt16(this TestStreamWriter streamWriter, Int16 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return streamWriter.WithBytesEndianOrder(bytes);
        }

        public static TestStreamWriter WithInt32(this TestStreamWriter streamWriter, Int32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return streamWriter.WithBytesEndianOrder(bytes);
        }

        public static TestStreamWriter WithBytes(this TestStreamWriter streamWriter, params byte[] value)
        {
            streamWriter.Stream.Write(value, 0, value.Length);
            return streamWriter;
        }

        public static TestStreamWriter WithBytesEndianOrder(this TestStreamWriter streamWriter, params byte[] value)
        {
            if (BitConverter.IsLittleEndian != streamWriter.IsLittleEndian)
                Array.Reverse(value);
            
            streamWriter.Stream.Write(value, 0, value.Length);
            return streamWriter;
        }
        
        public class TestStreamWriter
        {
            public TestStreamWriter(Stream stream, bool isLittleEndian)
            {
                this.Stream = stream;
                this.IsLittleEndian = isLittleEndian;
            }

            public Stream Stream
            {
                get;
            }
            public bool IsLittleEndian
            {
                get;
            }
        }
    }
}