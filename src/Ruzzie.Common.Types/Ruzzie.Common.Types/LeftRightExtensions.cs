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

        public static IEitherReferenceType<TLeftResult, TRight> MapLeft<TLeft,TRight,TLeftResult>(this IEitherReferenceType<TLeft,TRight> source, Func<TLeft, TLeftResult> selector)
        {
            return source.Map(selector, r => r);
        }

        public static IEitherReferenceType<TLeft, TRightResult> MapRight<TLeft,TRight,TRightResult>(this IEitherReferenceType<TLeft,TRight> source, Func<TRight, TRightResult> selector)
        {
            return source.Map(l => l, selector);
        }
    }
}