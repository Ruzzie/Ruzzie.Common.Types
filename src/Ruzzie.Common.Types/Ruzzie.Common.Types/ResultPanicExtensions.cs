﻿using System;

namespace Ruzzie.Common.Types
{
    public static class ResultPanicExtensions
    {
        internal static PanicException<TError> CreatePanicExceptionForErr<TError>(string message, TError error)
        {
            return new PanicException<TError>(error,FormattableString.Invariant($"{message}: {error?.IfDebug()}"));
        }

        /// <summary>
        /// Unwraps a result, yielding the content of an Ok.
        /// </summary>
        /// <exception>Panics if the value is an Err, with a panic message provided by the Errs value.</exception>
        public static T Unwrap<TError, T>(this Result<TError, T> self)
        {
            return self.Match(OnErrorUnwrapFail, PassValue);

            static T OnErrorUnwrapFail(TError e)
            {
                throw CreatePanicExceptionForErr("called `Result::unwrap()` on an `Err` value", e);
            }
        }

        private static T PassValue<T>(T value)
        {
            return value;
        }

        /// <summary>
        /// Unwraps a result, yielding the content of an Ok.
        /// </summary>
        /// <exception>Panics if the value is an Err, with a panic message including the passed message, and the content of the Err.</exception>
        public static T Expect<TError,T>(this Result<TError,T> self, string message)
        {
            return self.Match(OnErrorExpectFail, PassValue);

            T OnErrorExpectFail(TError e)
            {
                throw CreatePanicExceptionForErr(message, e);
            }
        }

        /// <summary>
        /// Unwraps a result, yielding the content of an Err.
        /// </summary>
        /// <exception>Panics if the value is an Ok, with a custom panic message provided by the Ok's value.</exception>
        public static TError UnwrapError<TError, T>(this Result<TError, T> self)
        {
            return self.Match(PassValue, OnOkFail);

            static TError OnOkFail(T ok)
            {
                throw CreatePanicExceptionForErr("called `Result::unwrap_err()` on an `Ok` value", ok);
            }
        }

        /// <summary>
        /// Unwraps a result, yielding the content of an Err.
        /// </summary>
        /// <exception>Panics if the value is an Ok, with a panic message including the passed message, and the content of the Ok.</exception>
        public static TError ExpectError<TError, T>(this Result<TError, T> self, string message)
        {
            return self.Match(PassValue, OnOkFail);

            TError OnOkFail(T ok)
            {
                throw CreatePanicExceptionForErr(message, ok);
            }
        }
    }
}