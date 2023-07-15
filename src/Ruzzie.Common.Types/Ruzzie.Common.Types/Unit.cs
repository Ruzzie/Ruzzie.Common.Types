using System.Diagnostics;

namespace Ruzzie.Common.Types;

/// <summary>
/// Represents a type with a single value. This type is often used to denote the successful completion of a void-returning method (C#).
/// </summary>
#if !NO_SERIALIZABLE
[Serializable]
#endif
[DebuggerDisplay("{" + nameof(ToString) + "}")]
public sealed class Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
{
    public static readonly Unit Void = new Unit();

    int IComparable.CompareTo(object? obj)
    {
        return 0;
    }

    public int CompareTo(Unit? other)
    {
        return 0;
    }

    public bool Equals(Unit? other)
    {
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return true;
        }

        return obj is Unit;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public static bool operator ==(Unit left, Unit right)
    {
        return true;
    }

    public static bool operator !=(Unit left, Unit right)
    {
        return false;
    }

    public override string ToString()
    {
        return "void";
    }
}