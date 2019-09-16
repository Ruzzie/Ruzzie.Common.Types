using System;
using FluentAssertions;
using NUnit.Framework;

using CustomErrorTypeAlias = Ruzzie.Common.Types.Error<Ruzzie.Common.Types.UnitTests.ErrorTypeTests.SampleErrorKind, System.Exception>;

namespace Ruzzie.Common.Types.UnitTests
{
    [TestFixture]
    public class ErrorTypeTests
    {
        [Test]
        public void SmokeTest()
        {
            //generic error
            var error = new Error("Generic error",Option<IError>.None);

            error.Message.Should().Be("Generic error");
        }

        [Test]
        public void MessageShouldBeSet()
        {
            var error = new Error<SampleErrorKind>("Item 1 does not exist", SampleErrorKind.ItemDoesNotExist, Option<IError>.None);
            
            error.Message.Should().Be("Item 1 does not exist");
        }

        [Test]
        public void ErrorWithExceptionSourceExample()
        {
            var result = CreateErrorResult();
            
           result.ExpectError("should be error");
        }

        static Result<Error<SampleErrorKind, Exception>, string> CreateErrorResult()
        {
            return new Error<SampleErrorKind, Exception>("Error!", SampleErrorKind.Unknown, new Exception("exception"));
        }

        [Test]
        public void CustomErrorExample()
        {
            CreateCustomErrorResult().ExpectError("should be error");
        }

        static Result<CustomError, int> CreateCustomErrorResult()
        {
            return new CustomError("CustomError!", SampleErrorKind.Unknown);
        }

        class CustomError : Error<SampleErrorKind>
        {
            public CustomError(string message, SampleErrorKind errorKind) : base(message, errorKind)
            {
            }

            // ReSharper disable once UnusedMember.Local
            public CustomError(string message, SampleErrorKind errorKind, Option<IError> source) : base(message, errorKind, source)
            {
            }
        }

        [Test]
        public void CustomErrorTypeAliasExample()
        {
            var error = new CustomErrorTypeAlias(SampleErrorKind.Unknown);

            error.ErrorKind.Should().Be(SampleErrorKind.Unknown);
        }

        public enum SampleErrorKind
        {
            Unknown,
            ItemDoesNotExist,
            InvalidOptions
        }
    }
}
