namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// Where under the caller's promise that the predicate is pure: no side effects, and a result that depends only on its
// argument. Nothing then pins the order it runs in, so a from-end index is found scanning backwards from the end, and
// a reversal is the source's with the filter over it.
public static partial class Filter
{
    [Operator]
    public partial struct Node<T, NodeT>
        : INode<T>
        , Optimizations.IOptimizationToArray<T>
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationFilteredSpanSource<T>
        , Optimizations.IOptimizationMaterialiseList<T>
        , Optimizations.IOptimizationReverse<T>
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        [Propagate] internal Func<T, bool> _predicate;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out T[] response)
        {
            if (Optimizations.AsSpanSource.TryGet<T, NodeT>(ref _nodeT, out var source))
            {
                response = Where.ToArray(source.GetSpan(), _predicate, nullIfUnchanged: false)!;
                return true;
            }

            response = default;
            return false;
        }

        private bool MaterialiseList(in Optimizations.MaterialiseList request, [MaybeNullWhen(false)] out List<T> response)
        {
            if (Optimizations.AsSpanSource.TryGet<T, NodeT>(ref _nodeT, out var source))
            {
                response = Where.ToList(source.GetSpan(), _predicate);
                return true;
            }

            response = default;
            return false;
        }

        // over a span, any index from either end is a scan from that end counting matches; otherwise a from-end index
        // is one from the start of the reversed source, where the source reverses
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            var index = request.Index;
            if (Optimizations.AsSpanSource.TryGet<T, NodeT>(ref _nodeT, out var source))
            {
                response = Find(source.GetSpan(), index);
                return true;
            }

            if (!index.IsFromEnd)
            {
                response = default;
                return false;
            }

            if (index.Value == 0)
            {
                response = (false, default!);
                return true;
            }

            if (!Optimizations.Reverse.Try<T, NodeT>(ref _nodeT, out var reversedSource))
            {
                response = default;
                return false;
            }

            var reversed = new Node<T, NodeContainerNode<T>>(new NodeContainerNode<T>(in reversedSource), _predicate);
            response = ((INode<T>)reversed).CreateViaPush<(bool, T), Aggregation.FoundElementAt<T>, Construct<Aggregation.FoundElementAt<T>, Aggregation.FoundElementAt<T>.Args>>(default(ChainOptions), new Construct<Aggregation.FoundElementAt<T>, Aggregation.FoundElementAt<T>.Args>(new Aggregation.FoundElementAt<T>.Args(new Index(index.Value - 1))));
            return true;
        }

        private readonly (bool, T) Find(ReadOnlySpan<T> span, Index index)
        {
            var predicate = _predicate;
            var remaining = index.IsFromEnd ? index.Value - 1 : index.Value;
            if (remaining < 0)
                return (false, default!);

            if (index.IsFromEnd)
            {
                for (var i = span.Length - 1; i >= 0; --i)
                {
                    if (predicate(span[i]) && remaining-- == 0)
                        return (true, span[i]);
                }
            }
            else
            {
                foreach (var item in span)
                {
                    if (predicate(item) && remaining-- == 0)
                        return (true, item);
                }
            }

            return (false, default!);
        }

        private bool FilteredSpanSource(in Optimizations.FilteredSpanSource request, out (Optimizations.SpanSource<T> Source, Func<T, bool> Predicate) response)
        {
            if (!Optimizations.AsSpanSource.TryGet<T, NodeT>(ref _nodeT, out var source))
            {
                response = default;
                return false;
            }

            response = (source, _predicate);
            return true;
        }

        private bool Reverse(in Optimizations.Reverse request, out NodeContainer<T> response)
        {
            if (!Optimizations.Reverse.Try<T, NodeT>(ref _nodeT, out var reversedSource))
            {
                response = default;
                return false;
            }

            Unsafe.SkipInit(out response);
            response.SetNode(new Node<T, NodeContainerNode<T>>(new NodeContainerNode<T>(in reversedSource), _predicate));
            return true;
        }

        // pushed through Where's forwarder, whose batch paths it shares
        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler) =>
            _nodeT.CreateViaPush<TPushResult, Where.Push<T, TPushEnumerator>, Where.Filler<T, TPushEnumerator, TPushFiller>>(in options, new Where.Filler<T, TPushEnumerator, TPushFiller>(_predicate, in filler));
    }

    public partial struct Pull<T, NodeT, TInEnumerator>
    {
        [AllowNull] private T _current;

        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                var current = _enumerator.Current;
                if (_predicate(current))
                {
                    _current = current;
                    return true;
                }
            }
            return false;
        }

        public T Current => _current;
    }
}
