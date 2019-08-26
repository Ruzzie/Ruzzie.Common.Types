using FluentAssertions;
using NUnit.Framework;
using static Ruzzie.Common.Types.Result;
// ReSharper disable InconsistentNaming
namespace Ruzzie.Common.Types.UnitTests
{
    public class ResultExtensionsTests
    {
        [Test]
        public void DefaultIsErrUnwrapOrDefault()
        {
            //result.IsOk
            Result<string, int> res = default;
            res.UnwrapOrDefault(1).Should().Be(1);
        }

        [Test]
        public void UnwrapOrDefaultExample()
        {
            var good_year_from_input = "1909";
            var bad_year_from_input = "190blarg";
            var good_year = good_year_from_input.Parse().UnwrapOrDefault();
            var bad_year = bad_year_from_input.Parse().UnwrapOrDefault();

            Assert.AreEqual(1909, good_year);
            Assert.AreEqual(0, bad_year);
        }

        [Test]
        public void UnwrapOrDefaultWithDefaultValueExample()
        {
            var @default = 0;
            var good_year_from_input = "1909";
            var bad_year_from_input = "190blarg";
            var good_year = good_year_from_input.Parse().UnwrapOrDefault(@default);
            var bad_year = bad_year_from_input.Parse().UnwrapOrDefault(@default);

            Assert.AreEqual(1909, good_year);
            Assert.AreEqual(0, bad_year);
        }

        [Test]
        public void MapOrElseExample()
        {
            var k = 21;

            var x = Ok<int, string>("foo");
            Assert.AreEqual(3, x.MapOrElse(e => k * 2, v => v.Length));


            var y =  Err<string, string>("bar");
            Assert.AreEqual(42, y.MapOrElse(e => k * 2, v=> v.Length));
        }
    }
}