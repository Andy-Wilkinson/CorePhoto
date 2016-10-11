using System;
using System.IO;

namespace CorePhoto.Tests.Helpers
{
    public class StreamBuilder
    {
        public StreamBuilder(StreamBuilderByteOrder byteOrder)
        {
            Stream = new MemoryStream();
            ByteOrder = byteOrder;
        }

        public Stream Stream
        {
            get;
        }
        public StreamBuilderByteOrder ByteOrder
        {
            get;
        }

        public Stream ToStream()
        {
            Stream.Flush();
            Stream.Seek(0, SeekOrigin.Begin);
            return Stream;
        }

        public StreamBuilder WriteByte(byte value)
        {
            Stream.WriteByte(value);
            return this;
        }

        public StreamBuilder WriteBytes(params byte[] value)
        {
            Stream.Write(value, 0, value.Length);
            return this;
        }

        public StreamBuilder WriteInt16(Int16 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytesEndianOrder(bytes);
        }

        public StreamBuilder WriteInt32(Int32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytesEndianOrder(bytes);
        }

        public StreamBuilder WriteBytesEndianOrder(params byte[] value)
        {
            if ((BitConverter.IsLittleEndian && ByteOrder == StreamBuilderByteOrder.BigEndian)
            || (!BitConverter.IsLittleEndian && ByteOrder == StreamBuilderByteOrder.LittleEndian))
                Array.Reverse(value);

            Stream.Write(value, 0, value.Length);
            return this;
        }
    }
}