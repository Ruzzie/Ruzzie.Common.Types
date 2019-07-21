namespace Ruzzie.Common.Types
{
    public static class DebugFormat
    {
        private const string Empty = "";

        public static string IfDebug(this string value)
        {
#if DEBUG
            return value;
#else                

                return Empty;
#endif
        }
        
        public static string IfDebug(this object value)
        {
#if DEBUG
                
            return value.ToString();
#else                

                return Empty;
#endif
        }

    }
}