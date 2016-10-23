using CorePhoto.Numerics;
using Xunit;

namespace CorePhoto.Tests.Numerics
{
    public class SignedRationalTests
    {
        [Theory]
        [InlineDataAttribute(1, 1)]
        [InlineDataAttribute(1, 2)]
        [InlineDataAttribute(2, 3)]
        public void Constructor_SetsProperties(int numerator, int denominator)
        {
            var rational = new SignedRational(numerator, denominator);

            Assert.Equal(numerator, rational.Numerator);
            Assert.Equal(denominator, rational.Denominator);
        }

        [Theory]
        [InlineDataAttribute(1, 1, "1/1")]
        [InlineDataAttribute(1, 2, "1/2")]
        [InlineDataAttribute(2, 3, "2/3")]
        [InlineDataAttribute(-1, 2, "-1/2")]
        public void ToString_ReturnsReadableString(int numerator, int denominator, string str)
        {
            var rational = new SignedRational(numerator, denominator);

            Assert.Equal(str, rational.ToString());
        }
    }
}