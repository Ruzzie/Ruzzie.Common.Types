﻿using System;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

using CustomErrorTypeAlias = Ruzzie.Common.Types.Error<Ruzzie.Common.Types.UnitTests.ErrorTypeTests.SampleErrorKind, System.Exception>;

namespace Ruzzie.Common.Types.UnitTests
{
    [TestFixture]
    public class ErrorTypeTests
    {

        [Test]
        public void BasicErrorValueTypeWithOnlyMessageAndExceptionSourceCtor()
        {
            var exceptionSource = new Exception("exception message");
            Error err = new Error("something went wrong.", exceptionSource);

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
                .Which.Message.Should()
                .Be(
                    "The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters. ");

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