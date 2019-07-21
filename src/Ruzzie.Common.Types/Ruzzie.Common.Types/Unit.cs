using System;
using System.Diagnostics;

namespace Ruzzie.Common.Types
{
    /// <summary>
    /// Represents a type with a single value. This type is often used to denote the successful completion of a void-returning method (C#).
    /// </summary>
#if !NO_SERIALIZABLE
    [Serializable]
#endif
    [DebuggerDisplay("{" + nameof(ToString) + "}")]
    public class Unit : IEquatable<Unit>, IComparable<Unit>,  IComparable
    {
        public bool Equals(Unit other)
        {
            return true;
        }

        public override bool Equals(object obj)
        {            
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

        public int CompareTo(Unit other)
        {
            return 0;
        }

        public override string ToString()
        {
            return "void";
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj is Unit)
            {
                return 0;
            }

            return 1;
        }
        
        public static readonly Unit Void = new Unit();
    }
}