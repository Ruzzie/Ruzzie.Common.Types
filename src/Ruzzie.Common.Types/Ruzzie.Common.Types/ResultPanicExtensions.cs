using System;
#nullable enable
namespace Ruzzie.Common.Types
{
    public static class ResultPanicExtensions
    {
        internal static PanicException<TError> CreatePanicExceptionForErr<TError>(FormattableString message, in TError error)
        {
            return new PanicException<TError>(error,
                                              // ReSharper disable once ConstantConditionalAccessQualifier
                                              FormattableString.Invariant($"{FormattableString.Invariant(message)}: {error?.ToString()}"));

        }

        /// <summary>
        /// Unwraps a result, yielding the content of an Ok. When Error throws an exception.
        /// </summary>
        /// <exception cref="PanicException{TError}">Panics if the value is an Err, with a panic message provided by the Errs value.</exception>
        public static T Unwrap<TError, T>(this in Result<TError, T> self)
        {
            unsafe
            {
                return self.Match(&OnErrorThrowPanicException<TError,T>, &PassValue);
            }
        }

        private static T OnErrorThrowPanicException<TError, T>(in TError e)
        {
            throw CreatePanicExceptionForErr($"called `{nameof(Unwrap)}` on an `Error` value", e);
        }

        private static T PassValue<T>(in T value)
        {
            return value;
        }

        /// <summary>
        /// Unwraps a result, yielding the content of an Ok. Throws an exception when the result is an Err.
        /// </summary>
        /// <exception cref="PanicException{TError}">Panics if the value is an Err, with a panic message including the passed message, and the content of the Err.</exception>
        public static T Expect<TError,T>(this in Result<TError,T> self, string message)
        {
            return self.Match(OnErrorExpectFail, PassValue);

            T OnErrorExpectFail(in TError e)
            {
                throw CreatePanicExceptionForErr($"{message}", e);
            }
        }

        /// <summary>
        /// Unwraps a result, yielding the content of an Err. Throws an exception when the result is an Ok.
        /// </summary>
        /// <exception cref="PanicException{TError}">Panics if the value is an Ok, with a custom panic message provided by the Ok's value.</exception>
        public static TError UnwrapError<TError, T>(this in Result<TError, T> self)
        {
            unsafe
            {
                return self.Match(&PassValue, &OnOkFail);
            }

            static TError OnOkFail(in T ok)
            {
                throw CreatePanicExceptionForErr($"called `{nameof(UnwrapError)}` on an `Ok` value", ok);
            }
        }

        /// <summary>
        /// Unwraps a result, yielding the content of an Err. Throws an exception when the result is an Ok.
        /// </summary>
        /// <param name="self">the result</param>
        /// <param name="message">A panic message to pass when the result is ok.</param>
        /// <exception cref="PanicException{TError}">Panics if the value is an Ok, with a panic message including the passed message, and the content of the Ok.</exception>
        public static TError ExpectError<TError, T>(this in Result<TError, T> self, string message)
        {
            return self.Match(PassValue, OnOkFail);

            TError OnOkFail(in T ok)
            {
                throw CreatePanicExceptionForErr($"{message}", ok);
            }
        }
    }
}