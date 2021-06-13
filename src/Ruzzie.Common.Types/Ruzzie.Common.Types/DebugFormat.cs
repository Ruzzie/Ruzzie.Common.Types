using System.Diagnostics;
using System.Reflection;

namespace Ruzzie.Common.Types
{
    /// <summary>
    /// Functions to format strings when in debug build mode.
    /// </summary>
    public static class DebugFormat
    {
        private static readonly bool Debug = IsAssemblyDebugBuild(Assembly.GetExecutingAssembly());
        private const string Empty = "";

        /// <summary>Returns the string if in debug mode / build.</summary>
        /// <param name="value">The value.</param>
        /// <returns>the value when in debug, empty otherwise.</returns>
        public static string IfDebug(this string value)
        {
            return Debug ? value : Empty;
        }

        /// <summary>Returns the ToString if in debug mode / build.</summary>
        /// <param name="value">The value.</param>
        /// <returns>the value when in debug, empty otherwise.</returns>
        public static string IfDebug(this object value)
        {
            return Debug ? (value.ToString() ?? Empty) : Empty;
        }

        private static bool IsAssemblyDebugBuild(Assembly assembly)
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(DebuggableAttribute), false);
            if (attributes.Length > 0)
            {
                // Just because the 'DebuggableAttribute' is found doesn't necessarily mean
                // it's a DEBUG build; we have to check the JIT Optimization flag
                // i.e. it could have the "generate PDB" checked but have JIT Optimization enabled
                if (attributes[0] is DebuggableAttribute debuggableAttribute)
                {
                    var isDebug = debuggableAttribute.IsJITOptimizerDisabled;
                    return isDebug;
                }
            }
            else
            {
                return false;
            }

            return false;
        }
    }
}