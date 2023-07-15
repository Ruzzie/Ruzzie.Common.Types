using System.Diagnostics;

namespace Ruzzie.Common.Types;

public interface IHasExceptionSource<TException> : IHasBaseException where TException : Exception
{
    Option<TException> ExceptionSource { get; }

    Option<Exception> IHasBaseException.BaseException
    {
        get
        {
            unsafe
            {
                return ExceptionSource.Map(&MapToBaseException);
            }
        }
    }

    internal static Exception MapToBaseException(TException arg)
    {
        return arg.GetBaseException();
    }
}

public interface IHasBaseException
{
    Option<Exception> BaseException { get; }
}

public interface IError
{
    string         Message { get; }
    Option<IError> Source  { get; }
}

public interface IError<out TKind> : IError where TKind : struct, Enum
{
    TKind ErrorKind { get; }
}

public readonly struct Error : IError, IHasExceptionSource<Exception>, IEquatable<Error>
{
    public Error(string message) : this(message, Option<IError>.None)
    {
    }

    public Error(string message, Option<IError> source)
    {
        Message         = message;
        Source          = source;
        ExceptionSource = Option<Exception>.None;
    }

    public Error(string message, Option<IError> source, Option<Exception> exceptionSource)
    {
        Message         = message;
        Source          = source;
        ExceptionSource = exceptionSource;
    }

    public Error(string message, Option<Exception> exceptionSource)
    {
        Message         = message;
        Source          = Option.None<IError>();
        ExceptionSource = exceptionSource;
    }

    public string Message { get; }

    public Option<IError>    Source          { get; }
    public Option<Exception> ExceptionSource { get; }

    public bool Equals(Error other)
    {
        return Message == other.Message && Source.Equals(other.Source) && ExceptionSource.Equals(other.ExceptionSource);
    }

    public override bool Equals(object? obj)
    {
        return obj is Error other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Message.GetHashCode();
            hashCode = (hashCode * 397) ^ Source.GetHashCode();
            hashCode = (hashCode * 397) ^ ExceptionSource.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(Error left, Error right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Error left, Error right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return Message;
    }
}

[DebuggerDisplay("{" + nameof(ErrorKind) + "}:{" + nameof(Message) + "}")]
public class Error<TKind> : IError<TKind> where TKind : struct, Enum
{
    public Error(TKind errorKind) : this(Enum.Format(typeof(TKind), errorKind, "d"), errorKind)
    {
    }

    public Error(TKind errorKind, Option<IError> source) : this(Enum.Format(typeof(TKind), errorKind, "d")
                                                              , errorKind
                                                              , source)
    {
    }

    public Error(string message, TKind errorKind) : this(message, errorKind, Option<IError>.None)
    {
    }

    public Error(string message, TKind errorKind, Option<IError> source)
    {
        Message   = message;
        ErrorKind = errorKind;
        Source    = source;
    }

    public string Message { get; }

    public TKind          ErrorKind { get; }
    public Option<IError> Source    { get; }

    public override string ToString()
    {
        return Message;
    }
}

[DebuggerDisplay("{" + nameof(ErrorKind) + "}:{" + nameof(Message) + "}")]
public class Error<TKind, TException> : IError<TKind>, IHasExceptionSource<TException>
    where TException : Exception where TKind : struct, Enum
{
    public Error(TKind errorKind, Option<IError> source) : this(Enum.Format(typeof(TKind), errorKind, "d")
                                                              , errorKind
                                                              , source)
    {
    }

    public Error(TKind errorKind, Option<TException> exceptionSource) : this(Enum.Format(typeof(TKind), errorKind, "d")
                                                                           , errorKind
                                                                           , exceptionSource)
    {
    }

    public Error(TKind errorKind) : this(Enum.Format(typeof(TKind), errorKind, "d"), errorKind)
    {
    }

    public Error(string message, TKind errorKind, Option<TException> exceptionSource) : this(message
                                                                                           , errorKind
                                                                                           , Option<IError>.None
                                                                                           , exceptionSource)
    {
    }

    public Error(string message, TKind errorKind, Option<IError> source) : this(message
                                                                              , errorKind
                                                                              , source
                                                                              , Option<TException>.None)
    {
    }

    public Error(string message, TKind errorKind) : this(message
                                                       , errorKind
                                                       , Option<IError>.None
                                                       , Option<TException>.None)
    {
    }

    public Error(string message, TKind errorKind, Option<IError> source, Option<TException> exceptionSource)
    {
        Message         = message;
        ErrorKind       = errorKind;
        Source          = source;
        ExceptionSource = exceptionSource;
    }

    public string             Message         { get; }
    public TKind              ErrorKind       { get; }
    public Option<IError>     Source          { get; }
    public Option<TException> ExceptionSource { get; }

    public override string ToString()
    {
        return Message;
    }
}

[DebuggerDisplay("{" + nameof(ErrorKind) + "}:{" + nameof(Message) + "}")]
public readonly struct Err<TKind> : IError<TKind>, IEquatable<Err<TKind>> where TKind : struct, Enum
{
    public string         Message   { get; }
    public Option<IError> Source    { get; }
    public TKind          ErrorKind { get; }

    public Err(in TKind errorKind) : this(Enum.Format(typeof(TKind), errorKind, "d"), errorKind)
    {
    }

    public Err(in TKind errorKind, in Option<IError> source) : this(Enum.Format(typeof(TKind), errorKind, "d")
                                                                  , errorKind
                                                                  , source)
    {
    }

    public Err(in string message, in TKind errorKind) : this(message, errorKind, Option<IError>.None)
    {
    }

    public Err(in string message, in TKind errorKind, in Option<IError> source)
    {
        Message   = message;
        ErrorKind = errorKind;
        Source    = source;
    }

    public override string ToString()
    {
        return Message;
    }

    public bool Equals(Err<TKind> other)
    {
        return Message == other.Message && Source.Equals(other.Source) &&
               EqualityComparer<TKind>.Default.Equals(ErrorKind, other.ErrorKind);
    }

    public override bool Equals(object? obj)
    {
        return obj is Err<TKind> other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Message.GetHashCode();
            hashCode = (hashCode * 397) ^ Source.GetHashCode();
            hashCode = (hashCode * 397) ^ EqualityComparer<TKind>.Default.GetHashCode(ErrorKind);
            return hashCode;
        }
    }

    public static bool operator ==(Err<TKind> left, Err<TKind> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Err<TKind> left, Err<TKind> right)
    {
        return !left.Equals(right);
    }
}

