namespace CorePhoto.Numerics
{
    public struct SignedRational
    {
        private readonly int _numerator;
        private readonly int _denominator;

        public SignedRational(int numerator, int denominator)
        {
            _numerator = numerator;
            _denominator = denominator;
        }

        public int Numerator => _numerator;

        public int Denominator => _denominator;

        public override string ToString() => $"{_numerator}/{_denominator}";
    }
}