using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace Ruzzie.Common.Types.Diagnostics;

public static class ResultPanicExtensions
{
    internal static PanicException<TError> CreatePanicExceptionForErr<TError>(
        FormattableString message
      , in TError         error)
    {
        var errMsg = FormattableString.Invariant($"{FormattableString.Invariant(message)}: {error?.ToString()}");

        if (error is IHasBaseException hasBase)
        {
            if (hasBase.BaseException.TryGetValue(out var innerEx))
            {
                var panicExceptionForErr = new PanicException<TError>(error, errMsg, innerEx!);

                if (innerEx?.StackTrace != null)
                {
                    ExceptionDispatchInfo.SetRemoteStackTrace(panicExceptionForErr, innerEx.StackTrace);
                }

                return panicExceptionForErr;
            }
        }


        return new PanicException<TError>(error, errMsg);
    }


    /// <summary>
    /// Unwraps a result, yielding the content of an Ok. When Error throws an exception.
    /// </summary>
    /// <exception cref="PanicException{TError}">Panics if the value is an Err, with a panic message provided by the Errs value.</exception>
    /// <remarks>Beware this can throw an Exception. Best used in test scenario's</remarks>
    public static T Unwrap<TError, T>(this Result<TError, T> self)
    {
        unsafe
        {
            return self.Match(&Unwrap_ThrowPanicExceptionForGivenError<TError, T>, &PassValue);
        }
    }

    private static T Unwrap_ThrowPanicExceptionForGivenError<TError, T>(in TError e)
    {
        // will throw
        ExceptionDispatchInfo.Throw(CreatePanicExceptionForErr($"called `{nameof(Unwrap)}` on an `Error` value", e));

        // wil never return
        return default;
    }

    private static T PassValue<T>(in T value)
    {
        return value;
    }

    /// <summary>
    /// Unwraps a result, yielding the content of an Ok. Throws an exception when the result is an Err.
    /// </summary>
    /// <exception cref="PanicException{TError}">Panics if the value is an Err, with a panic message including the passed message and the content of the Err.</exception>
    /// <remarks>Beware this can throw an Exception. Best used in test scenario's</remarks>
    public static T Expect<TError, T>(this Result<TError, T> self, string message)
    {
        return self.Match(OnErrorExpectFail, PassValue);

        [DoesNotReturn]
        T OnErrorExpectFail(in TError e)
        {
            //this will throw the exception and not return
            ExceptionDispatchInfo.Throw(CreatePanicExceptionForErr($"{message}", e));

            return default;
        }
    }

    /// <summary>
    /// Unwraps a result, yielding the content of an Err. Throws an exception when the result is an Ok.
    /// </summary>
    /// <exception cref="PanicException{TError}">Panics if the value is an Ok, with a custom panic message provided by the Ok's value.</exception>
    /// <remarks>Beware this can throw an Exception. Best used in test scenario's</remarks>
    public static TError UnwrapError<TError, T>(this Result<TError, T> self)
    {
        unsafe
        {
            return self.Match(&PassValue, &OnOkFail);
        }

        static TError OnOkFail(in T ok)
        {
            //this will throw the exception and not return
            ExceptionDispatchInfo.Throw(CreatePanicExceptionForErr($"called `{nameof(UnwrapError)}` on an `Ok` value"
                                                                 , ok));

            return default;
        }
    }

    /// <summary>
    /// Unwraps a result, yielding the content of an Err. Throws an exception when the result is an Ok.
    /// </summary>
    /// <param name="self">the result</param>
    /// <param name="message">A panic message to pass when the result is ok.</param>
    /// <exception cref="PanicException{TError}">Panics if the value is an Ok, with a panic message including the passed message, and the content of the Ok.</exception>
    /// <remarks>Beware this can throw an Exception. Best used in test scenario's</remarks>
    public static TError ExpectError<TError, T>(this Result<TError, T> self, string message)
    {
        return self.Match(PassValue, OnOkFail);

        TError OnOkFail(in T ok)
        {
            ExceptionDispatchInfo.Throw(CreatePanicExceptionForErr($"{message}", ok));
            return default;
        }
    }
}