/// A convenient Error wrapper that contains an 'error code' an Enum and can contain Exception information
[DebuggerDisplay("{" + nameof(ErrorKind) + "}:{" + nameof(Message) + "}")]
public readonly struct Err<TKind, TException> : IError<TKind>, IHasExceptionSource<TException>
                                              , IEquatable<Err<TKind, TException>>
    where TException : Exception where TKind : struct, Enum
{
    public string             Message         { get; }
    public Option<IError>     Source          { get; }
    public TKind              ErrorKind       { get; }
    public Option<TException> ExceptionSource { get; }

    public Err(in TKind errorKind, in Option<IError> source) : this(Enum.Format(typeof(TKind), errorKind, "d")
                                                                  , errorKind
                                                                  , source)
    {
    }

    public Err(in TKind errorKind, in Option<TException> exceptionSource) : this(
                                                                                 Enum.Format(typeof(TKind)
                                                                                           , errorKind
                                                                                           , "d")
                                                                               , errorKind
                                                                               , exceptionSource)
    {
    }

    public Err(in TKind errorKind) : this(Enum.Format(typeof(TKind), errorKind, "d"), errorKind)
    {
    }

    public Err(in string message, in TKind errorKind, in Option<TException> exceptionSource) : this(message
                                                                                                  , errorKind
                                                                                                  , Option<IError>.None
                                                                                                  , exceptionSource)
    {
    }

    public Err(in string message, in TKind errorKind, in Option<IError> source) : this(message
                                                                                     , errorKind
                                                                                     , source
                                                                                     , Option<TException>.None)
    {
    }

    public Err(in string message, in TKind errorKind) : this(message
                                                           , errorKind
                                                           , Option<IError>.None
                                                           , Option<TException>.None)
    {
    }

    public Err(in string message, in TKind errorKind, in Option<IError> source, in Option<TException> exceptionSource)
    {
        Message         = message;
        ErrorKind       = errorKind;
        Source          = source;
        ExceptionSource = exceptionSource;
    }

    public override string ToString()
    {
        return Message;
    }

    public bool Equals(Err<TKind, TException> other)
    {
        return Message == other.Message                                           && Source.Equals(other.Source) &&
               EqualityComparer<TKind>.Default.Equals(ErrorKind, other.ErrorKind) &&
               ExceptionSource.Equals(other.ExceptionSource);
    }

    public override bool Equals(object? obj)
    {
        return obj is Err<TKind, TException> other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Message.GetHashCode();
            hashCode = (hashCode * 397) ^ Source.GetHashCode();
            hashCode = (hashCode * 397) ^ EqualityComparer<TKind>.Default.GetHashCode(ErrorKind);
            hashCode = (hashCode * 397) ^ ExceptionSource.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(Err<TKind, TException> left, Err<TKind, TException> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Err<TKind, TException> left, Err<TKind, TException> right)
    {
        return !left.Equals(right);
    }
}