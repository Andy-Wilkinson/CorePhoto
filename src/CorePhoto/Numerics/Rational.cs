namespace CorePhoto.Numerics
{
    public struct Rational
    {
        private readonly uint _numerator;
        private readonly uint _denominator;

        public Rational(uint numerator, uint denominator)
        {
            _numerator = numerator;
            _denominator = denominator;
        }

        public uint Numerator => _numerator;

        public uint Denominator => _denominator;

        public override string ToString() => $"{_numerator}/{_denominator}";
    }
}