using System;
using System.Collections.Generic;

namespace Ruzzie.Common.Types
{
    public interface IEitherReferenceType<out TLeft, out TRight> : IEither<TLeft, TRight>
    {
        //T Match<T>(Func<TLeft, T> onLeft, Func<TRight, T> onRight);

        IEitherReferenceType<TLeftResult, TRightResult> SelectBoth<TLeftResult, TRightResult>(
            Func<TLeft, TLeftResult> selectLeft,
            Func<TRight, TRightResult> selectRight);
    }

    public class Left<TLeft, TRight> :  IEitherReferenceType<TLeft, TRight>, IEquatable<Left<TLeft, TRight>>
    {
        private readonly TLeft _value;

        public Left(TLeft value)
        {
            _value = value;
        }

        public T Match<T>(Func<TLeft, T> onLeft, Func<TRight, T> onRight)
        {
            return onLeft(_value);
        }

        public IEitherReferenceType<TLeftResult, TRightResult> SelectBoth<TLeftResult, TRightResult>(Func<TLeft, TLeftResult> selectLeft, Func<TRight, TRightResult> selectRight)
        {

            return new Left<TLeftResult, TRightResult>(selectLeft(_value));
            //return Match<IEitherReferenceType<TLeftResult, TRightResult>>(
            //    onLeft: leftValue => new Left<TLeftResult, TRightResult>(selectLeft(leftValue)),
            //    onRight: rightValue => new Right<TLeftResult, TRightResult>(selectRight(rightValue)));
        }

        public bool Equals(Left<TLeft, TRight> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return EqualityComparer<TLeft>.Default.Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is Left<TLeft, TRight> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TLeft>.Default.GetHashCode(_value);
        }

        public static bool operator ==(Left<TLeft, TRight> left, Left<TLeft, TRight> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Left<TLeft, TRight> left, Left<TLeft, TRight> right)
        {
            return !Equals(left, right);
        }
    }

    public class Right<TLeft, TRight> : IEitherReferenceType<TLeft, TRight>, IEquatable<Right<TLeft, TRight>>
    {
        private readonly TRight _value;

        public Right(TRight value)
        {
            _value = value;
        }

        public T Match<T>(Func<TLeft, T> onLeft, Func<TRight, T> onRight)
        {
            return onRight(_value);
        }

        public IEitherReferenceType<TLeftResult, TRightResult> SelectBoth<TLeftResult, TRightResult>(Func<TLeft, TLeftResult> selectLeft, Func<TRight, TRightResult> selectRight)
        {
            return new Right<TLeftResult, TRightResult>(selectRight(_value));
        }

        public bool Equals(Right<TLeft, TRight> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return EqualityComparer<TRight>.Default.Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is Right<TLeft, TRight> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TRight>.Default.GetHashCode(_value);
        }

        public static bool operator ==(Right<TLeft, TRight> left, Right<TLeft, TRight> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Right<TLeft, TRight> left, Right<TLeft, TRight> right)
        {
            return !Equals(left, right);
        }
    }
}
