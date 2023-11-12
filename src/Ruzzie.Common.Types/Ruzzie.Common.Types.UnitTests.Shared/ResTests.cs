using FluentAssertions;
using NUnit.Framework;
using Ruzzie.Common.Types.Diagnostics;

// ReSharper disable InconsistentNaming
namespace Ruzzie.Common.Types.UnitTests;

[TestFixture]
public class ResTests
{
    [Test]
    public void IsOk_True()
    {
        var result = new Res<string, int>(42);

        Assert.AreEqual(result.IsOk(out int okValue, out _, 0, ""), true);
        Assert.AreEqual(okValue,                                    42);
    }

    [Test]
    public void IsError_True()
    {
        var result = new Res<string, int>("error");

        Assert.AreEqual(result.IsErr(out _, out var error, 0, ""), true);
        Assert.AreEqual(error.ErrKind,                             "error");
    }

    [Test]
    public void IsError_False()
    {
        var result = new Res<string, int>(123);

        Assert.AreEqual(result.IsErr(out var okValue, out _, 0, ""), false);
        Assert.AreEqual(okValue,                                     123);
    }

    [Test]
    public void IsOk_False_WhenNotInitialized()
    {
        default(Res<string, int>).IsOk(out _, out _, 0, "").Should().BeFalse();
    }


    public enum ErrCode
    {
        MinorFail
      , Fail
      , EpicFail
    }

    [Test]
    public unsafe void AndJoinOk_Ok()
    {
        //Arrange
        var x = Res.Ok<ErrCode, uint>(2);

        //Act
        var joinedResult = x.AndJoinOk(&DoSomething);

        //Assert
        joinedResult.Unwrap().Should().Be((2, 3));

        static Res<ErrCode, uint> DoSomething()
        {
            return Res.Ok<ErrCode, uint>(3);
        }
    }

    [Test]
    public unsafe void AndJoinOk_FirstIsErr()
    {
        //Arrange
        var x = Res.Err<ErrCode, uint>(ErrCode.Fail);

        //Act
        var joinedResult = x.AndJoinOk(&DoSomething);

        //Assert
        joinedResult.UnwrapError().ErrKind.Should().Be(ErrCode.Fail);

        static Res<ErrCode, uint> DoSomething()
        {
            return Res.Ok<ErrCode, uint>(3);
        }
    }

    [Test]
    public unsafe void AndJoinOk_SecondIsErr()
    {
        //Arrange
        var x = Res.Ok<ErrCode, uint>(2);

        //Act
        var joinedResult = x.AndJoinOk(&DoSomething);

        //Assert
        var error = joinedResult.UnwrapError();

        error.ErrKind.Should().Be(ErrCode.EpicFail);
        error.ErrMsg.ToString().Should().Be("Error message");


        static Res<ErrCode, uint> DoSomething()
        {
            return new Res<ErrCode, uint>(new RefErr<ErrCode>("Error message", ErrCode.EpicFail));
        }
    }


    private static string MapErrToString<TErrKind>(RefErr<TErrKind> err)
    {
        return err.ToString();
    }

    private static string MapOkToString<TOk>(TOk ok)
    {
        return ok?.ToString() ?? "";
    }

    [Test]
    public unsafe void MatchOk()
    {
        var result = new Res<ErrCode, int>(42);
        var actual = result.Match(&MapErrToString, &MapOkToString);
        Assert.AreEqual("42", actual);
    }

    [Test]
    public void MatchErr()
    {
        unsafe
        {
            var result = new Res<ErrCode, int>(new RefErr<ErrCode>(ErrCode.Fail, "foo"));
            var actual = result.Match(&MapErrToString, &MapOkToString);
            Assert.AreEqual("[Fail]: foo", actual);
        }
    }

    [Test]
    public unsafe void DefaultIsErr()
    {
        Res<object, object> res = default;

        //Is is an error, however the Exception type is initialized to default
        // We detect that the Result type is not initialized and throw a panic exception otherwise
        //   when we try to consume that err value it is null, this results in a runtime exception.
        var asString = res.Match(&MapErrToString, &MapOkToString);

        Assert.AreEqual("[]: ", asString);
    }

    [Test]
    public unsafe void MapErrExample()
    {
        static RefErr<ErrCode> stringify(RefErr<uint> i)
        {
            return new RefErr<ErrCode>(ErrCode.MinorFail, $"error code: {i}");
        }

        var x = Res.Ok<uint, uint>(2);


        Res<ErrCode, uint> mappedResult = x.MapErr(&stringify);

        // the mapped result only mapped the types; since the value was OK the ok values should be equal

        //Assert
        Res.Ok<ErrCode, uint>(2).Unwrap().Should().Be(mappedResult.Unwrap());

        x = Res.Err<uint, uint>(13);


        // this result is an Error
        var mappedErrResult = x.MapErr(&stringify);

        //Assert
        var theError = mappedErrResult.UnwrapError();
        theError.ErrKind.Should().Be(ErrCode.MinorFail);
        theError.ErrMsg.ToString().Should().Be("error code: 13");
    }

    [Test]
    public unsafe void AndThenExample()
    {
        static Res<ErrCode, uint> sq(uint x)
        {
            return Res.Ok<ErrCode, uint>(x * x);
        }

        static Res<ErrCode, uint> err(uint x)
        {
            return Res.Err<ErrCode, uint>(ErrCode.EpicFail);
        }

        Assert.AreEqual(Res.Ok<ErrCode, uint>(2).AndThen(&sq).AndThen(&sq).Unwrap(), 16);

        Assert.AreEqual(Res.Ok<ErrCode, uint>(2).AndThen(&sq).AndThen(&err).UnwrapError().ErrKind, ErrCode.EpicFail);
        Assert.AreEqual(Res.Ok<ErrCode, uint>(2).AndThen(&err).AndThen(&sq).UnwrapError().ErrKind, ErrCode.EpicFail);
        Assert.AreEqual(Res.Err<ErrCode, uint>(ErrCode.MinorFail).AndThen(&sq).AndThen(&sq).UnwrapError().ErrKind
                      , ErrCode.MinorFail);
    }

    [Test]
    public void UnwrapOrExample()
    {
        uint defaultWith = 2;
        var  x           = Res.Ok<string, uint>(9);
        Assert.AreEqual(x.UnwrapOr(defaultWith), 9);

        var y = Res.Err<string, uint>("error");
        Assert.AreEqual(y.UnwrapOr(defaultWith), defaultWith);
    }

    [Test]
    public void DefaultStateIsErr()
    {
        Res<string, int> res = default;

        res.IsErr(out _, out _, default).Should().BeTrue();
    }
}