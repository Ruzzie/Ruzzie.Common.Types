using NUnit.Framework;
using Ruzzie.Common.Types.Diagnostics;

namespace Ruzzie.Common.Types.UnitTests;

[TestFixture]
public class ResultPanicExtensionsTests
{
    [Test]
    public void UnwrapExample()
    {
        var x = Result.Ok<string, int>(2);
        Assert.AreEqual(2, x.Unwrap());
    }

    [Test]
    public void UnwrapThrowsPanicExceptionExampleOnErr()
    {
        var x = Result.Err<string, int>("emergency failure");
        Assert.That(
                    () => x.Unwrap()
                  , Throws.TypeOf<PanicException<string>>()
                          .With.Message.EqualTo("called `Unwrap` on an `Error` value: emergency failure"));
    }

    [Test]
    public void UnwrapErrorExample()
    {
        var x = Result.Err<string, int>("emergency failure");
        Assert.AreEqual("emergency failure", x.UnwrapError());
    }

    [Test]
    public void UnwrapErrorThrowsPanicExceptionExampleOnOk()
    {
        var x = Result.Ok<string, int>(2);
        Assert.That(
                    () => x.UnwrapError()
                  , Throws.TypeOf<PanicException<int>>()
                          .With.Message.EqualTo("called `UnwrapError` on an `Ok` value: 2"));
    }

    [Test]
    public void ExpectExample()
    {
        var x = Result.Err<string, int>("emergency failure");

        Assert.That(
                    () => x.Expect("Testing expect")
                  , Throws.TypeOf<PanicException<string>>().With.Message.EqualTo("Testing expect: emergency failure")
                   );
    }

    [Test]
    public void ExpectErrorExample()
    {
        var x = Result.Ok<string, int>(2);
        Assert.That(
                    () => x.ExpectError("Testing expect error")
                  , Throws.TypeOf<PanicException<int>>().With.Message.EqualTo("Testing expect error: 2")
                   );
    }
}