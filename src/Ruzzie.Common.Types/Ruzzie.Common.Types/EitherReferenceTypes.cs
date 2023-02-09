using System;
using System.Collections.Generic;

namespace Ruzzie.Common.Types;

/// <summary>
/// Basic interface to indicate that an Either type is a reference type.
/// </summary>
/// <typeparam name="TLeft">The type of the left.</typeparam>
/// <typeparam name="TRight">The type of the right.</typeparam>
/// <seealso cref="Ruzzie.Common.Types.IEither{TLeft, TRight}" />
public interface IEitherReferenceType<out TLeft, out TRight> : IEither<TLeft, TRight>
{
    IEitherReferenceType<TLeftResult, TRightResult> Map<TLeftResult, TRightResult>(
        Func<TLeft, TLeftResult>   selectLeft,
        Func<TRight, TRightResult> selectRight);
}

public class Left<TLeft, TRight> :  IEitherReferenceType<TLeft, TRight>, IEquatable<Left<TLeft, TRight>>
{
    private readonly TLeft _value;

    public Left(TLeft value)
    {
        _value = value;
    }

    /// <inheritdoc />
    public void For(Action<TLeft> onLeft, Action<TRight> onRight)
    {
        onLeft(_value);
    }

    /// <inheritdoc />
    public T Match<T>(Func<TLeft, T> onLeft, Func<TRight, T> onRight)
    {
        return onLeft(_value);
    }

    /// <inheritdoc />
    public IEitherReferenceType<TLeftResult, TRightResult> Map<TLeftResult, TRightResult>(Func<TLeft, TLeftResult> selectLeft, Func<TRight, TRightResult> selectRight)
    {

        return new Left<TLeftResult, TRightResult>(selectLeft(_value));
        //return Match<IEitherReferenceType<TLeftResult, TRightResult>>(
        //    onLeft: leftValue => new Left<TLeftResult, TRightResult>(selectLeft(leftValue)),
        //    onRight: rightValue => new Right<TLeftResult, TRightResult>(selectRight(rightValue)));
    }

    public bool Equals(Left<TLeft, TRight>? other)
    {
#pragma warning disable IDE0041 // Use 'is null' check
        if (ReferenceEquals(null, other))

        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }
#pragma warning restore IDE0041 // Use 'is null' check

        return EqualityComparer<TLeft>.Default.Equals(_value, other._value);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Left<TLeft, TRight> other && Equals(other);
    }

    public override int GetHashCode()
    {
        if (_value != null)
        {
            return EqualityComparer<TLeft>.Default.GetHashCode(_value);
        }
        else
        {
            return 0;
        }
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

    /// <inheritdoc />
    public void For(Action<TLeft> onLeft, Action<TRight> onRight)
    {
        onRight(_value);
    }

    /// <inheritdoc />
    public T Match<T>(Func<TLeft, T> onLeft, Func<TRight, T> onRight)
    {
        return onRight(_value);
    }

    /// <inheritdoc />
    public IEitherReferenceType<TLeftResult, TRightResult> Map<TLeftResult, TRightResult>(Func<TLeft, TLeftResult> selectLeft, Func<TRight, TRightResult> selectRight)
    {
        return new Right<TLeftResult, TRightResult>(selectRight(_value));
    }

    public bool Equals(Right<TLeft, TRight>? other)
    {
#pragma warning disable IDE0041 // Use 'is null' check
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }
#pragma warning restore IDE0041 // Use 'is null' check
        return EqualityComparer<TRight>.Default.Equals(_value, other._value);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Right<TLeft, TRight> other && Equals(other);
    }

    public override int GetHashCode()
    {

        return _value != null ? EqualityComparer<TRight>.Default.GetHashCode(_value) : 0;
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