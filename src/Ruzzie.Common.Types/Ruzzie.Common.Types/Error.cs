using System;
using System.Diagnostics;

namespace Ruzzie.Common.Types
{
    public interface IError
    {
        string Message { get; }
        Option<IError> Source { get; }
    }

    public interface IError<out TKind> : IError where TKind : Enum
    {
        TKind ErrorKind { get; }
    }

    public readonly struct Error : IError
    {
        public Error(string message) : this(message,Option<IError>.None)
        {

        }

        public Error(string message, Option<IError> source)
        {
            Message = message;
            Source = source;
        }

        public string Message { get; }

        public Option<IError> Source { get; }
    }

    [DebuggerDisplay("{" + nameof(ErrorKind) + "}:{"+nameof(Message)+"}")]
    public class Error<TKind> : IError<TKind> where TKind: Enum
    {
        public Error(TKind errorKind) : this(Enum.Format(typeof(TKind), errorKind, "d"), errorKind)
        {

        }

        public Error(TKind errorKind,  Option<IError> source) : this(Enum.Format(typeof(TKind), errorKind, "d"), errorKind, source)
        {

        }

        public Error(string message, TKind errorKind) : this(message, errorKind, Option<IError>.None)
        {

        }

        public Error(string message, TKind errorKind, Option<IError> source)
        {
            Message = message;
            ErrorKind = errorKind;
            Source = source;
        }

        public string Message { get; }

        public TKind ErrorKind { get; }
        public Option<IError> Source { get; }
    }

    public class Error<TKind, TException> : IError<TKind> where TException : Exception where TKind: Enum
    {
        public Error(TKind errorKind,  Option<IError> source) : this(Enum.Format(typeof(TKind), errorKind, "d"), errorKind, source)
        {

        }

        public Error(TKind errorKind, Option<TException> exceptionSource) : this(Enum.Format(typeof(TKind), errorKind, "d"), errorKind, exceptionSource)
        {

        }

        public Error(TKind errorKind) : this(Enum.Format(typeof(TKind), errorKind, "d"), errorKind)
        {

        }

        public Error(string message, TKind errorKind, Option<TException> exceptionSource) : this(message, errorKind, Option<IError>.None, exceptionSource)
        {

        }

        public Error(string message, TKind errorKind, Option<IError> source) : this(message,errorKind, source,Option<TException>.None)
        {

        }

        public Error(string message, TKind errorKind) : this(message, errorKind, Option<IError>.None,
            Option<TException>.None)
        {

        }

        public Error(string message, TKind errorKind, Option<IError> source, Option<TException> exceptionSource)
        {
            Message = message;
            ErrorKind = errorKind;
            Source = source;
            ExceptionSource = exceptionSource;
        }

        public string Message { get; }
        public TKind ErrorKind { get; }
        public Option<IError> Source { get; }
        public Option<TException> ExceptionSource { get; }
    }
}
