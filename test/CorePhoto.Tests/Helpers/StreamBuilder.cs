using System;
using System.IO;
using CorePhoto.IO;

namespace CorePhoto.Tests.Helpers
{
    public class StreamBuilder
    {
        public StreamBuilder(ByteOrder byteOrder)
        {
            Stream = new MemoryStream();
            ByteOrder = byteOrder;
        }

        public Stream Stream
        {
            get;
        }
        public ByteOrder ByteOrder
        {
            get;
        }

        public Stream ToStream()
        {
            Stream.Flush();
            Stream.Seek(0, SeekOrigin.Begin);
            return Stream;
        }

        public byte[] ToBytes()
        {
            Stream.Flush();
            Stream.Seek(0, SeekOrigin.Begin);

            int length = (int)Stream.Length;
            byte[] buffer = new byte[length];
            Stream.Read(buffer, 0, length);
            return buffer;
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

        public StreamBuilder WriteUInt16(UInt16 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytesEndianOrder(bytes);
        }

        public StreamBuilder WriteUInt32(UInt32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteBytesEndianOrder(bytes);
        }

        public StreamBuilder WriteBytesEndianOrder(params byte[] value)
        {
            byte[] data = value.WithByteOrder(ByteOrder);
            Stream.Write(data, 0, data.Length);
            return this;
        }

        public StreamBuilder WritePadding(int count)
        {
            byte[] bytes = new byte[count];
            Stream.Write(bytes, 0, count);
            return this;
        }
    }
}