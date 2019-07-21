using System;

namespace Ruzzie.Common.Types
{
    public static class LeftRightExtensions
    {
        public static Left<TLeft, TRight> AsLeft<TLeft, TRight>(this TLeft leftValue)
        {
            return new Left<TLeft, TRight>(leftValue);
        }

        public static Right<TLeft, TRight> AsRight<TLeft, TRight>(this TRight rightValue)
        {
            return new Right<TLeft, TRight>(rightValue);
        }

        public static IEitherReferenceType<TLeftResult, TRightResult> SelectBoth<TLeft, TLeftResult, TRight, TRightResult>(
            this IEitherReferenceType<TLeft,TRight> source,
            Func<TLeft, TLeftResult> selectLeft,
            Func<TRight, TRightResult> selectRight)
        {
            return source.Match<IEitherReferenceType<TLeftResult, TRightResult>>(
                onLeft: leftValue => new Left<TLeftResult, TRightResult>(selectLeft(leftValue)),
                onRight: rightValue => new Right<TLeftResult, TRightResult>(selectRight(rightValue)));
        }

        public static IEitherReferenceType<TLeftResult, TRight> SelectLeft<TLeft,TRight,TLeftResult>(this IEitherReferenceType<TLeft,TRight> source, Func<TLeft, TLeftResult> selector)
        {
            return source.SelectBoth(selector, r => r);
        }

        public static IEitherReferenceType<TLeft, TRightResult> SelectRight<TLeft,TRight,TRightResult>(this IEitherReferenceType<TLeft,TRight> source, Func<TRight, TRightResult> selector)
        {
            return source.SelectBoth(l => l, selector);
        }

    }
}