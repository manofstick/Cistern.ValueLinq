using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class E
    {
        [Benchmark]
        public double E01a_Handcoded()
        {
            var min = _doublesNaN[0];
            foreach (var x in _doublesNaN)
                min = Math.Min(min, x);
            return min;
        }

        [Benchmark]
        public double? E01b_Handcoded()
        {
            double? min = null;
            foreach (var x in _nullableDoublesNaN)
            {
                if (x is double v)
                    min = min is double m ? Math.Min(m, v) : v;
            }
            return min;
        }

        [Benchmark]
        public double E01c_Handcoded()
        {
            var min = _nanAtMinusOne(_ints10[0]);
            foreach (var x in _ints10)
                min = Math.Min(min, _nanAtMinusOne(x));
            return min;
        }

        [Benchmark]
        public double E01d_Handcoded()
        {
            var first = true;
            var min = 0.0;
            foreach (var x in _doublesNaNIterator)
            {
                min = first ? x : Math.Min(min, x);
                first = false;
            }
            return min;
        }

        [Benchmark]
        public double E02a_Handcoded()
        {
            var max = _leadingNaN[0];
            foreach (var x in _leadingNaN)
            {
                if (x > max || double.IsNaN(max))
                    max = x;
            }
            return max;
        }

        [Benchmark]
        public double E02b_Handcoded()
        {
            var max = _leadingNaNList[0];
            foreach (var x in _leadingNaNList)
            {
                if (x > max || double.IsNaN(max))
                    max = x;
            }
            return max;
        }

        [Benchmark]
        public int? E03a_Handcoded()
        {
            int? max = null;
            foreach (var x in _nullableInts)
            {
                if (x is int v && (max is not int m || v > m))
                    max = v;
            }
            return max;
        }

        [Benchmark]
        public int? E03b_Handcoded()
        {
            int? max = null;
            foreach (var x in _ints)
            {
                if (_nullTenth(x) is int v && (max is not int m || v > m))
                    max = v;
            }
            return max;
        }

        [Benchmark]
        public short E04a_Handcoded()
        {
            var first = true;
            short min = 0;
            foreach (var x in _shortIterator)
            {
                if (first || x < min)
                    min = x;
                first = false;
            }
            return min;
        }

        [Benchmark]
        public ulong E04b_Handcoded()
        {
            var first = true;
            ulong max = 0;
            foreach (var x in _ulongIterator)
            {
                if (first || x > max)
                    max = x;
                first = false;
            }
            return max;
        }

        [Benchmark]
        public int E05a_Handcoded()
        {
            var best = _ints[0];
            var bestKey = _key(best);
            foreach (var x in _ints)
            {
                var k = _key(x);
                if (k < bestKey)
                {
                    best = x;
                    bestKey = k;
                }
            }
            return best;
        }

        [Benchmark]
        public int E05b_Handcoded()
        {
            var first = true;
            var best = 0;
            var bestKey = 0;
            foreach (var x in _iterator)
            {
                var k = _key(x);
                if (first || k > bestKey)
                {
                    best = x;
                    bestKey = k;
                    first = false;
                }
            }
            return best;
        }

        [Benchmark]
        public TimeSpan E05c_Handcoded()
        {
            var min = _timeSpans[0];
            foreach (var x in _timeSpans)
            {
                if (x < min)
                    min = x;
            }
            return min;
        }

        [Benchmark]
        public short E05d_Handcoded()
        {
            var min = _toShort(_ints[0]);
            foreach (var x in _ints)
            {
                var s = _toShort(x);
                if (s < min)
                    min = s;
            }
            return min;
        }

        [Benchmark]
        public int? E06a_Handcoded()
        {
            int? min = null;
            foreach (var x in _nullableInts)
            {
                if (x is int v && (min is not int m || v < m))
                    min = v;
            }
            return min;
        }

        [Benchmark]
        public long? E06b_Handcoded()
        {
            long? max = null;
            foreach (var x in _nullableLongs)
            {
                if (x is long v && (max is not long m || v > m))
                    max = v;
            }
            return max;
        }

        [Benchmark]
        public decimal? E06c_Handcoded()
        {
            decimal? min = null;
            foreach (var x in _nullableDecimals)
            {
                if (x is decimal v && (min is not decimal m || v < m))
                    min = v;
            }
            return min;
        }

        [Benchmark]
        public int[] E07a_Handcoded() =>
            LastTen(_iterator).ToArray();

        [Benchmark]
        public int E07b_Handcoded()
        {
            var queue = new Queue<int>();
            var sum = 0;
            foreach (var x in _iterator)
            {
                queue.Enqueue(x);
                if (queue.Count > 10)
                    sum += queue.Dequeue();
            }
            return sum;
        }

        [Benchmark]
        public int[] E07c_Handcoded() =>
            LastTen(_iterator).ToArray();

        [Benchmark]
        public int E08_Handcoded() =>
            LastTen(_iterator).Peek();

        [Benchmark]
        public int[][] E09a_Handcoded() =>
            Chunks(_tenIterator, 10_000).ToArray();

        [Benchmark]
        public int E09b_Handcoded()
        {
            var chunks = 0;
            var inChunk = 0;
            foreach (var x in _iterator)
            {
                if (++inChunk == 100)
                {
                    ++chunks;
                    inChunk = 0;
                }
            }
            return inChunk > 0 ? chunks + 1 : chunks;
        }

        [Benchmark]
        public int E10a_Handcoded()
        {
            var seen = new HashSet<int>();
            foreach (var x in _empty)
                seen.Add(x);
            return seen.Count;
        }

        [Benchmark]
        public int E10b_Handcoded()
        {
            var seen = new HashSet<int>();
            var count = 0;
            foreach (var x in _empty)
            {
                if (seen.Add(_key(x)))
                    ++count;
            }
            return count;
        }

        [Benchmark]
        public int E10c_Handcoded()
        {
            var seen = new HashSet<int>();
            foreach (var x in _empty)
                seen.Add(x);
            foreach (var x in _empty)
                seen.Add(x);
            return seen.Count;
        }

        [Benchmark]
        public int E10d_Handcoded()
        {
            var counts = new Dictionary<int, int>();
            foreach (var x in _empty)
            {
                var k = _key(x);
                counts.TryGetValue(k, out var n);
                counts[k] = n + 1;
            }
            return counts.Count;
        }

        [Benchmark]
        public int E10e_Handcoded()
        {
            var accumulators = new Dictionary<int, int>();
            foreach (var x in _empty)
            {
                var k = _key(x);
                accumulators.TryGetValue(k, out var acc);
                accumulators[k] = _add(acc, x);
            }
            return accumulators.Count;
        }

        [Benchmark]
        public KeyValuePair<int, int>[] E11a_Handcoded()
        {
            var counts = new Dictionary<int, int>();
            foreach (var x in _ints)
            {
                var k = _key(x);
                counts.TryGetValue(k, out var n);
                counts[k] = n + 1;
            }
            return Pairs(counts);
        }

        [Benchmark]
        public KeyValuePair<int, int>[] E11b_Handcoded()
        {
            var accumulators = new Dictionary<int, int>();
            foreach (var x in _ints)
            {
                var k = _key(x);
                accumulators.TryGetValue(k, out var acc);
                accumulators[k] = _add(acc, x);
            }
            return Pairs(accumulators);
        }

        // the last ten elements, oldest first
        static Queue<int> LastTen(IEnumerable<int> source)
        {
            var queue = new Queue<int>();
            foreach (var x in source)
            {
                if (queue.Count == 10)
                    queue.Dequeue();
                queue.Enqueue(x);
            }
            return queue;
        }

        static List<int[]> Chunks(IEnumerable<int> source, int size)
        {
            var chunks = new List<int[]>();
            var chunk = new List<int>();
            foreach (var x in source)
            {
                chunk.Add(x);
                if (chunk.Count == size)
                {
                    chunks.Add(chunk.ToArray());
                    chunk.Clear();
                }
            }
            if (chunk.Count > 0)
                chunks.Add(chunk.ToArray());
            return chunks;
        }

        static KeyValuePair<int, int>[] Pairs(Dictionary<int, int> dictionary)
        {
            var pairs = new KeyValuePair<int, int>[dictionary.Count];
            var i = 0;
            foreach (var pair in dictionary)
                pairs[i++] = pair;
            return pairs;
        }
    }
}
