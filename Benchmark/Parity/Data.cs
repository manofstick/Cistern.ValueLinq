using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    // The share of a source's elements a predicate from Data.Passing lets through.
    public enum Selectivity
    {
        Ten = 10,
        Fifty = 50,
        Ninety = 90,
    }

    // The data every Parity class draws from. Values are seeded random draws, so a predicate of a set selectivity passes
    // elements in no pattern a branch predictor can learn (as `x % 2 == 0` over a counting source is learnt), and every
    // run measures the same elements.
    public static class Data
    {
        public const int Seed = 20261031;

        // Values are drawn from [0, Range), so the sum of a medium source fits an int.
        public const int Range = 1000;

        public static int[] Values(int length, int seed = Seed) =>
            Keys(length, Range, seed);

        // Values drawn from [0, distinct): each value is shared by about length / distinct elements, the tie rate that
        // shapes an ordering, set or grouping entry.
        public static int[] Keys(int length, int distinct, int seed = Seed)
        {
            var random = new Random(seed);
            var keys = new int[length];
            for (var i = 0; i < keys.Length; ++i)
                keys[i] = random.Next(distinct);
            return keys;
        }

        // Passes about the given share of Values' elements.
        public static Func<int, bool> Passing(Selectivity selectivity)
        {
            var threshold = (int)selectivity * Range / 100;
            return x => x < threshold;
        }

        // The values behind an iterator, a source neither library can see through.
        public static IEnumerable<int> Iterator(int[] values)
        {
            foreach (var x in values)
                yield return x;
        }
    }
}
