using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Ruzzie.Common.Types;

public delegate TU OnNone<out TU>();

public delegate TU OnSome<out TU, T>(in T some);

public static class Option
{
    public static Option<TValue> Some<TValue>(in TValue value)
    {
        return new Option<TValue>(value);
    }

    public static Option<TValue> None<TValue>()
    {
        return Option<TValue>.None;
    }
}

public static class OptionExtensions
{
    public static Option<TValue> AsSome<TValue>(this TValue value)
    {
        return new Option<TValue>(value);
    }

    // ReSharper disable once ConvertNullableToShortForm
    public static Option<TValue> ToOption<TValue>(this Nullable<TValue> value) where TValue : struct
    {
        if (value.HasValue)
        {
            return value.Value.AsSome();
        }

        return Option<TValue>.None;
    }

    public static Option<TValue> ToOption<TValue>(this TValue? value)
    {
        if (value != null)
        {
            return value.AsSome();
        }

        return Option<TValue>.None;
    }
}

[Serializable]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[SkipLocalsInit]
public readonly record struct Option<TValue> : ISerializable, IFormattable
{
    private const          string         HasValueFieldName = "hasValue";
    private const          string         ValueFieldName    = "value";
    public static readonly Option<TValue> None              = default;


    private readonly TValue        _value;
    private readonly OptionVariant _variant = OptionVariant.None;

    private string DebuggerDisplay => $"{_variant}({ToString()})";

    public Option(in TValue value)
    {
        _value   = value;
        _variant = OptionVariant.Some;
    }

    //Deserialize
    private Option(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
        _variant = serializationInfo.GetBoolean(HasValueFieldName) ? OptionVariant.Some : OptionVariant.None;
        if (_variant == OptionVariant.Some)
        {
            var value = (TValue?)serializationInfo.GetValue(ValueFieldName, typeof(TValue));
            _value = value!; //note: decide on whether to panic or leave this as is, since it is possible that the caller intended to serialize a null value
        }
        else
        {
            _value = default!;
        }
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (IsNone())
        {
            return "";
        }

        if (_value is IFormattable formattable)
        {
            return formattable.ToString(format, formatProvider);
        }

        return format != null ? string.Format(formatProvider, format, _value) : _value?.ToString() ?? "";
    }

    //Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(HasValueFieldName, _variant == OptionVariant.Some);
        if (IsSome())
        {
            info.AddValue(ValueFieldName, _value, typeof(TValue));
        }
    }

    public override int GetHashCode()
    {
        if (_value != null && _variant == OptionVariant.Some)
        {
            return _value.GetHashCode();
        }

        return 0;
    }

    public static Option<TValue> Some(in TValue value)
    {
        return new Option<TValue>(value);
    }

    public static implicit operator Option<TValue>(in TValue value)
    {
        return new Option<TValue>(value);
    }

    public T Match<T>(Func<T> onNone, Func<TValue, T> onSome)
    {
        return IsNone() ? onNone() : onSome(_value);
    }

    public T Match<T>(OnNone<T> onNone, OnSome<T, TValue> onSome)
    {
        return IsNone() ? onNone() : onSome(_value);
    }

    public unsafe T Match<T>(delegate*<T> onNone, delegate*<TValue, T> onSome)
    {
        return IsNone() ? onNone() : onSome(_value);
    }

    /// <summary>
    /// Maps an <see cref="Option{TValue}"/>  by applying a function to the value. The function will only be applied when there is a value. None is returned otherwise.
    /// </summary>
    public Option<TResult> Map<TResult>(Func<TValue, TResult> selector)
    {
        return IsSome() ? new Option<TResult>(selector(_value)) : Option<TResult>.None;
    }

    /// <summary>
    /// Maps an <see cref="Option{TValue}"/> by applying a function to the value. The function will only be applied when there is a value. None is returned otherwise.
    /// </summary>
    public unsafe Option<TResult> Map<TResult>(delegate*<TValue, TResult> selector)
    {
        return IsSome() ? new Option<TResult>(selector(_value)) : Option<TResult>.None;
    }

    /// <summary>
    /// Applies a function to the contained value (if any), or returns the provided default.
    /// </summary>
    public unsafe T MapOr<T>(T @default, delegate*<TValue, T> mapValueTo)
    {
        return IsSome() ? mapValueTo(_value) : @default;
    }

    /// <summary>
    /// Applies a function to the contained value (if any), or returns the provided default.
    /// </summary>
    public T MapOr<T>(T @default, Func<TValue, T> mapValueTo)
    {
        return IsSome() ? mapValueTo(_value) : @default;
    }

    /// <summary>
    /// Experimental For method. Alternative to Match with Void return type.
    /// </summary>
    public void For(Action onNone, Action<TValue> onSome)
    {
        if (IsSome())
        {
            onSome(_value);
        }
        else
        {
            onNone();
        }
    }

    ///<summary>
    ///Returns the contained value or a default.
    ///</summary>
    public TValue UnwrapOr(TValue @default)
    {
        return IsSome() ? _value : @default;
    }

    ///<summary>
    ///Returns the contained value or computes it from a closure.
    ///</summary>
    public TValue UnwrapOrElse(Func<TValue> orElse)
    {
        return IsSome() ? _value : orElse();
    }

    /// <summary>
    /// When a contained value is present, <c>true</c> will be returned and the <paramref name="value"/> will be set to the contained value.
    /// </summary>
    /// <param name="value">The contained value when present, <c>default</c> otherwise.</param>
    /// <returns><c>true</c> when a value is present, <c>false</c> otherwise.</returns>
    public bool TryGetValue(out TValue? value)
    {
        if (IsSome())
        {
            value = _value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// When a contained value is present, <c>true</c> will be returned and the <paramref name="value"/> will be set to the contained value.
    /// </summary>
    /// <param name="value">The contained value when present, <paramref name="default"/> otherwise.</param>
    /// <param name="default">The value to return when no value is present</param>
    /// <returns><c>true</c> when a value is present, <c>false</c> otherwise.</returns>
    public bool TryGetValue(out TValue value, TValue @default)
    {
        if (IsSome())
        {
            value = _value;
            return true;
        }

        value = @default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNone()
    {
        return _variant == OptionVariant.None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSome()
    {
        return _variant == OptionVariant.Some;
    }

    public override string ToString()
    {
        if (IsNone())
        {
            return "";
        }

        return _value?.ToString() ?? "";
    }
}

internal enum OptionVariant : byte
{
    None = 0
  , Some = 1
}