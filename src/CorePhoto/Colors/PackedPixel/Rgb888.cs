using System;
using System.Numerics;
using ImageSharp;

namespace CorePhoto.Colors.PackedPixel
{
    public struct Rgb888 : IPixel<Rgb888>, IPackedVector<Struct888>
    {
        private static readonly Vector3 MaxBytes = new Vector3(255);
        private static readonly Vector4 MaxBytes4 = new Vector4(255);
        private static readonly Vector3 Half = new Vector3(0.5F);
        private static readonly Vector4 Half4 = new Vector4(0.5F);

        public Rgb888(byte r, byte g, byte b)
        {
            this.PackedValue = Pack(r, g, b);
        }

        public Rgb888(float r, float g, float b)
        {
            this.PackedValue = Pack(r, g, b);
        }

        public Rgb888(Vector3 vector)
        {
            this.PackedValue = Pack(ref vector);
        }

        public Rgb888(Struct888 packed = default(Struct888))
        {
            this.PackedValue = packed;
        }

        public Struct888 PackedValue { get; set; }

        public BulkPixelOperations<Rgb888> CreateBulkOperations() => new BulkPixelOperations<Rgb888>();

        public byte R
        {
            get
            {
                return this.PackedValue.X;
            }

            set
            {
                this.PackedValue = new Struct888(value, this.PackedValue.Y, this.PackedValue.Z);
            }
        }

        public byte G
        {
            get
            {
                return this.PackedValue.Y;
            }

            set
            {
                this.PackedValue = new Struct888(this.PackedValue.X, value, this.PackedValue.Z);
            }
        }

        public byte B
        {
            get
            {
                return this.PackedValue.Z;
            }

            set
            {
                this.PackedValue = new Struct888(this.PackedValue.X, this.PackedValue.Y, value);
            }
        }

        public static bool operator ==(Rgb888 left, Rgb888 right)
        {
            return left.PackedValue == right.PackedValue;
        }
        public static bool operator !=(Rgb888 left, Rgb888 right)
        {
            return left.PackedValue != right.PackedValue;
        }

        public void PackFromVector4(Vector4 vector)
        {
            this.PackedValue = Pack(ref vector);
        }
        public Vector4 ToVector4()
        {
            return new Vector4(this.R, this.G, this.B, 255) / MaxBytes4;
        }

        public void PackFromBytes(byte x, byte y, byte z, byte w)
        {
            this.PackedValue = Pack(x, y, z);
        }

        public void ToXyzBytes(byte[] bytes, int startIndex)
        {
            bytes[startIndex] = this.R;
            bytes[startIndex + 1] = this.G;
            bytes[startIndex + 2] = this.B;
        }

        public void ToXyzwBytes(byte[] bytes, int startIndex)
        {
            throw new NotSupportedException();
        }

        public void ToZyxBytes(byte[] bytes, int startIndex)
        {
            bytes[startIndex] = this.B;
            bytes[startIndex + 1] = this.G;
            bytes[startIndex + 2] = this.R;
        }

        public void ToZyxwBytes(byte[] bytes, int startIndex)
        {
            throw new NotSupportedException();
        }

        public override bool Equals(object obj)
        {
            return obj is Rgb888 && this.Equals((Rgb888)obj);
        }

        public bool Equals(Rgb888 other)
        {
            return this.PackedValue == other.PackedValue;
        }

        public override int GetHashCode()
        {
            return this.PackedValue.GetHashCode();
        }

        private static Struct888 Pack(float x, float y, float z)
        {
            var value = new Vector3(x, y, z);
            return Pack(ref value);
        }
        private static Struct888 Pack(byte x, byte y, byte z)
        {
            return new Struct888(x, y, z);
        }

        private static Struct888 Pack(ref Vector3 vector)
        {
            vector = Vector3.Clamp(vector, Vector3.Zero, Vector3.One);
            vector *= MaxBytes;
            vector += Half;
            return new Struct888((byte)vector.X, (byte)vector.Y, (byte)vector.Z);
        }

        private static Struct888 Pack(ref Vector4 vector)
        {
            vector = Vector4.Clamp(vector, Vector4.Zero, Vector4.One);
            vector *= MaxBytes4;
            vector += Half4;
            return new Struct888((byte)vector.X, (byte)vector.Y, (byte)vector.Z);
        }
    }
}