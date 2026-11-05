using System;
using System.Collections.Generic;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Appends and prepends onto an AppendPrepend.Node extend it, holding four inline and the rest in an ImmutableList;
    // a node extended twice from one point gives two independent sequences, push and pull agree, a downstream that
    // halts gets no appends, and an index is answered from whichever end or the source holds it.
    public class AppendPrependChains
    {
        private static readonly int[] Data = [10, 11, 12];

        private static IEnumerable<int> Iterate(int[] values)
        {
            foreach (var item in values)
                yield return item;
        }

        private sealed class NeverEnumerated : IEnumerable<int>
        {
            public IEnumerator<int> GetEnumerator() => throw new InvalidOperationException();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private static List<int> Pull(IEnumerable<int> source)
        {
            var items = new List<int>();
            foreach (var item in source)
                items.Add(item);
            return items;
        }

        [Fact]
        public void Chain_StaysOneNode()
        {
            ValueEnumerable<int, Nodes.AppendPrepend.Node<int, SystemArray.Node<int>>, Nodes.AppendPrepend.Pull<int, SystemArray.Node<int>, SystemArray.Pull<int>>> chain = Data.Append(13).Prepend(9).Append(14).Prepend(8).Append(15).Prepend(7);

            Assert.Equal([7, 8, 9, 10, 11, 12, 13, 14, 15], chain.ToArray());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(12)]
        public void InterleavedCalls_ComeOutPrependsLatestFirstThenAppendsInOrder(int calls)
        {
            IEnumerable<int> viaInterface = Iterate(Data);
            var expectedPrepends = new List<int>();
            var expectedAppends = new List<int>();
            for (var i = 0; i < calls; ++i)
            {
                if (i % 3 == 1)
                {
                    viaInterface = viaInterface.Prepend(-i);
                    expectedPrepends.Insert(0, -i);
                }
                else
                {
                    viaInterface = viaInterface.Append(100 + i);
                    expectedAppends.Add(100 + i);
                }
            }

            var expected = new List<int>(expectedPrepends);
            expected.AddRange(Data);
            expected.AddRange(expectedAppends);

            Assert.Equal(expected, viaInterface.ToArray());
            Assert.Equal(expected, Pull(viaInterface));
            Assert.Equal(expected.Count, viaInterface.Count());
        }

        [Fact]
        public void ExtendedTwiceFromOnePoint_GivesIndependentSequences()
        {
            var three = Data.Append(1).Append(2).Append(3);
            var fullA = three.Append(4);
            var fullB = three.Prepend(5);
            var spilledA = fullA.Append(6);
            var spilledB = fullA.Prepend(7);
            var spilledC = spilledA.Append(8);

            Assert.Equal([10, 11, 12, 1, 2, 3], three.ToArray());
            Assert.Equal([10, 11, 12, 1, 2, 3, 4], fullA.ToArray());
            Assert.Equal([5, 10, 11, 12, 1, 2, 3], fullB.ToArray());
            Assert.Equal([10, 11, 12, 1, 2, 3, 4, 6], spilledA.ToArray());
            Assert.Equal([7, 10, 11, 12, 1, 2, 3, 4], spilledB.ToArray());
            Assert.Equal([10, 11, 12, 1, 2, 3, 4, 6, 8], spilledC.ToArray());
            Assert.Equal([10, 11, 12, 1, 2, 3, 4, 6], Pull(spilledA));
        }

        [Fact]
        public void PrependsThatSatisfyTheDownstream_NeverStartTheSource()
        {
            Assert.Equal(1, new NeverEnumerated().Prepend(2).Prepend(1).First());
        }

        [Fact]
        public void HaltedDownstream_GetsNoAppends()
        {
            var seen = new List<int>();

            var first = Data.Append(13).Append(14).Select(x => { seen.Add(x); return x; }).First();

            Assert.Equal(10, first);
            Assert.Equal([10], seen);
        }

        [Fact]
        public void ElementAt_IsAnsweredFromEitherEndOrTheSource()
        {
            var inline = Data.Prepend(9).Append(13);                                  // 9 10 11 12 13
            var spilled = Data.Prepend(8).Prepend(7).Append(13).Append(14).Append(15); // 7 8 10 11 12 13 14 15

            Assert.Equal([9, 10, 11, 12, 13], new[] { 0, 1, 2, 3, 4 }.Select(i => inline.ElementAt(i)).ToArray());
            Assert.Equal([13, 12, 11, 10, 9], new[] { 1, 2, 3, 4, 5 }.Select(i => inline.ElementAt(^i)).ToArray());
            Assert.Equal([7, 8, 10, 11, 12, 13, 14, 15], new[] { 0, 1, 2, 3, 4, 5, 6, 7 }.Select(i => spilled.ElementAt(i)).ToArray());
            Assert.Equal([15, 14, 13, 12, 11, 10, 8, 7], new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.Select(i => spilled.ElementAt(^i)).ToArray());
            Assert.Throws<ArgumentOutOfRangeException>(() => spilled.ElementAt(8));
            Assert.Throws<ArgumentOutOfRangeException>(() => spilled.ElementAt(^9));
            Assert.Equal(0, inline.ElementAtOrDefault(^6));
        }

        [Fact]
        public void SkipAndTake_AtEveryCount()
        {
            int[] inlineExpected = [9, 10, 11, 12, 13];
            int[] spilledExpected = [7, 8, 10, 11, 12, 13, 14, 15];

            for (var n = -1; n <= spilledExpected.Length + 1; ++n)
            {
                var inline = Data.Prepend(9).Append(13);
                var spilled = Data.Prepend(8).Prepend(7).Append(13).Append(14).Append(15);
                var unknownLength = Iterate(Data).Prepend(9).Append(13);

                Assert.Equal(inlineExpected[Math.Clamp(n, 0, inlineExpected.Length)..], inline.Skip(n).ToArray());
                Assert.Equal(spilledExpected[Math.Clamp(n, 0, spilledExpected.Length)..], spilled.Skip(n).ToArray());
                Assert.Equal(inlineExpected[Math.Clamp(n, 0, inlineExpected.Length)..], unknownLength.Skip(n).ToArray());
                Assert.Equal(inlineExpected[..Math.Clamp(n, 0, inlineExpected.Length)], inline.Take(n).ToArray());
                Assert.Equal(spilledExpected[..Math.Clamp(n, 0, spilledExpected.Length)], spilled.Take(n).ToArray());
                Assert.Equal(inlineExpected[..Math.Clamp(n, 0, inlineExpected.Length)], unknownLength.Take(n).ToArray());

                Assert.Equal(Math.Max(0, spilledExpected.Length - Math.Max(n, 0)), spilled.Skip(n).Count());
                Assert.Equal(Pull(spilled.Skip(n)), spilled.Skip(n).ToArray());
                Assert.Equal(Pull(spilled.Skip(n).Take(3)), spilled.Skip(n).Take(3).ToArray());
            }
        }

        [Fact]
        public void SkipPastASourceWithSideEffects_StillRunsIt()
        {
            var seen = new List<int>();

            var skipped = Data.Select(x => { seen.Add(x); return x; }).Prepend(9).Append(13).Skip(4).ToArray();

            Assert.Equal([13], skipped);
            Assert.Equal(Data, seen);
        }

        [Fact]
        public void Reverse_OverEachKindOfSource()
        {
            int[] inlineExpected = [13, 12, 11, 10, 9];
            int[] spilledExpected = [15, 14, 13, 12, 11, 10, 8, 7];

            var overArray = Data.Prepend(9).Append(13).Reverse();
            var overList = new List<int>(Data).Prepend(9).Append(13).Reverse();
            var overIterator = Iterate(Data).Prepend(9).Append(13).Reverse();
            var spilled = Data.Prepend(8).Prepend(7).Append(13).Append(14).Append(15).Reverse();

            Assert.Equal(inlineExpected, overArray.ToArray());
            Assert.Equal(inlineExpected, overList.ToArray());
            Assert.Equal(inlineExpected, overIterator.ToArray());
            Assert.Equal(spilledExpected, spilled.ToArray());
            Assert.Equal(inlineExpected, Pull(overArray));
            Assert.Equal(spilledExpected, Pull(spilled));
            Assert.Equal(55, overArray.Sum());
            Assert.Equal(90, spilled.Sum());
            Assert.Equal(9, Data.Prepend(9).Append(13).Last(x => x < 10));
            Assert.Equal(13, Data.Prepend(9).Append(13).Last(x => x > 12));
            Assert.Equal(new List<int>(inlineExpected), overArray.ToList());
        }

        [Fact]
        public void Pull_OverArraySources()
        {
            Assert.Equal([9, 10, 11, 12, 13], Pull(Data.Prepend(9).Append(13)));
            Assert.Equal([10, 11, 12, 13], Pull(Data.Append(13)));
            Assert.Equal([9, 10, 11, 12], Pull(Data.Prepend(9)));
            Assert.Equal([9, 13], Pull(Array.Empty<int>().Prepend(9).Append(13)));
            Assert.Equal([7, 8, 10, 11, 12, 13, 14, 15], Pull(Data.Prepend(8).Prepend(7).Append(13).Append(14).Append(15)));
            Assert.Equal([9, 11, 12, 13], Pull(Data.Skip(1).Prepend(9).Append(13)));
            Assert.Equal([9, 10, 11, 13], Pull(Data.Take(2).Prepend(9).Append(13)));
            Assert.Equal([9, 13], Pull(Data.Skip(3).Prepend(9).Append(13)));
        }

        [Fact]
        public void Push_InlineAndSpilledEnds()
        {
            Assert.Equal(55, Data.Prepend(9).Append(13).Sum());
            Assert.Equal(22, Array.Empty<int>().Prepend(9).Append(13).Sum());
            Assert.Equal(90, Data.Prepend(8).Prepend(7).Append(13).Append(14).Append(15).Sum());
            Assert.Equal(8, Data.Prepend(8).Prepend(7).Append(13).Select(x => x).ElementAt(1));
            Assert.Equal(14, Data.Prepend(8).Prepend(7).Append(13).Append(14).Append(15).Select(x => x).ElementAt(6));
        }

        [Fact]
        public void ElementAt_OverASourceWithNoKnownLength()
        {
            var chain = Iterate(Data).Prepend(9).Append(13); // 9 10 11 12 13

            Assert.Equal(9, chain.ElementAt(0));
            Assert.Equal(11, chain.ElementAt(2));
            Assert.Equal(13, chain.ElementAt(4));
            Assert.Equal(13, chain.ElementAt(^1));
            Assert.Equal(12, chain.ElementAt(^2));
            Assert.Equal(9, chain.ElementAt(^5));
            Assert.Throws<ArgumentOutOfRangeException>(() => chain.ElementAt(5));
            Assert.Throws<ArgumentOutOfRangeException>(() => chain.ElementAt(^6));
        }
    }
}
