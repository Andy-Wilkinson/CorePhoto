using CorePhoto.Numerics;
using Xunit;

namespace CorePhoto.Tests.Numerics
{
    public class RationalTests
    {
        [Theory]
        [InlineDataAttribute(1, 1)]
        [InlineDataAttribute(1, 2)]
        [InlineDataAttribute(2, 3)]
        public void Constructor_SetsProperties(uint numerator, uint denominator)
        {
            var rational = new Rational(numerator, denominator);

            Assert.Equal(numerator, rational.Numerator);
            Assert.Equal(denominator, rational.Denominator);
        }

        [Theory]
        [InlineDataAttribute(1, 1, "1/1")]
        [InlineDataAttribute(1, 2, "1/2")]
        [InlineDataAttribute(2, 3, "2/3")]
        public void ToString_ReturnsReadableString(uint numerator, uint denominator, string str)
        {
            var rational = new Rational(numerator, denominator);

            Assert.Equal(str, rational.ToString());
        }
    }
}