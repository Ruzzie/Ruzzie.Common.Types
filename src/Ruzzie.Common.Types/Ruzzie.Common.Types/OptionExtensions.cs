namespace Ruzzie.Common.Types;

public static class OptionExtensions
{
    public static Option<TValue> AsSome<TValue>(this TValue value)
    {
        return new Option<TValue>(value);
    }
}