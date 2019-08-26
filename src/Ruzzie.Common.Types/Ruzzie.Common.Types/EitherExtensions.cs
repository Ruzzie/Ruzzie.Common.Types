using System;

namespace Ruzzie.Common.Types
{
    /// <summary>
    /// Extension methods for the <see cref="Either{TLeft,TRight}"/> type.
    /// </summary>
    public static class EitherExtensions
    {
        /// <summary>
        /// Map to a new type.
        /// </summary>
        /// <typeparam name="TLeft">The type of the left.</typeparam>
        /// <typeparam name="TLeftResult">The type of the left result.</typeparam>
        /// <typeparam name="TRight">The type of the right.</typeparam>
        /// <typeparam name="TRightResult">The type of the right result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selectLeft">The select left.</param>
        /// <param name="selectRight">The select right.</param>
        /// <returns></returns>
        public static Either<TLeftResult, TRightResult> Map<TLeft, TLeftResult, TRight, TRightResult>(
            this IEither<TLeft, TRight> source,
            Func<TLeft, TLeftResult> selectLeft,
            Func<TRight, TRightResult> selectRight)
        {
            return source.Match(
                onLeft: leftValue => new Either<TLeftResult, TRightResult>(selectLeft(leftValue)),
                onRight: rightValue => new Either<TLeftResult, TRightResult>(selectRight(rightValue)));
        }

        /// <summary>
        /// Maps the left value to a new type.
        /// </summary>
        /// <typeparam name="TLeft">The type of the left.</typeparam>
        /// <typeparam name="TRight">The type of the right.</typeparam>
        /// <typeparam name="TLeftResult">The type of the left result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static Either<TLeftResult, TRight> MapLeft<TLeft,TRight,TLeftResult>(this IEither<TLeft,TRight> source, Func<TLeft, TLeftResult> selector)
        {
            return source.Map(selector, r => r);
        }

        /// <summary>
        /// Maps the right value to a new type.
        /// </summary>
        /// <typeparam name="TLeft">The type of the left.</typeparam>
        /// <typeparam name="TRight">The type of the right.</typeparam>
        /// <typeparam name="TRightResult">The type of the right result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static Either<TLeft, TRightResult> MapRight<TLeft,TRight,TRightResult>(this IEither<TLeft,TRight> source, Func<TRight, TRightResult> selector)
        {
            return source.Map(l => l, selector);
        }


        /// <summary>
        /// Create a new Left value.
        /// </summary>
        /// <typeparam name="TLeft">The type of the left.</typeparam>
        /// <typeparam name="TRight">The type of the right.</typeparam>
        /// <param name="leftValue">The left value.</param>
        /// <returns></returns>
        public static Either<TLeft, TRight> AsLeft<TLeft, TRight>(this TLeft leftValue)
        {
            return new Either<TLeft, TRight>(leftValue);
        }

        /// <summary>
        /// Create a new Right value.
        /// </summary>
        /// <typeparam name="TLeft">The type of the left.</typeparam>
        /// <typeparam name="TRight">The type of the right.</typeparam>
        /// <param name="rightValue">The right value.</param>
        /// <returns></returns>
        public static Either<TLeft, TRight> AsRight<TLeft, TRight>(this TRight rightValue)
        {
            return new Either<TLeft, TRight>(rightValue);
        }
    }
}