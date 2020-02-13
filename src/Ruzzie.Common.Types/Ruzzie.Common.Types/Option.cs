using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Ruzzie.Common.Types
{
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

    [Serializable]
    [DebuggerDisplay("{"+nameof(_variant) + "}, {"+nameof(_value) + "}")]
    public readonly struct Option<TValue> : IOption<TValue>, IEquatable<Option<TValue>>, ISerializable, IFormattable
    {
        public static readonly Option<TValue> None = new Option<TValue>(Unit.Void);
        private readonly OptionVariant _variant;
        private readonly TValue _value;
        private const string HasValueFieldName = "hasValue";
        private const string ValueFieldName = "value";

        public static Option<TValue> Some(in TValue value)
        {
            return new Option<TValue>(value);
        }

        // ReSharper disable once UnusedParameter.Local
#pragma warning disable IDE0060 // Remove unused parameter
        private Option(in Unit noValue)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            _variant = OptionVariant.None;
            _value = default!;
        }
        
        public Option(in TValue value)
        {
            _value = value;
            _variant = OptionVariant.Some;
        }

        public static implicit operator Option<TValue>(in TValue value) => new Option<TValue>(value);
        // ReSharper disable once UnusedParameter.Global
        public static implicit operator Option<TValue>(in Unit _) => new Option<TValue>(Unit.Void);

        T IEither<Unit, TValue>.Match<T>(Func<Unit, T> onNone, Func<TValue, T> onSome)
        {
            return _variant == OptionVariant.None ? onNone(Unit.Void) : onSome(_value);
        }

        public T Match<T>(Func<T> onNone, Func<TValue, T> onSome)
        {
            return _variant == OptionVariant.None ? onNone() : onSome(_value);
        }

        public T Match<T>(OnNone<T> onNone, OnSome<T, TValue> onSome)
        {
            return _variant == OptionVariant.None ? onNone() : onSome(_value);
        }

        /// <summary>
        /// Maps an <see cref="Option{TValue}"/> <see cref="IOption{TResult}"/> by applying a function to the value. The function will only be applied when there is a value. None is returned otherwise.
        /// </summary>
        IOption<TResult> IOption<TValue>.Map<TResult>(Func<TValue, TResult> selector)
        {
            return Map(selector);
        }

        /// <summary>
        /// Maps an <see cref="Option{TValue}"/> <see cref="IOption{TResult}"/> by applying a function to the value. The function will only be applied when there is a value. None is returned otherwise.
        /// </summary>
        public Option<TResult> Map<TResult>(Func<TValue, TResult> selector)
        {
            if (IsSome())
            {
                return new Option<TResult>(selector(_value));
            }

            return Option<TResult>.None;
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

        /// <inheritdoc />
        void IEither<Unit, TValue>.For(Action<Unit> onNone, Action<TValue> onSome)
        {
            For(() => onNone(Unit.Void), onSome);
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
        public bool Equals(Option<TValue> other)
        {
            if (IsNone() && other.IsNone())
            {
                return true;
            }

            return Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            return obj is Option<TValue> other && Equals(other);
        }

        public override int GetHashCode()
        {
            if (_value != null && _variant == OptionVariant.Some)
            {
                return _value.GetHashCode();
            }

            return Unit.Void.GetHashCode();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (IsNone())
            {
                return string.Empty;
            }

            if (_value is IFormattable formattable)
            {
                return formattable.ToString(format, formatProvider);
            }

            return string.Format(formatProvider, format);
        }

        public override string ToString()
        {
            if (IsNone())
            {
                return string.Empty;
            }

            return _value.ToString();
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

        //Deserialize
        private Option(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            _variant = serializationInfo.GetBoolean(HasValueFieldName) ? OptionVariant.Some : OptionVariant.None;
            if (_variant == OptionVariant.Some)
            {
                _value = (TValue) serializationInfo.GetValue(ValueFieldName, typeof(TValue));
            }
            else
            {
                _value = default!;
            }
        }

        public static bool operator ==(Option<TValue> leftOption, Option<TValue> rightOption)
        {
            return Equals(leftOption, rightOption);
        }

        public static bool operator !=(Option<TValue> leftOption, Option<TValue> rightOption)
        {
            return !Equals(leftOption, rightOption);
        }

        private enum OptionVariant : byte //We use an enum value so that it is more readable
        {
            None = 0,
            Some = 1
        }
    }
}