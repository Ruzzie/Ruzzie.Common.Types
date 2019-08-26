using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Ruzzie.Common.Types
{
    /// <summary>
    /// Basic interface for Either types
    /// </summary>
    /// <typeparam name="TLeft">The type of the left.</typeparam>
    /// <typeparam name="TRight">The type of the right.</typeparam>
    public interface IEither<out TLeft, out TRight>
    {
        /// <summary>
        ///  Control flow based on pattern matching.
        /// </summary>
        /// <typeparam name="T">The output type of the match.</typeparam>
        /// <param name="onLeft">Calls this when the type is left.</param>
        /// <param name="onRight">Calls this when the type is right.</param>
        /// <returns></returns>
        T Match<T>(Func<TLeft, T> onLeft, Func<TRight, T> onRight);
    }

    /// <summary>
    /// Basic interface to indicate that an Either type is a value type.
    /// </summary>
    /// <typeparam name="TLeft">The type of the left.</typeparam>
    /// <typeparam name="TRight">The type of the right.</typeparam>
    /// <seealso cref="Ruzzie.Common.Types.IEither{TLeft, TRight}" />
    public interface IEitherValueType<out TLeft, out TRight>: IEither<TLeft,TRight>
    {
       // T Match<T>(Func<TLeft, T> onLeft, Func<TRight, T> onRight);
        
    }

    /// <summary>
    /// An Either (value) type, this type can either have a Left value or a Right value.
    /// </summary>
    /// <typeparam name="TLeft">The type of the left.</typeparam>
    /// <typeparam name="TRight">The type of the right.</typeparam>
    [DebuggerDisplay("{" + nameof(_eitherStatus) + "}, {" + nameof(_leftValue) + "}, {" + nameof(_rightValue) + "}")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public readonly struct Either<TLeft, TRight> : IEitherValueType<TLeft, TRight>, IEquatable<Either<TLeft, TRight>>
    {
        private readonly TLeft _leftValue;
        private readonly TRight _rightValue;
        private readonly Status _eitherStatus;

        /// <summary>
        /// Initializes a new instance of the <see cref="Either{TLeft, TRight}" /> struct.
        /// </summary>
        /// <param name="value">The Left value.</param>
        public Either(in TLeft value)
        {
            _leftValue = value;
            _eitherStatus = Status.Left;
            _rightValue = default!;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Either{TLeft, TRight}" /> struct.
        /// </summary>
        /// <param name="value">The Right value.</param>
        public Either(in TRight value)
        {
            _rightValue = value;
            _eitherStatus = Status.Right;
            _leftValue = default!;
        }

        /// <summary>
        /// Performs an implicit conversion from TLeft to <see cref="Ruzzie.Common.Types.Either{TLeft, TRight}" />.
        /// </summary>
        /// <param name="value">The Left value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Either<TLeft, TRight>(in TLeft value) => new Either<TLeft, TRight>(value);

        /// <summary>
        /// Performs an implicit conversion from TRight to <see cref="Ruzzie.Common.Types.Either{TLeft, TRight}" />.
        /// </summary>
        /// <param name="value">The Right value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Either<TLeft, TRight>(in TRight value) => new Either<TLeft, TRight>(value);

        ///<inheritdoc />
        /// <exception cref="EitherIsDefaultValueException">When Either is not initialized (created with default(T)) the exception will be thrown.</exception>
        /// <returns></returns>
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
                        $"Cannot perform {nameof(Match)}. The current {nameof(Either<TLeft, TRight>)} is initialized as default. It has neither a Left or a Right value.");
            }
        }

        /// <summary>
        /// Match pattern.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onLeft">The on left.</param>
        /// <param name="onRight">The on right.</param>
        /// <returns>None when the the either type is default.</returns>
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

        /// <summary>
        /// Maps the left value when present to a new type.
        /// </summary>
        /// <typeparam name="TLeftResult">The type of the left result.</typeparam>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public Either<TLeftResult, TRight> MapLeft<TLeftResult>(Func<TLeft, TLeftResult> selector)
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

        /// <summary>
        /// Maps the right value when present to a new type.
        /// </summary>
        /// <typeparam name="TRightResult">The type of the right result.</typeparam>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public Either<TLeft, TRightResult> MapRight<TRightResult>(Func<TRight, TRightResult> selector)
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

        /// <summary>
        /// Maps the left value, or the right value when present to a new type.
        /// </summary>
        /// <typeparam name="TLeftResult">The type of the left result.</typeparam>
        /// <typeparam name="TRightResult">The type of the right result.</typeparam>
        /// <param name="selectLeft">The select left.</param>
        /// <param name="selectRight">The select right.</param>
        /// <returns></returns>
        public Either<TLeftResult, TRightResult> Map<TLeftResult, TRightResult>(
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

        /// <inheritdoc />
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


        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Either<TLeft, TRight> other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            if (_eitherStatus == Status.IsDefaultValue)
            {
                return 0;
            }
            return _eitherStatus == Status.Right ? _rightValue?.GetHashCode() ?? 0 : _leftValue?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Either<TLeft, TRight> left, Either<TLeft, TRight> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Either<TLeft, TRight> left, Either<TLeft, TRight> right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class EitherIsDefaultValueException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EitherIsDefaultValueException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public EitherIsDefaultValueException(string message) : base(message)
        {

        }
        
        public EitherIsDefaultValueException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public EitherIsDefaultValueException()
        {
        }
    }
}