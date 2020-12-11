using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq
{
    class CachedTypeInfo<T>
    {
        public static readonly bool IsPrimitive;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsValueType() => !(default(T) is null);


        static CachedTypeInfo()
        {
            var type = typeof(T);

            IsPrimitive = type.IsPrimitive;
        }
    }
}
