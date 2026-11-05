using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // First, Last and ElementAt over a node made of parts, above an OrderBy that answers by running user code (its
    // selection) or a source that is run in full: the user code runs in one pass, whether the parts answer or not.
    public class ElementRequestsRunOnce
    {
        private static readonly int[] Data = [5, 3, 9, 1, 7];

        private readonly List<string> _failures = [];

        private sealed class Counter(bool known, bool empty, bool matches)
        {
            public int Length => empty ? 0 : Data.Length;
            public int Keys { get; private set; }
            public int Tests { get; private set; }
            public int Enumerations { get; private set; }
            public int SecondEnumerations { get; private set; }

            public int Key(int x)
            {
                ++Keys;
                return x;
            }

            public bool Match(int x)
            {
                ++Tests;
                return matches;
            }

            public IEnumerable<int> Source()
            {
                var items = empty ? [] : Data;
                if (known)
                    return items;

                return Iterate(items);
            }

            private IEnumerable<int> Iterate(int[] items)
            {
                ++Enumerations;
                foreach (var item in items)
                    yield return item;
            }

            public IEnumerable<int> Second()
            {
                ++SecondEnumerations;
                for (var i = 0; i < Data.Length; ++i)
                    yield return i * 10;
            }
        }

        public static IEnumerable<object[]> Sources()
        {
            foreach (var known in new[] { true, false })
                foreach (var empty in new[] { false, true })
                    foreach (var matches in new[] { true, false })
                        yield return [known, empty, matches];
        }

        [Theory]
        [MemberData(nameof(Sources))]
        public void OverOrderBy(bool known, bool empty, bool matches)
        {
            Counter New() => new(known, empty, matches);

            Check("OrderBy", New, c => c.Source().OrderBy(c.Key));
            Check("Index", New, c => c.Source().OrderBy(c.Key).Index());
            Check("Append", New, c => c.Source().OrderBy(c.Key).Append(99));
            Check("Prepend", New, c => c.Source().OrderBy(c.Key).Prepend(99));
            Check("DefaultIfEmpty", New, c => c.Source().OrderBy(c.Key).DefaultIfEmpty(-1));
            Check("Chunk", New, c => c.Source().OrderBy(c.Key).Chunk(2));
            Check("Skip", New, c => c.Source().OrderBy(c.Key).Skip(1));
            Check("Take", New, c => c.Source().OrderBy(c.Key).Take(3));
            Check("Reverse", New, c => c.Source().OrderBy(c.Key).Reverse());
            Check("Zip", New, c => c.Source().OrderBy(c.Key).Zip(c.Second().OfEnumerable()));
            Check("ZipSelector", New, c => c.Source().OrderBy(c.Key).Zip(c.Second().OfEnumerable(), (x, y) => x + y));

            Assert.True(_failures.Count == 0, string.Join(Environment.NewLine, _failures));
        }

        [Theory]
        [MemberData(nameof(Sources))]
        public void OverWhereThenOrderBy(bool known, bool empty, bool matches)
        {
            Counter New() => new(known, empty, matches);

            Check("OrderBy", New, c => c.Source().Where(c.Match).OrderBy(c.Key));
            Check("Index", New, c => c.Source().Where(c.Match).OrderBy(c.Key).Index());
            Check("Append", New, c => c.Source().Where(c.Match).OrderBy(c.Key).Append(99));
            Check("Prepend", New, c => c.Source().Where(c.Match).OrderBy(c.Key).Prepend(99));
            Check("DefaultIfEmpty", New, c => c.Source().Where(c.Match).OrderBy(c.Key).DefaultIfEmpty(-1));
            Check("Chunk", New, c => c.Source().Where(c.Match).OrderBy(c.Key).Chunk(2));
            Check("Skip", New, c => c.Source().Where(c.Match).OrderBy(c.Key).Skip(1));
            Check("Take", New, c => c.Source().Where(c.Match).OrderBy(c.Key).Take(3));
            Check("Reverse", New, c => c.Source().Where(c.Match).OrderBy(c.Key).Reverse());
            Check("Zip", New, c => c.Source().Where(c.Match).OrderBy(c.Key).Zip(c.Second().OfEnumerable()));
            Check("ZipSelector", New, c => c.Source().Where(c.Match).OrderBy(c.Key).Zip(c.Second().OfEnumerable(), (x, y) => x + y));

            Assert.True(_failures.Count == 0, string.Join(Environment.NewLine, _failures));
        }

        private void Check<T, TNode, TEnumerator>(string shape, Func<Counter> counter, Func<Counter, ValueEnumerable<T, TNode, TEnumerator>> build)
            where TNode : INode<T>
            where TEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<TNode, TEnumerator>
        {
            var items = new List<T>();
            foreach (var item in build(counter()))
                items.Add(item);

            Run(shape, "First", counter, build, items, 0, false, v => v.First());
            Run(shape, "FirstOrDefault", counter, build, items, 0, true, v => v.FirstOrDefault()!);
            Run(shape, "Last", counter, build, items, ^1, false, v => v.Last());
            Run(shape, "LastOrDefault", counter, build, items, ^1, true, v => v.LastOrDefault()!);
            for (var i = 0; i <= Data.Length + 2; ++i)
            {
                var at = i;
                Run(shape, $"ElementAt({at})", counter, build, items, at, false, v => v.ElementAt(at));
                Run(shape, $"ElementAtOrDefault({at})", counter, build, items, at, true, v => v.ElementAtOrDefault(at)!);
                if (at > 0)
                {
                    Run(shape, $"ElementAt(^{at})", counter, build, items, ^at, false, v => v.ElementAt(^at));
                    Run(shape, $"ElementAtOrDefault(^{at})", counter, build, items, ^at, true, v => v.ElementAtOrDefault(^at)!);
                }
            }
        }

        private void Run<T, TNode, TEnumerator>(string shape, string query, Func<Counter> counter, Func<Counter, ValueEnumerable<T, TNode, TEnumerator>> build, List<T> items, Index index, bool orDefault, Func<ValueEnumerable<T, TNode, TEnumerator>, T> ask)
            where TNode : INode<T>
            where TEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<TNode, TEnumerator>
        {
            var offset = index.GetOffset(items.Count);
            var present = (uint)offset < (uint)items.Count;
            var expectedValue = present ? items[offset] : default!;
            var expectedAnswer = present || orDefault;

            var c = counter();
            var answered = true;
            T value = default!;
            try
            {
                value = ask(build(c));
            }
            catch (InvalidOperationException)
            {
                answered = false;
            }
            catch (ArgumentOutOfRangeException)
            {
                answered = false;
            }

            var label = $"{shape}.{query}";
            if (answered != expectedAnswer)
                _failures.Add($"{label}: answered {answered}");
            else if (answered && !Same(expectedValue, value))
                _failures.Add($"{label}: {value} where {expectedValue} was expected");
            if (c.Keys > c.Length)
                _failures.Add($"{label}: {c.Keys} keys over {c.Length} elements");
            if (c.Tests > c.Length)
                _failures.Add($"{label}: {c.Tests} tests over {c.Length} elements");
            if (c.Enumerations > 1)
                _failures.Add($"{label}: source enumerated {c.Enumerations} times");
            if (c.SecondEnumerations > 1)
                _failures.Add($"{label}: second enumerated {c.SecondEnumerations} times");
        }

        private static bool Same<T>(T expected, T actual) =>
            expected is int[] expectedChunk && actual is int[] actualChunk
                ? expectedChunk.AsSpan().SequenceEqual(actualChunk)
                : EqualityComparer<T>.Default.Equals(expected, actual);
    }
}
