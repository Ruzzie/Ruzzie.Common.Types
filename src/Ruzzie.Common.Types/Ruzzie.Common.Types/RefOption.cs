using System.Runtime.CompilerServices;

namespace Ruzzie.Common.Types;

[SkipLocalsInit]
public readonly ref struct RefOption<TValue>
{
    private readonly OptionVariant _variant = OptionVariant.None;
    private readonly TValue        _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RefOption<TValue> None()
    {
        return new RefOption<TValue>();
    }

    public RefOption(TValue value)
    {
        _variant = OptionVariant.Some;
        _value   = value;
    }

    /// <summary>
    /// When a contained value is present, <c>true</c> will be returned and the <paramref name="value"/> will be set to the contained value.
    /// </summary>
    /// <param name="value">The contained value when present, <paramref name="default"/> otherwise.</param>
    /// <param name="default">The value to return when no value is present</param>
    /// <returns><c>true</c> when a value is present, <c>false</c> otherwise.</returns>
    public bool TryGetValue(out TValue value, TValue @default)
    {
        if (_variant == OptionVariant.Some)
        {
            value = _value;
            return true;
        }

        value = @default;
        return false;
    }

    public unsafe T Match<T>(delegate*<T> onNone, delegate*<TValue, T> onSome)
    {
        return _variant == OptionVariant.None ? onNone() : onSome(_value);
    }

    // experimental naming
    public T When<T>(Func<T> isNone, Func<TValue, T> isSome)
    {
        return _variant == OptionVariant.None ? isNone() : isSome(_value);
    }

    public RefOption<T> Map<T>(Func<TValue, T> mapping)
    {
        return _variant == OptionVariant.Some
                   ? new RefOption<T>(mapping(_value))
                   : RefOption<T>.None();
    }

    public unsafe RefOption<T> Map<T>(delegate*<TValue, T> mapping)
    {
        return _variant == OptionVariant.Some
                   ? new RefOption<T>(mapping(_value))
                   : RefOption<T>.None();
    }

    public unsafe RefOption<T> Bind<T>(delegate*<TValue, RefOption<T>> binder)
    {
        return _variant == OptionVariant.Some ? binder(_value) : RefOption<T>.None();
    }

    public RefOption<(TValue, T2)> And<T2>(RefOption<T2> other)
    {
        if (_variant == OptionVariant.Some && other._variant == OptionVariant.Some)
        {
            return new RefOption<(TValue, T2)>((_value, other._value));
        }
        else
        {
            return RefOption<(TValue, T2)>.None();
        }
    }

    public RefOption<TCombined> And<T2, TCombined>(RefOption<T2> other, Func<TValue, T2, TCombined> map)
    {
        if (_variant == OptionVariant.Some && other._variant == OptionVariant.Some)
        {
            return new RefOption<TCombined>(map(_value, other._value));
        }
        else
        {
            return RefOption<TCombined>.None();
        }
    }

    ///<summary>
    ///Returns the contained value or a default.
    ///</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue UnwrapOr(TValue @default)
    {
        return _variant == OptionVariant.Some ? _value : @default;
    }

    public TValue DefaultValue(TValue value)
    {
        return UnwrapOr(value);
    }


    ///<summary>
    ///Returns the contained value or computes it from a closure.
    ///</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue UnwrapOrElse(Func<TValue> orElse)
    {
        return _variant == OptionVariant.Some ? _value : orElse();
    }

    public TValue DefaultWith(Func<TValue> orElse)
    {
        return UnwrapOrElse(orElse);
    }

    public unsafe TValue DefaultWith(delegate*<TValue> orElse)
    {
        return UnwrapOrElse(orElse);
    }

    ///<summary>
    ///Returns the contained value or computes it from a closure.
    ///</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe TValue UnwrapOrElse(delegate*<TValue> orElse)
    {
        return _variant == OptionVariant.Some ? _value : orElse();
    }
}