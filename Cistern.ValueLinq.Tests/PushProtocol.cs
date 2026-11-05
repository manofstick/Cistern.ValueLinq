using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // The push protocol, seen from a chain pushed into: every source enumerator opened is disposed exactly once, before
    // the chain's GetResult; GetResult is called at most once and only when nothing threw; the chain's Dispose is
    // called exactly once, last. Checked over every source shape and the operators that push or pull other sequences,
    // for a run to the end, a chain that stops early and a chain that throws, element by element and in batches.
    public class PushProtocol
    {
        private sealed class Log
        {
            public readonly List<string> Events = new();

            // an iterator throws from its third MoveNext, a struct source as it is opened
            public bool SourcesThrow;

            public void Add(string e) => Events.Add(e);

            public IEnumerable<int> Source(string name, int count) => new Recorded(this, name, count);

            public StructSource Struct(string name, int count, bool steady = false) => new(this, name, count, steady);
        }

        private sealed class Recorded(Log log, string name, int count) : IEnumerable<int>
        {
            public IEnumerator<int> GetEnumerator()
            {
                log.Add($"open {name}");
                return new Enumerator(log, name, count);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private sealed class Enumerator(Log log, string name, int count) : IEnumerator<int>
            {
                private int _index = -1;

                public int Current => _index;
                object IEnumerator.Current => Current;

                public bool MoveNext()
                {
                    if (log.SourcesThrow && _index == 1)
                        throw new Boom();

                    return ++_index < count;
                }

                public void Reset() => throw new NotSupportedException();

                public void Dispose() => log.Add($"dispose {name}");
            }
        }

        // a steady source does not throw when the others do
        private sealed class StructSource(Log log, string name, int count, bool steady) : IEnumerable<int>
        {
            public Enumerator GetEnumerator()
            {
                if (log.SourcesThrow && !steady)
                    throw new Boom();

                log.Add($"open {name}");
                return new Enumerator(log, name, count);
            }

            IEnumerator<int> IEnumerable<int>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public struct Enumerator(Log log, string name, int count) : IEnumerator<int>
            {
                private int _index = -1;

                public readonly int Current => _index;
                readonly object IEnumerator.Current => Current;

                public bool MoveNext() => ++_index < count;

                public void Reset() => throw new NotSupportedException();

                public readonly void Dispose() => log.Add($"dispose {name}");
            }
        }

        // hands out a Cistern chain's own enumerator, which is pushed rather than pulled
        private sealed class Wrapper(IEnumerable<int> inner) : IEnumerable<int>
        {
            public IEnumerator<int> GetEnumerator() => inner.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class Boom : Exception
        {
        }

        public enum Outcome
        {
            ToTheEnd,
            Stops,
            Throws,
            SourceThrows,
        }

        private sealed class Behaviour(Log log, Outcome outcome)
        {
            private int _seen;

            // false to stop
            public bool Take()
            {
                ++_seen;
                if (outcome == Outcome.Throws && _seen == 3)
                    throw new Boom();

                return outcome != Outcome.Stops || _seen < 2;
            }

            public void Result() => log.Add("result");

            public void Dispose() => log.Add("chain disposed");

            // the push this chain was handed to has started it, and owns it from here
            public bool Started { get; set; }
        }

        private readonly struct Elements(Behaviour behaviour) : IPushEnumerator<int>
        {
            public bool ProcessNext(int input) => behaviour.Take();
            public TPushResult GetResult<TPushResult>()
            {
                behaviour.Result();
                return default!;
            }
            public void Dispose() => behaviour.Dispose();
        }

        private readonly struct Batches(Behaviour behaviour) : IPushEnumerator<int>
        {
            static BatchKinds IPushEnumerator<int>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.Cold;

            bool IPushEnumerator<int>.ProcessBatch(scoped ref Batch<int> batch)
            {
                foreach (var _ in batch.GetSpan())
                {
                    if (!behaviour.Take())
                        return false;
                }

                return true;
            }

            public bool ProcessNext(int input) => behaviour.Take();
            public TPushResult GetResult<TPushResult>()
            {
                behaviour.Result();
                return default!;
            }
            public void Dispose() => behaviour.Dispose();
        }

        // a chain built already, handed in as Downstream hands one: until the push has started it, it is the test's, which
        // disposes it when the push fails first (Push)
        private readonly struct Filler<TPushEnumerator>(TPushEnumerator chain, Behaviour behaviour) : IPushFiller<TPushEnumerator>
        {
            public void Fill(ref TPushEnumerator target) => target = chain;

            public void Started() => behaviour.Started = true;
        }

        private static int Push<TNode>(TNode node, Behaviour behaviour, bool batches)
            where TNode : INode<int>
        {
            try
            {
                return batches
                    ? node.CreateViaPush<int, Batches, Filler<Batches>>(default(ChainOptions), new Filler<Batches>(new Batches(behaviour), behaviour))
                    : node.CreateViaPush<int, Elements, Filler<Elements>>(default(ChainOptions), new Filler<Elements>(new Elements(behaviour), behaviour));
            }
            finally
            {
                if (!behaviour.Started)
                    behaviour.Dispose();
            }
        }

        private static readonly Dictionary<string, Func<Log, INode<int>>> Shapes = new()
        {
            ["array"] = log => new[] { 0, 1, 2, 3, 4 }.OfArray(),
            ["list"] = log => new List<int> { 0, 1, 2, 3, 4 }.OfEnumerable(),
            ["hashSet"] = log => new HashSet<int> { 0, 1, 2, 3, 4 }.OfEnumerable(),
            ["iList"] = log => new Collection<int>(new List<int> { 0, 1, 2, 3, 4 }).OfEnumerable(),
            ["range"] = log => Enumerable.Range(0, 5),
            ["iterator"] = log => log.Source("a", 5).OfEnumerable(),
            ["iteratorWhereSelect"] = log => log.Source("a", 5).OfEnumerable().Where(x => true).Select(x => x),
            ["erasedChain"] = log => ((IEnumerable<int>)log.Source("a", 5).Select(x => x)).OfEnumerable(),
            ["wrappedEnumerator"] = log => new Wrapper(log.Source("a", 5).Select(x => x)).OfEnumerable(),
            ["enumeratorConstraint"] = log => log.Struct("a", 5).OfEnumeratorConstraint<int, StructSource, StructSource.Enumerator>(s => s.GetEnumerator()),
            ["concat"] = log => log.Source("a", 3).Concat(log.Source("b", 3)).OfEnumerable(),
            ["concatOfThree"] = log => log.Source("a", 2).Concat(log.Source("b", 2)).Concat(log.Source("c", 2)).OfEnumerable(),
            ["typedConcat"] = log => log.Source("a", 3).OfEnumerable().Concat(log.Struct("b", 3).OfEnumeratorConstraint<int, StructSource, StructSource.Enumerator>(s => s.GetEnumerator())),
            ["selectMany"] = log => log.Source("outer", 3).OfEnumerable().SelectMany(i => log.Source($"inner{i}", 3)),
            ["zip"] = log => log.Source("a", 5).OfEnumerable().Zip(log.Source("b", 5), (x, y) => x + y),
            ["orderBy"] = log => log.Source("a", 5).OfEnumerable().OrderBy(x => x),
            ["reverse"] = log => log.Source("a", 5).OfEnumerable().Reverse(),
            ["distinct"] = log => log.Source("a", 5).OfEnumerable().Distinct(),
            ["except"] = log => log.Source("a", 5).OfEnumerable().Except(log.Source("b", 1)),
            ["join"] = log => log.Source("a", 5).OfEnumerable().Join(log.Source("b", 5), x => x, y => y, (x, y) => x),
            ["append"] = log => log.Source("a", 5).OfEnumerable().Append(9),
            ["skipTake"] = log => log.Source("a", 6).OfEnumerable().Skip(1).Take(4),
            ["takeLast"] = log => log.Source("a", 6).OfEnumerable().TakeLast(3),
            ["aggregateBy"] = log => log.Source("a", 5).OfEnumerable().AggregateBy(x => x % 3, 0, (s, x) => s + x).Select(p => p.Value),
            ["countBy"] = log => log.Source("a", 5).OfEnumerable().CountBy(x => x % 3).Select(p => p.Value),
            ["groupBy"] = log => log.Source("a", 5).OfEnumerable().GroupBy(x => x % 3, (k, g) => k),
            ["intersect"] = log => log.Source("a", 5).OfEnumerable().Intersect(log.Source("b", 5)),
            ["zipOverOrderBy"] = log => log.Source("a", 5).OfEnumerable().OrderBy(x => x).Zip(log.Source("b", 5), (x, y) => x + y),
            ["prependOverOrderBy"] = log => log.Source("a", 5).OfEnumerable().OrderBy(x => x).Prepend(9),
            // met through IEnumerable<T> as a node that is not a Cistern chain's IPullEnumerable, the chain is handed on to it
            ["autoFusedOrderBy"] = log => log.Source("a", 5).OfEnumerable().OrderBy(x => x).ToEnumerable(EnumerableOptions.Fuse).OfEnumerable(),
            // Except's Start, run as its filler fills it, follows Zip's filler, which has put its open second sequence
            // into the chain already
            ["zipUnderExcept"] = log => log.Source("a", 5).OfEnumerable().Zip(log.Struct("b", 5, steady: true).OfEnumeratorConstraint<int, StructSource, StructSource.Enumerator>(s => s.GetEnumerator()), (x, y) => x + y + 100).Except(log.Source("c", 5)),        };

        // Except initialises the chain (its set, from c); over every kind of push, each of which must initialise the chain
        // it runs before opening its source, or Except finds no set
        private static readonly Dictionary<string, Func<Log, INode<int>>> Initialising = new()
        {
            ["array"] = log => new[] { 0, 1, 2, 3, 4 }.OfArray().Except(log.Source("c", 1)),
            ["list"] = log => new List<int> { 0, 1, 2, 3, 4 }.OfList().Except(log.Source("c", 1)),
            ["listAsEnumerable"] = log => new List<int> { 0, 1, 2, 3, 4 }.OfEnumerable().Except(log.Source("c", 1)),
            ["listReversed"] = log => new List<int> { 0, 1, 2, 3, 4 }.OfList().Reverse().Except(log.Source("c", 1)),
            ["memory"] = log => new ReadOnlyMemory<int>([0, 1, 2, 3, 4]).OfMemory().Except(log.Source("c", 1)),
            ["hashSet"] = log => new HashSet<int> { 0, 1, 2, 3, 4 }.OfEnumerable().Except(log.Source("c", 1)),
            ["iList"] = log => new Collection<int>(new List<int> { 0, 1, 2, 3, 4 }).OfEnumerable().Except(log.Source("c", 1)),
            ["range"] = log => Enumerable.Range(0, 5).Except(log.Source("c", 1)),
            ["sequence"] = log => Enumerable.Sequence(0, 4, 1).Except(log.Source("c", 1)),
            ["repeat"] = log => Enumerable.Repeat(7, 5).Select((x, i) => i).Except(log.Source("c", 1)),
            ["return"] = log => Enumerable.Return(7).Except(log.Source("c", 1)),
            ["empty"] = log => Enumerable.Empty<int>().Except(log.Source("c", 1)),
            ["cast"] = log => ((System.Collections.IEnumerable)new object[] { 0, 1, 2, 3, 4 }).Cast<int>().Except(log.Source("c", 1)),
            ["iterator"] = log => log.Source("a", 5).OfEnumerable().Except(log.Source("c", 1)),
            ["skipped"] = log => log.Source("a", 6).Skip(1).OfEnumerable().Except(log.Source("c", 1)),
            ["erasedChain"] = log => ((IEnumerable<int>)log.Source("a", 5).Select(x => x)).OfEnumerable().Except(log.Source("c", 1)),
            ["wrappedEnumerator"] = log => new Wrapper(log.Source("a", 5).Select(x => x)).OfEnumerable().Except(log.Source("c", 1)),
            ["enumeratorConstraint"] = log => log.Struct("a", 5).OfEnumeratorConstraint<int, StructSource, StructSource.Enumerator>(s => s.GetEnumerator()).Except(log.Source("c", 1)),
            ["concat"] = log => log.Source("a", 3).Concat(log.Source("b", 3).Select(x => x + 10)).OfEnumerable().Except(log.Source("c", 1)),
            ["selectMany"] = log => log.Source("outer", 3).OfEnumerable().SelectMany(i => log.Source($"inner{i}", 3).Select(x => x + 10 * i)).Except(log.Source("c", 1)),
            ["zippedArrays"] = log => new[] { 0, 1, 2, 3, 4 }.OfArray().Zip(new[] { 0, 1, 2, 3, 4 }, (x, y) => x + y).Except(log.Source("c", 1)),
            ["orderBy"] = log => log.Source("a", 5).OfEnumerable().OrderBy(x => x).Except(log.Source("c", 1)),
            ["groupBy"] = log => log.Source("a", 5).OfEnumerable().GroupBy(x => x).Select(g => g.Key).Except(log.Source("c", 1)),
            ["countBy"] = log => log.Source("a", 5).OfEnumerable().CountBy(x => x).Select(p => p.Key).Except(log.Source("c", 1)),
            ["prepend"] = log => log.Source("a", 5).OfEnumerable().Prepend(9).Except(log.Source("c", 1)),
        };

        // shapes of fewer than three elements, whose chain never reaches the element it throws on
        private static readonly HashSet<string> Short = ["initialising return", "initialising empty"];

        public static IEnumerable<object[]> Cases()
        {
            foreach (var shape in Shapes.Keys)
            {
                foreach (var outcome in Enum.GetValues<Outcome>())
                {
                    yield return [shape, outcome, false];
                    yield return [shape, outcome, true];
                }
            }
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void APushKeepsTheProtocol(string shape, Outcome outcome, bool batches)
        {
            var log = new Log { SourcesThrow = outcome == Outcome.SourceThrows };
            var node = Shapes[shape](log);
            var behaviour = new Behaviour(log, outcome);

            void Run() => Push(node, behaviour, batches);

            var throws = outcome is Outcome.Throws && !Short.Contains(shape)
                || outcome is Outcome.SourceThrows && !NoSource.Contains(shape);
            if (throws)
                Assert.Throws<Boom>(Run);
            else
                Run();

            AssertProtocol(log.Events, result: !throws);
        }

        // shapes over collections alone, which have no source to throw
        private static readonly HashSet<string> NoSource = ["array", "list", "hashSet", "iList", "range"];

        // with Except's second sequence (c) of one element, which the throwing sources never reach, the initialising shapes
        // over collections have no source to throw either
        static PushProtocol()
        {
            foreach (var (name, shape) in Initialising)
                Shapes[$"initialising {name}"] = shape;

            foreach (var name in new[] { "array", "list", "listAsEnumerable", "listReversed", "memory", "hashSet", "iList", "range", "sequence", "repeat", "return", "empty", "cast", "zippedArrays" })
                NoSource.Add($"initialising {name}");
        }

        private static int PushRecorded<TNode>(TNode node, Behaviour behaviour)
            where TNode : INode<int>
            => Push(node, behaviour, batches: false);

        private static int Throw() => throw new Boom();

        [Fact]
        public void AForksFirstChainIsDisposedWhenTheSecondAggregationThrowsFirst()
        {
            var log = new Log();
            var behaviour = new Behaviour(log, Outcome.ToTheEnd);

            Assert.Throws<Boom>(() => new[] { 1, 2, 3 }.Fork(x => PushRecorded(x, behaviour), x => Throw()));
            Assert.Equal(["chain disposed"], log.Events);
        }

        [Fact]
        public void AForksFirstChainIsDisposedWhenTheSecondAggregationNeverPushes()
        {
            var log = new Log();
            var behaviour = new Behaviour(log, Outcome.ToTheEnd);

            new[] { 1, 2, 3 }.Fork(x => PushRecorded(x, behaviour), x => 0);
            Assert.Equal(["chain disposed"], log.Events);
        }

        [Fact]
        public void AForksChainsAreDisposedWhenItsPushFailsBeforeStartingThem()
        {
            var log = new Log { SourcesThrow = true };
            var behaviour = new Behaviour(log, Outcome.ToTheEnd);

            Assert.Throws<Boom>(() => log.Source("a", 5).OfEnumerable().OrderBy(x => x).Fork(x => PushRecorded(x, behaviour), x => x.Sum()));
            AssertProtocol(log.Events, result: false);
        }

        [Fact]
        public void AForksFirstChainTakesOnePush()
        {
            var log = new Log();
            var behaviour = new Behaviour(log, Outcome.ToTheEnd);

            Assert.Equal(12, new[] { 1, 2, 3 }.Fork(x => PushRecorded(x, behaviour), x => x.Sum() + x.Sum()).Item2);
            Assert.Equal(["result", "chain disposed"], log.Events);
        }

        // a counted tail is read by a Concat's ToArray and ToList once the head has run to its end, disposal included
        private sealed class Tail(Log log, int[] items) : ICollection<int>
        {
            public int Count
            {
                get
                {
                    log.Add("count tail");
                    return items.Length;
                }
            }

            public bool IsReadOnly => true;

            public void CopyTo(int[] array, int arrayIndex)
            {
                log.Add("copy tail");
                items.CopyTo(array, arrayIndex);
            }

            public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)items).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public bool Contains(int item) => throw new NotSupportedException();
            public void Add(int item) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();
            public bool Remove(int item) => throw new NotSupportedException();
        }

        private static readonly Dictionary<string, Func<Log, Tail, IEnumerable<int>>> Heads = new()
        {
            ["iterator"] = (log, tail) => log.Source("a", 3).Concat(tail),
            ["twoIterators"] = (log, tail) => log.Source("a", 3).Concat(log.Source("b", 3)).Concat(tail),
            ["erasedChain"] = (log, tail) => log.Source("a", 3).Select(x => x).Concat(tail),
            ["typed"] = (log, tail) => log.Source("a", 3).OfEnumerable().Concat(tail.OfEnumerable()),
            ["enumeratorConstraint"] = (log, tail) => log.Struct("a", 3).OfEnumeratorConstraint<int, StructSource, StructSource.Enumerator>(s => s.GetEnumerator()).Concat(tail.OfEnumerable()),
        };

        public static IEnumerable<object[]> HeadCases()
        {
            foreach (var head in Heads.Keys)
            {
                yield return [head, false];
                yield return [head, true];
            }
        }

        [Theory]
        [MemberData(nameof(HeadCases))]
        public void AConcatsTailIsReadAfterItsHeadIsDisposed(string head, bool toList)
        {
            var log = new Log();
            var query = Heads[head](log, new Tail(log, [7, 8]));
            var result = toList ? query.ToList().ToArray() : query.ToArray();

            var trace = string.Join(", ", log.Events);
            Assert.Equal(7, result[^2]);
            Assert.Equal(1, log.Events.FindAll(e => e == "copy tail").Count);

            var copied = log.Events.IndexOf("copy tail");
            var lastCount = log.Events.LastIndexOf("count tail");
            var lastDispose = log.Events.FindLastIndex(e => e.StartsWith("dispose "));
            Assert.True(lastDispose < lastCount && lastCount < copied, $"the head is disposed before the tail is counted and copied: {trace}");
        }

        private static void AssertProtocol(List<string> events, bool result)
        {
            var trace = string.Join(", ", events);

            Assert.True(events.Count > 0 && events[^1] == "chain disposed", $"the chain is disposed last: {trace}");
            Assert.Equal(1, events.FindAll(e => e == "chain disposed").Count);

            var resultAt = events.IndexOf("result");
            Assert.True(result ? resultAt >= 0 : resultAt < 0, $"the result is taken only when nothing threw: {trace}");
            Assert.Equal(result ? 1 : 0, events.FindAll(e => e == "result").Count);

            var opened = new Dictionary<string, int>();
            var disposed = new Dictionary<string, int>();
            for (var i = 0; i < events.Count; ++i)
            {
                var e = events[i];
                if (e.StartsWith("open "))
                    opened[e[5..]] = opened.GetValueOrDefault(e[5..]) + 1;
                else if (e.StartsWith("dispose "))
                {
                    disposed[e[8..]] = disposed.GetValueOrDefault(e[8..]) + 1;
                    Assert.True(resultAt < 0 || i < resultAt, $"a source is disposed before the result is taken: {trace}");
                }
            }

            Assert.Equal(opened, disposed);
        }
    }
}
