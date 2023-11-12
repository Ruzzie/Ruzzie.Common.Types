using System.Diagnostics;

namespace Ruzzie.Common.Types.Diagnostics;

/// only use in debugging / testing scenario's
public static class ResPanicExtensions
{
    [StackTraceHidden]
    public static T Unwrap<T, TErrKind>(this Res<TErrKind, T> res) where TErrKind : struct, Enum
    {
        if (res.IsOk(out var ok, out var error, default!))
        {
            return ok;
        }

        throw new Exception($"Expected Ok, called `Unwrap` on an Result with an Err: [{error.ToString()}]");
    }

    [StackTraceHidden]
    public static RefErr<TErrKind> UnwrapError<T, TErrKind>(this Res<TErrKind, T> res) where TErrKind : struct, Enum
    {
        if (res.IsErr(out var ok, out var error, default!))
        {
            return error;
        }

        throw new Exception($"Expected Err, called `Unwrap` on an Result with an Ok: [{ok}]");
    }
}