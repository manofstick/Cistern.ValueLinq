namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Utils;
using System;

public static partial class TakeWhileIdx
{
    [Operator]
    public partial struct Node<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        [Propagate] internal Func<T, int, bool> _predicate;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }
    }

    partial struct Push<T, Next>
    {
        bool _taking;
        int _idx;

        partial void Start() => _taking = true;

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            // as TakeWhile.Push.ProcessBatch, the index advancing with each call of the predicate
            if (!_taking)
                return false;

            var span = batch.GetSpan();
            if (!Next.AllowBatchesWhen.Takes(batch.Requires))
                return ProcessElements(span);

            var predicate = _predicate;
            while (!span.IsEmpty)
            {
                var chunk = span[..Math.Min(span.Length, StackBuffer<T>.Capacity)];
                var taken = 0;
                while (taken < chunk.Length && predicate(chunk[taken], _idx++))
                    ++taken;

                if (taken > 0)
                {
                    var prefix = new Batch<T>(chunk[..taken], batch.Requires);
                    if (!_next.ProcessBatch(ref prefix))
                        return false;
                }

                if (taken < chunk.Length)
                {
                    _taking = false;
                    return false;
                }

                span = span[chunk.Length..];
            }

            return true;
        }

        // element by element, from a local copy: Docs/BatchFromLocalCopy.md, "The fix"
        private bool ProcessElements(ReadOnlySpan<T> span)
        {
            var next = _next;
            var predicate = _predicate;
            var more = true;
            foreach (var item in span)
            {
                if (!predicate(item, _idx++))
                {
                    _taking = false;
                    more = false;
                    break;
                }

                if (!next.ProcessNext(item))
                {
                    more = false;
                    break;
                }
            }
            _next = next;
            return more;
        }

        public bool ProcessNext(T input)
        {
            if (_taking && _predicate(input, _idx++))
                return _next.ProcessNext(input);

            _taking = false;
            return false;
        }
    }

    public partial struct Pull<T, NodeT, TInEnumerator>
    {
        bool _taking;
        int _idx;

        partial void Start() => _taking = true;

        [AllowNull] private T _current;

        public bool MoveNext()
        {
            if (!_taking)
                return false;

            if (_enumerator.MoveNext())
            {
                var current = _enumerator.Current;
                if (_predicate(current, _idx++))
                {
                    _current = current;
                    return true;
                }
            }

            _taking = false;
            return false;
        }

        public T Current => _current;
    }
}
