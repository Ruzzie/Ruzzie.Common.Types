using System;

namespace Ruzzie.Common.Types.UnitTests
{
    public readonly struct ParseIntError
    {
        public readonly ParseIntErrorKind Kind;
        public readonly Option<Exception> Exception;

        public ParseIntError(ParseIntErrorKind kind, Option<Exception> exception)
        {
            Kind = kind;
            Exception = exception;
        }
    }
}