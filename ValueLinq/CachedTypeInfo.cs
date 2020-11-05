namespace Cistern.ValueLinq
{
    class CachedTypeInfo<T>
    {
        public static readonly bool IsPrimitive;

        static CachedTypeInfo()
        {
            var type = typeof(T);

            IsPrimitive = type.IsPrimitive;
        }
    }
}
