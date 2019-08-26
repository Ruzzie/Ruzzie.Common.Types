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

    [TestFixture]
    public class ResultPanicExtensionsTests
    {
        [Test]
        public void UnwrapExample()
        {
            var x = Ok<string, int>(2);
            Assert.AreEqual(2, x.Unwrap());
        }

        [Test]
        public void UnwrapThrowsPanicExceptionExampleOnErr()
        {
            var x = Err<string, int>("emergency failure");
            Assert.That(
                () => x.Unwrap(),
                Throws.TypeOf<PanicException<string>>().With.Message.EqualTo("called `Unwrap` on an `Error` value: emergency failure"));
        }

        [Test]
        public void UnwrapErrorExample()
        {
            var x = Err<string, int>("emergency failure");
            Assert.AreEqual("emergency failure", x.UnwrapError());
        }

        [Test]
        public void UnwrapErrorThrowsPanicExceptionExampleOnOk()
        {
            var x = Ok<string, int>(2);
            Assert.That(
                () => x.UnwrapError(),
                Throws.TypeOf<PanicException<int>>().With.Message.EqualTo("called `UnwrapError` on an `Ok` value: 2"));
        }

        [Test]
        public void ExpectExample()
        {
            var x = Err<string, int>("emergency failure");

            Assert.That(
                () => x.Expect("Testing expect"),
                Throws.TypeOf<PanicException<string>>().With.Message.EqualTo("Testing expect: emergency failure")
            );
        }

        [Test]
        public void ExpectErrorExample()
        {
            var x = Ok<string, int>(2);
            Assert.That(
                () => x.ExpectError("Testing expect error"),
                Throws.TypeOf<PanicException<int>>().With.Message.EqualTo("Testing expect error: 2")
            );
        }
    }
}