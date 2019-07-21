using System;

namespace Ruzzie.Common.Types
{
    public interface IOption<out TValue> : IEitherValueType<Unit ,TValue>
    {
        IOption<TResult> Select<TResult>(Func<TValue, TResult> selector);
        T Match<T>(Func<T> onNone, Func<TValue, T> onSome);
        bool IsSome();
        bool IsNone();
    }
}