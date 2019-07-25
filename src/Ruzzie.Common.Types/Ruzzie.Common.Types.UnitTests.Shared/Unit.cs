using FluentAssertions;
using NUnit.Framework;

namespace Ruzzie.Common.Types.UnitTests
{
    [TestFixture]
    public class UnitTests
    {
        [Test]
        public void Equality()
        {
            new Unit().Should().Be(new Unit()).And.Be(Unit.Void);
        }

        [Test]
        public void EqualityOperator()
        {
            (new Unit() == Unit.Void).Should().BeTrue();
        }

        [Test]
        public void InEqualityOperator()
        {
            (new Unit() != Unit.Void).Should().BeFalse();
        }

        [Test]
        public void CompareIsZero()
        {

        }

        [Test]
        public void GetHashCodeIsZero()
        {
            Unit.Void.GetHashCode().Should().Be(0);
        }
    }
}
