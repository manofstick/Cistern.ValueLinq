using Cistern.ValueLinq.Nodes;
using System;

namespace Cistern.ValueLinq.Optimizations
{
    [PushOptimization]
    internal interface IOptimizationSkipTake<T>
    {
        bool SkipTake(in SkipTake request, out NodeContainer<T> response);
    }

    /// <summary>
    /// A slice of a sequence: the first Skip elements dropped, then at most Take of the rest, with long.MaxValue standing
    /// for the rest of them. Answered with an INode&lt;T&gt; holding the slice, from which CreateViaPush can be called on an
    /// aggregating object, or the empty container when nothing is left. A Skip node and a Take node each answer it by
    /// composing their own slice with the one asked (Compose) and putting that to their source (Answer).
    /// </summary>
    readonly struct SkipTake
    {
        // this slice, then skip more of what it yields and take at most thenTake of that; a skip is a long, as an
        // unbounded source is longer than int.MaxValue, and is held at long.MaxValue past it
        public static void Compose(long skip, long take, long thenSkip, long thenTake, out long skipOut, out long takeOut)
        {
            thenSkip = Math.Max(0, thenSkip);
            thenTake = Math.Max(0, thenTake);
            skipOut = skip > long.MaxValue - thenSkip ? long.MaxValue : skip + thenSkip;
            takeOut = take == long.MaxValue ? thenTake : Math.Min(Math.Max(0, take - thenSkip), thenTake);
        }

        // the slice of a source: answered by the source where it can be, or as the empty container when nothing is
        // left past a length that is not stale. Otherwise it is not answered: the asker stacks its own forwarder or pull, as an answer built from nodes
        // over the source would be the asker's own shape, which runs it and asks again without end.
        public static bool Answer<T, Node>(ref Node source, long skip, long take, ref NodeContainer<T> container)
            where Node : INode<T>
        {
            if (take == 0)
            {
                container.SetEmpty();
                return true;
            }

            if (Try<T, Node>(ref source, skip, take, out container))
                return true;

            source.GetCountInformation(out var info);
            if (!info.IsStale
                && skip > info.MaximumLength)
            {
                container.SetEmpty();
                return true;
            }

            return false;
        }

        private SkipTake(long skip, long take)
        {
            Skip = Math.Max(0, skip);
            Take = Math.Max(0, take);
        }

        public long Skip { get; }
        public long Take { get; }

        public bool TakesAll => Take == long.MaxValue;

        /// <summary>
        /// The slice of a sequence of the given length: where it starts and how many elements it holds.
        /// </summary>
        public void Of(int length, out int start, out int count)
        {
            start = (int)Math.Min(Skip, length);
            count = (int)Math.Min(Take, length - start);
        }

        public static bool Try<T, Node>(ref Node node, long skip, long take, out NodeContainer<T> container)
            where Node : INode<T>
            => node.TryPushOptimization(new SkipTake(skip, take), out container);
    }
}
