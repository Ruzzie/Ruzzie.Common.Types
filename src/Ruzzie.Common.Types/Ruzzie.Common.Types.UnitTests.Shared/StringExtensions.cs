using System;
using ParseIntError = Ruzzie.Common.Types.Error<Ruzzie.Common.Types.UnitTests.ParseIntErrorKind, System.Exception>;

namespace Ruzzie.Common.Types.UnitTests
{
    public static class StringExtensions
    {
        public static Result<ParseIntError, int> Parse(this string input)
        {
            try
            {
                var intValue = int.Parse(input);
                return new Result<ParseIntError, int>(intValue);
            }
            catch (Exception e) when (e is ArgumentNullException)
            {
                return Result.Err<ParseIntError, int>(new ParseIntError(ParseIntErrorKind.Null, e));
            }
            catch (Exception e) when (e is FormatException)
            {
                return Result.Err<ParseIntError, int>(new ParseIntError(ParseIntErrorKind.BadFormat, e));
            }
            catch (Exception e) when (e is OverflowException)
            {
                return Result.Err<ParseIntError, int>(new ParseIntError(ParseIntErrorKind.Overflow, e));
            }
            catch (Exception e)
            {
                throw new PanicException<ParseIntError>(new ParseIntError(ParseIntErrorKind.None, e), e.Message, e);
            }
        }
    }
}