﻿using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Ruzzie.Common.Types
{

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
    public readonly struct Option<TValue> : IOption<TValue>, IEquatable<Option<TValue>>, ISerializable
    {
        public static readonly Option<TValue> None = new Option<TValue>(Unit.Void);
        private readonly OptionVariant _variant;
        private readonly TValue _value;
        private const string VariantFieldName = "variant";
        private const string ValueFieldName = "value";

        public static Option<TValue> Some(in TValue value)
        {
            return new Option<TValue>(value);
        }

        // ReSharper disable once UnusedParameter.Local
        private Option(in Unit noValue)
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

        public T Match<T>(Func<Unit, T> onNone, Func<TValue, T> onSome)
        {
            return _variant == OptionVariant.None ? onNone(Unit.Void) : onSome(_value);
        }

        public T Match<T>(Func<T> onNone, Func<TValue, T> onSome)
        {
            return _variant == OptionVariant.None ? onNone() : onSome(_value);
        }

        public T Match<T>(Func<TValue, T> onSome, Func<T> onNone)
        {
            return _variant == OptionVariant.None ? onNone() : onSome(_value);
        }

        public IOption<TResult> Select<TResult>(Func<TValue, TResult> selector)
        {
            if (IsSome())
            {
                return new Option<TResult>(selector(_value));
            }

            return Option<TResult>.None;
        }

        ///Returns the contained value or a default.
        public TValue UnwrapOr(TValue @default)
        {
            return IsSome() ? _value : @default;
        }

        ///Returns the contained value or computes it from a closure.
        public TValue UnwrapOrElse(Func<TValue> orElse)
        {
            return IsSome() ? _value : orElse();
        }

        public bool IsNone()
        {
            return _variant == OptionVariant.None;
        }

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

        //Serialize
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(VariantFieldName, (byte) _variant);
            if (IsSome())
            {
                info.AddValue(ValueFieldName, _value);
            }
        }

        //Deserialize
        private Option(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            _variant = (OptionVariant) serializationInfo.GetByte(VariantFieldName);
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