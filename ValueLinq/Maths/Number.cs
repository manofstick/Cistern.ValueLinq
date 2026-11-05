using System;
using System.Numerics;

namespace Cistern.ValueLinq.Maths
{
    /// <summary>
    /// Facts about a numeric type that the aggregators branch on. A readonly static is a constant to the JIT once
    /// the class is initialised, so the branch it guards is resolved per instantiation.
    /// </summary>
    static class Number<T>
        where T : struct, INumber<T>
    {
        /// <summary>The type holds whole numbers only, so a sum is order-independent and can overflow.</summary>
        public static readonly bool IsIntegral = Implements(typeof(IBinaryInteger<>));

        public static readonly bool IsSigned = Implements(typeof(ISignedNumber<>));

        // the closed interface cannot be named here, since its self-constraint is narrower than INumber<T>
        private static bool Implements(Type openInterface)
        {
            foreach (var i in typeof(T).GetInterfaces())
            {
                if (i.IsGenericType && i.GetGenericTypeDefinition() == openInterface)
                    return true;
            }
            return false;
        }
    }

    static class Number
    {
        /// <summary>
        /// A floating point sum is divided before it is narrowed (a float average is computed in double); an integer
        /// sum is widened first, since dividing it by the count would be an integer division.
        /// </summary>
        public static TQuotient Divide<TAccumulator, TQuotient>(TAccumulator sum, long count)
            where TAccumulator : struct, INumber<TAccumulator>
            where TQuotient : struct, INumber<TQuotient>
            => Number<TAccumulator>.IsIntegral
                ? TQuotient.CreateChecked(sum) / TQuotient.CreateChecked(count)
                : TQuotient.CreateChecked(sum / TAccumulator.CreateChecked(count));
    }
}
