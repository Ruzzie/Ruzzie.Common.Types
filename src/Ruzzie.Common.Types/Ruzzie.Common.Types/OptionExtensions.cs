using System;

namespace Ruzzie.Common.Types
{
    public static class OptionExtensions
    {
        public static Option<TValue> AsSome<TValue>(this TValue value)
        {
            return new Option<TValue>(value);
        }

        /// <summary>
        /// Applies a function to the contained value (if any), or returns the provided default.
        /// </summary>
        public static T MapOr<T, TValue>(this Option<TValue> self, T @default, Func<TValue, T> mapValueTo)
        {
            return self.Match(ReturnDefault, mapValueTo);

            T ReturnDefault()
            {
                return @default;
            }
        }
    }
}