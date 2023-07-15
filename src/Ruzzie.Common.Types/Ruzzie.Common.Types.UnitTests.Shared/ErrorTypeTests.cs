using System;
using FluentAssertions;
using NUnit.Framework;
using Ruzzie.Common.Types.Diagnostics;
using CustomErrorTypeAlias =
    Ruzzie.Common.Types.Error<Ruzzie.Common.Types.UnitTests.ErrorTypeTests.SampleErrorKind, System.Exception>;

namespace Ruzzie.Common.Types.UnitTests;

[TestFixture]
public class ErrorTypeTests
{
    [Test]
    public void BasicErrorValueTypeWithOnlyMessageAndExceptionSourceCtor()
    {
        var   exceptionSource = new Exception("exception message");
        Error err             = new Error("something went wrong.", exceptionSource);

        err.ExceptionSource.UnwrapOr(new Exception()).Should().Be(exceptionSource);
    }

#if CS8
        [Test]
        public void UsageExampleWithPlainErrorType()
        {
            var inputString = "\\asd23+++---";

            var error = CreateStringFromBase64BasedKey(inputString).ExpectError("Should be Err");
            
            error.ExceptionSource.UnwrapOr(new Exception())
                .Should().BeOfType<FormatException>()
                .Which.Message.Should().NotBeNullOrWhiteSpace();

            static Result<Error, string> CreateStringFromBase64BasedKey(string base64Input)
            {
                if (string.IsNullOrWhiteSpace(base64Input))
                {
                    return base64Input;
                }
                try
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(base64Input));
                }
                catch (FormatException e)
                {
                    return new Error(e.Message, e);
                }
            }
        }
#endif

    [Test]
    public void SmokeTest()
    {
        //generic error
        var error = new Error("Generic error", Option<IError>.None);

        error.Message.Should().Be("Generic error");
    }

    [Test]
    public void ErrorBasic_ToStringReturnsMessage()
    {
        var error = new Error("Generic error", Option<IError>.None);

        error.Message.Should().Be("Generic error").And.Be(error.ToString());
    }

    [Test]
    public void ErrorWithKindType_ToStringReturnsMessage()
    {
        var error = new Error<SampleErrorKind>("Item 1 does not exist"
                                             , SampleErrorKind.ItemDoesNotExist
                                             , Option<IError>.None);

        error.Message.Should().Be("Item 1 does not exist").And.Be(error.ToString());
    }


    [Test]
    public void ErrorWithKindAndExceptionType_ToStringReturnsMessage()
    {
        var error = new Error<SampleErrorKind, Exception>("Item 1 does not exist"
                                                        , SampleErrorKind.ItemDoesNotExist
                                                        , Option<IError>.None);

        error.Message.Should().Be("Item 1 does not exist").And.Be(error.ToString());
    }

    [Test]
    public void MessageShouldBeSet()
    {
        var error = new Error<SampleErrorKind>("Item 1 does not exist"
                                             , SampleErrorKind.ItemDoesNotExist
                                             , Option<IError>.None);

        error.Message.Should().Be("Item 1 does not exist");
    }

    [Test]
    public void ErrorWithExceptionSourceExample()
    {
        var result = CreateErrorResult();

        result.ExpectError("should be error");
    }

    [Test]
    public void ErrWithKindSample()
    {
        var result = GiveError();

        var msg = result.Match(
                               (in Err<SampleErrorKind> err) => $"{err.ErrorKind}"
                             , (in string               ok) => ok);

        msg.Should().Be("Unknown");

        Result<Err<SampleErrorKind>, string> GiveError()
        {
            return new Err<SampleErrorKind>(SampleErrorKind.Unknown);
        }
    }

    [Test]
    public void ErrWithKindAndExceptionSample()
    {
        var result = GiveError();

        var msg = result.Match(
                               (in Err<SampleErrorKind, Exception> err) =>
                                   $"{err.ErrorKind} - {err.ExceptionSource.Match(() => string.Empty, exception => exception.Message)}"
                             , (in string ok) => ok);

        msg.Should().Be("Unknown - Test");

        Result<Err<SampleErrorKind, Exception>, string> GiveError()
        {
            return new Err<SampleErrorKind, Exception>(SampleErrorKind.Unknown, new Exception("Test"));
        }
    }

    private static Result<Error<SampleErrorKind, Exception>, string> CreateErrorResult()
    {
        return new Error<SampleErrorKind, Exception>("Error!", SampleErrorKind.Unknown, new Exception("exception"));
    }

    [Test]
    public void CustomErrorExample()
    {
        CreateCustomErrorResult().ExpectError("I should be an error");
    }

    private static Result<CustomError, int> CreateCustomErrorResult()
    {
        return new CustomError("CustomError!", SampleErrorKind.Unknown);
    }

    private class CustomError : Error<SampleErrorKind>
    {
        public CustomError(string message, SampleErrorKind errorKind) : base(message, errorKind)
        {
        }

        // ReSharper disable once UnusedMember.Local
        public CustomError(string message, SampleErrorKind errorKind, Option<IError> source) :
            base(message, errorKind, source)
        {
        }
    }

    public enum SampleErrorKind
    {
        Unknown
      , ItemDoesNotExist
      , InvalidOptions
    }

    [Test]
    public void CustomErrorTypeAliasExample()
    {
        var error = new CustomErrorTypeAlias(SampleErrorKind.Unknown);

        error.ErrorKind.Should().Be(SampleErrorKind.Unknown);
    }
}