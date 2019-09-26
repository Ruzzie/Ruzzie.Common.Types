using System;

namespace Ruzzie.Common.Types
{
    public interface IOption<out TValue> : IEitherValueType<Unit ,TValue>
    {
        IOption<TResult> Map<TResult>(Func<TValue, TResult> selector);
        T Match<T>(Func<T> onNone, Func<TValue, T> onSome);
        bool IsSome();
        bool IsNone();

        /// Experimental For method. Alternative to Match with Void return type.
        void For(Action onNone, Action<TValue> onSome);
    }
}