using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ruzzie.Common.Types;

public static class Res
{
    public static Res<TError, T> Ok<TError, T>(T ok)
    {
        return Res<TError, T>.Ok(ok);
    }

    public static Res<TError, T> Err<TError, T>(TError err)
    {
        return Res<TError, T>.Err(err);
    }
}

/// <summary>
/// A ref struct Result type. This represents a union like type that can either be Ok or Error.
/// This type forces the error to be at least an enum type, such that error cases can be handled.
/// </summary>
/// <typeparam name="TErrKind"></typeparam>
/// <typeparam name="TOk"></typeparam>
[SkipLocalsInit]
public readonly ref struct Res<TErrKind, TOk>
{
    private readonly ResultState _resultState = ResultState.NotInitialized; // default NotInitialized

    /// we use a specific RefErr{T} because ref structs cannot be generic parameters, so this is the trade-off
    private readonly RefErr<TErrKind> _err;

    private readonly TOk _ok;

    public Res(in TOk ok)
    {
        _ok          = ok;
        _err         = default;
        _resultState = ResultState.Success;
    }

    public Res(TOk ok)
    {
        _ok          = ok;
        _err         = default;
        _resultState = ResultState.Success;
    }

    public Res(TErrKind err)
    {
        _err         = new RefErr<TErrKind>(err);
        _ok          = default!;
        _resultState = ResultState.Error;
    }

    public Res(RefErr<TErrKind> err)
    {
        _err         = err;
        _ok          = default!;
        _resultState = ResultState.Error;
    }

    public static implicit operator Res<TErrKind, TOk>(in TOk ok)
    {
        return new Res<TErrKind, TOk>(in ok);
    }

    public static implicit operator Res<TErrKind, TOk>(RefErr<TErrKind> err)
    {
        return new Res<TErrKind, TOk>(err);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public bool IsOk(out TOk ok, out RefErr<TErrKind> err, TOk okDefault, RefErr<TErrKind> errDefault = default)
    {
        switch (_resultState)
        {
            default:
            case ResultState.Error:
            case ResultState.NotInitialized:
                // When this occurs the system is in a invalid state,
                //   this occurs when the Result = default, which is useless
                //   in other normal usage scenario's this is virtually impossible
                //   however when one uses reflection or such magic, this can occur
                // I would plead that the program may crash in this state
                // OR we make this state 'impossible' and the default is an error state,
                //  this would make consuming easier, but it does not address the default state and
                // For now: error
                err = _err;
                ok  = okDefault;
                return false;
            case ResultState.Success:
                ok  = _ok;
                err = errDefault;
                return true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public bool IsErr(out TOk ok, out RefErr<TErrKind> err, TOk okDefault, RefErr<TErrKind> errDefault = default)
    {
        return !IsOk(out ok, out err, okDefault, errDefault);
    }

    public unsafe TU Match<TU>(delegate*<RefErr<TErrKind>, TU> onError, delegate*<TOk, TU> onOk)
    {
        switch (_resultState)
        {
            default:
            case ResultState.NotInitialized:
            // When this occurs the system is in a invalid state,
            //   this occurs when the Result = default, which is useless
            //   in other normal usage scenario's this is virtually impossible
            //   however when one used reflection or such magic, this can occur
            // I would plead that the program may crash in this state
            // OR we make this state 'impossible' and the default is an error state,
            //  this would make consuming easier, but it does not address the default state and
            // For now: error
            case ResultState.Error:
                return onError(_err);

            case ResultState.Success:
                return onOk(_ok);
        }
    }


    // flatmap ? , match-bind? what is the correct functional name for this
    public unsafe Res<TNewErrKind, TNewOk>
        Map<TNewErrKind, TNewOk>(
            delegate*<RefErr<TErrKind>, Res<TNewErrKind, TNewOk>> onError
          , delegate*<TOk, Res<TNewErrKind, TNewOk>>              onOk)
        where TNewErrKind : struct, Enum
    {
        switch (_resultState)
        {
            default:
            case ResultState.NotInitialized:
            // When this occurs the system is in a invalid state,
            //   this occurs when the Result = default, which is useless
            //   in other normal usage scenario's this is virtually impossible
            //   however when one used reflection or such magic, this can occur
            // I would plead that the program may crash in this state
            // OR we make this state 'impossible' and the default is an error state,
            //  this would make consuming easier, but it does not address the default state and
            // For now: error
            case ResultState.Error:
                return onError(_err);

            case ResultState.Success:
                return onOk(_ok);
        }
    }


    public unsafe Res<TErrKind, TNewOk> MapOk<TNewOk>(delegate*<TOk, TNewOk> mapOk)
    {
        return _resultState == ResultState.Success
                   ? new Res<TErrKind, TNewOk>(mapOk(_ok))
                   : new Res<TErrKind, TNewOk>(_err);
    }

    public unsafe Res<TNewErrKind, TOk> MapErr<TNewErrKind>(
        delegate*<RefErr<TErrKind>, RefErr<TNewErrKind>> mapError)
        where TNewErrKind : struct, Enum
    {
        return _resultState == ResultState.Success
                   ? new Res<TNewErrKind, TOk>(_ok)
                   : new Res<TNewErrKind, TOk>(mapError(_err));
    }

    /// bind the ok value
    /// <remarks>AndThen</remarks>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe Res<TErrKind, TNewOk> Bind<TNewOk>(delegate*<TOk, Res<TErrKind, TNewOk>> binder)
    {
        return _resultState == ResultState.Success ? binder(_ok) : _err;
    }

    /// <summary>
    ///Calls <paramref name="onOk"/> if the result is Ok, otherwise returns the Err value of self.
    ///This function can be used for control flow based on Result values.
    /// </summary>
    public unsafe Res<TErrKind, TNewOk> AndThen<TNewOk>(delegate*<TOk, Res<TErrKind, TNewOk>> onOk)
    {
        return Bind(onOk);
    }

    /// <summary>
    ///Joins two results to a tuple of Ok values when both are Ok value, returns the first error otherwise.
    ///  This function can be used to compose results.
    /// </summary>
    public unsafe Res<TErrKind, (TOk, TU)> AndJoinOk<TU>(delegate*<Res<TErrKind, TU>> secondAction)
    {
        if (_resultState != ResultState.Success)
        {
            return _err;
        }

        var secondResult = secondAction();

        if (secondResult._resultState != ResultState.Success)
        {
            return secondResult._err;
        }

        return new Res<TErrKind, (TOk, TU)>((_ok, secondResult._ok));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Res<TErrKind, TOk> Ok(TOk ok)
    {
        return new Res<TErrKind, TOk>(ok);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Res<TErrKind, TOk> Err(TErrKind err)
    {
        return new Res<TErrKind, TOk>(err);
    }

    /// <summary>
    /// This method is not supported as spans cannot be boxed. To compare two spans, use operator==.
    /// <exception cref="System.NotSupportedException">
    /// Always thrown by this method.
    /// </exception>
    /// </summary>
    [Obsolete("Equals() on Res has will always throw an exception. Use the equality operator instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0809
    public override bool Equals(object? obj)
    {
        throw new NotSupportedException("Equals() on Res is not supported.");
    }

    /// <summary>
    /// This method is not supported as spans cannot be boxed.
    /// <exception cref="System.NotSupportedException">
    /// Always thrown by this method.
    /// </exception>
    /// </summary>
    [Obsolete("GetHashCode() on Res will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
    {
        throw new NotSupportedException("GetHashCode() on Res is not supported.");
    }

    public TOk UnwrapOr(TOk defaultWith)
    {
        return _resultState == ResultState.Success ? _ok : defaultWith;
    }
}

internal enum ResultState : byte
{
    // default, such that when not explicitly assigned it is zero
    NotInitialized = 0
  , Success        = 1
  , Error          = 2
}

public readonly ref struct RefErr<TErrKind>
{
    public readonly TErrKind           ErrKind;
    public readonly ReadOnlySpan<char> ErrMsg;

    public RefErr(TErrKind errKind, ReadOnlySpan<char> errMsg = default)
    {
        ErrKind = errKind;
        ErrMsg  = errMsg;
    }

    public RefErr(ReadOnlySpan<char> errMsg, TErrKind errKind)
    {
        ErrKind = errKind;
        ErrMsg  = errMsg;
    }

    public static implicit operator RefErr<TErrKind>(TErrKind err)
    {
        return new RefErr<TErrKind>(err);
    }

    public static implicit operator TErrKind(RefErr<TErrKind> err)
    {
        return err.ErrKind;
    }

    public override string ToString()
    {
        return $"[{ErrKind}]: {ErrMsg}";
    }

    /// <summary>
    /// This method is not supported as spans cannot be boxed. To compare two spans, use operator==.
    /// <exception cref="System.NotSupportedException">
    /// Always thrown by this method.
    /// </exception>
    /// </summary>
    [Obsolete("Equals() on RefErr has will always throw an exception. Use the equality operator instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0809
    public override bool Equals(object? obj)
    {
        throw new NotSupportedException("Equals() on RefErr is not supported.");
    }

    /// <summary>
    /// This method is not supported as spans cannot be boxed.
    /// <exception cref="System.NotSupportedException">
    /// Always thrown by this method.
    /// </exception>
    /// </summary>
    [Obsolete("GetHashCode() on RefErr will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
    {
        throw new NotSupportedException("GetHashCode() on RefErr is not supported.");
    }
}