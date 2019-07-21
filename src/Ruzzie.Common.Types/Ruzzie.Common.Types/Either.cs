using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Ruzzie.Common.Types
{
    public static class EitherExtensions
    {
        public static Either<TLeftResult, TRightResult> SelectBoth<TLeft, TLeftResult, TRight, TRightResult>(
            this Either<TLeft, TRight> source,
            Func<TLeft, TLeftResult> selectLeft,
            Func<TRight, TRightResult> selectRight)
        {
            return source.Match(
                onLeft: leftValue => new Either<TLeftResult, TRightResult>(selectLeft(leftValue)),
                onRight: rightValue => new Either<TLeftResult, TRightResult>(selectRight(rightValue)));
        }

        public static Either<TLeftResult, TRight> SelectLeft<TLeft,TRight,TLeftResult>(this Either<TLeft,TRight> source, Func<TLeft, TLeftResult> selector)
        {
            return source.SelectBoth(selector, r => r);
        }

        public static Either<TLeft, TRightResult> SelectRight<TLeft,TRight,TRightResult>(this Either<TLeft,TRight> source, Func<TRight, TRightResult> selector)
        {
            return source.SelectBoth(l => l, selector);
        }

        public static Either<TLeftResult, TRightResult> SelectBoth<TLeft, TLeftResult, TRight, TRightResult>(
            this IEither<TLeft, TRight> source,
            Func<TLeft, TLeftResult> selectLeft,
            Func<TRight, TRightResult> selectRight)
        {
            return source.Match(
                onLeft: leftValue => new Either<TLeftResult, TRightResult>(selectLeft(leftValue)),
                onRight: rightValue => new Either<TLeftResult, TRightResult>(selectRight(rightValue)));
        }

        public static Either<TLeftResult, TRight> SelectLeft<TLeft,TRight,TLeftResult>(this IEither<TLeft,TRight> source, Func<TLeft, TLeftResult> selector)
        {
            return source.SelectBoth(selector, r => r);
        }

        public static Either<TLeft, TRightResult> SelectRight<TLeft,TRight,TRightResult>(this IEither<TLeft,TRight> source, Func<TRight, TRightResult> selector)
        {
            return source.SelectBoth(l => l, selector);
        }


        public static Either<TLeft, TRight> AsLeft<TLeft, TRight>(this TLeft leftValue)
        {
            return new Either<TLeft, TRight>(leftValue);
        }

        public static Either<TLeft, TRight> AsRight<TLeft, TRight>(this TRight rightValue)
        {
            return new Either<TLeft, TRight>(rightValue);
        }
    }

    public interface IEither<out TLeft, out TRight>
    {
        T Match<T>(Func<TLeft, T> onLeft, Func<TRight, T> onRight);
    }

    public interface IEitherValueType<out TLeft, out TRight>: IEither<TLeft,TRight>
    {
       // T Match<T>(Func<TLeft, T> onLeft, Func<TRight, T> onRight);
        
    }

    [DebuggerDisplay("{"+nameof(_eitherStatus) + "}, {"+nameof(_leftValue) + "}, {"+nameof(_rightValue)+"}" )]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public readonly struct Either<TLeft, TRight> : IEitherValueType<TLeft, TRight>, IEquatable<Either<TLeft,TRight>>
    {
        private readonly TLeft _leftValue;
        private readonly TRight _rightValue;
        private readonly Status _eitherStatus;
        
        public Either(in TLeft value)
        {
            _leftValue = value;
            _eitherStatus = Status.Left;
            _rightValue = default!;
        }

        public Either(in TRight value)
        {
            _rightValue = value;
            _eitherStatus = Status.Right;
            _leftValue = default!;
        }

        public static implicit operator Either<TLeft, TRight>(in TLeft value) => new Either<TLeft, TRight>(value);

        public static implicit operator Either<TLeft, TRight>(in TRight value) => new Either<TLeft, TRight>(value);

        public T Match<T>(Func<TLeft, T> onLeft, Func<TRight, T> onRight)
        {
            switch (_eitherStatus)
            {
                case Status.Right:
                    return onRight(_rightValue);
                case Status.Left:
                    return onLeft(_leftValue);
                default:
                case Status.IsDefaultValue:
                    throw new EitherIsDefaultValueException(
                        $"Cannot perform {nameof(Match)}. The current {nameof(Either<TLeft,TRight>)} is initialized as default. It has neither a Left or a Right value.");
            }
        }

        public Option<T> Match<T>(Func<TLeft, Option<T>> onLeft, Func<TRight, Option<T>> onRight)
        {
            switch (_eitherStatus)
            {
                case Status.Right:
                    return onRight(_rightValue);
                case Status.Left:
                    return onLeft(_leftValue);
                default:
                case Status.IsDefaultValue:
                    return Option<T>.None;
            }
        }

        public Either<TLeftResult, TRight> SelectLeft<TLeftResult>(Func<TLeft, TLeftResult> selector)
        {
            switch (_eitherStatus)
            {
                case Status.Right:
                    return _rightValue;
                case Status.Left:
                    return selector(_leftValue);
                case Status.IsDefaultValue:
                default:
                    return default;
            }
        }

        public Either<TLeft, TRightResult> SelectRight<TRightResult>(Func<TRight, TRightResult> selector)
        {
            switch (_eitherStatus)
            {
                case Status.Right:
                    return selector(_rightValue);
                case Status.Left:
                    return _leftValue;
                case Status.IsDefaultValue:
                default:
                    return default;
            }
        }

        public Either<TLeftResult, TRightResult> SelectBoth<TLeftResult, TRightResult>(
            Func<TLeft, TLeftResult> selectLeft,
            Func<TRight, TRightResult> selectRight)
        {
            switch (_eitherStatus)
            {
                case Status.Right:
                    return new Either<TLeftResult, TRightResult>(selectRight(_rightValue));
                case Status.Left:
                    return new Either<TLeftResult, TRightResult>(selectLeft(_leftValue));
                case Status.IsDefaultValue:
                default:
                    return default;
            }
        }

        private enum Status
        {
            IsDefaultValue = 0,//Uninitialized (like with default(T))
            Left = 1,
            Right = 2
        }

        public bool Equals(Either<TLeft, TRight> other)
        {
            if (_eitherStatus != other._eitherStatus)
            {
                return false;
            }

            if (_eitherStatus == Status.IsDefaultValue)
            {
                return object.Equals(_rightValue, other._rightValue) && object.Equals(_leftValue, other._leftValue);
            }

            if (_eitherStatus == Status.Right)
            {
                return object.Equals(_rightValue, other._rightValue);
            }

            return object.Equals(_leftValue, other._leftValue);
        }


        public override bool Equals(object obj)
        {
            return obj is Either<TLeft, TRight> other && Equals(other);
        }

        public override int GetHashCode()
        {
            if (_eitherStatus == Status.IsDefaultValue)
            {
                return 0;
            }
            return _eitherStatus == Status.Right ? _rightValue?.GetHashCode() ?? 0 : _leftValue?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Either<TLeft, TRight> left, Either<TLeft, TRight> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Either<TLeft, TRight> left, Either<TLeft, TRight> right)
        {
            return !left.Equals(right);
        }
    }

    public class EitherIsDefaultValueException : Exception
    {
        public EitherIsDefaultValueException(string message) : base(message)
        {
            
        }
    }
}