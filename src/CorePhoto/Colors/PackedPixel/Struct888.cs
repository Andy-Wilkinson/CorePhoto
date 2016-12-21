using System;
using System.Runtime.InteropServices;

namespace CorePhoto.Colors.PackedPixel
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Struct888 : IEquatable<Struct888>
    {
        public byte X;
        public byte Y;
        public byte Z;

        public Struct888(byte x, byte y, byte z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override bool Equals(object obj)
        {
            return obj is Struct888 && this.Equals((Struct888)obj);
        }

        public bool Equals(Struct888 other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }

        public override int GetHashCode()
        {
            return this.X ^ this.Y ^ this.Z;
        }

        public static bool operator ==(Struct888 left, Struct888 right)
        {
            return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
        }
        public static bool operator !=(Struct888 left, Struct888 right)
        {
            return left.X != right.X || left.Y != right.Y || left.Z != right.Z;
        }
    }
}