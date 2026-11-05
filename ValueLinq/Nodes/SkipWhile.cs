namespace Cistern.ValueLinq.Nodes;

using System;

public static partial class SkipWhile
{
    [Operator]
    public partial struct Node<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        [Propagate] internal Func<T, bool> _predicate;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }
    }

    partial struct Push<T, Next>
    {
        bool _skipping;

        partial void Start() => _skipping = true;

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            // The predicate is never called again once an element fails it, so the rest of the span goes downstream as
            // it stands, with its own origin, and the predicate's calls and the downstream's keep the element path's order.
            var span = batch.GetSpan();
            if (_skipping)
            {
                var predicate = _predicate;
                var skipped = 0;
                while (skipped < span.Length && predicate(span[skipped]))
                    ++skipped;

                if (skipped == span.Length)
                    return true;

                _skipping = false;
                span = span[skipped..];
            }

            if (!Next.AllowBatchesWhen.Takes(batch.Requires))
                return ProcessElements(span);

            var rest = new Batch<T>(span, batch.Requires);
            return _next.ProcessBatch(ref rest);
        }

        // element by element, from a local copy: Docs/BatchFromLocalCopy.md, "The fix"
        private bool ProcessElements(ReadOnlySpan<T> span)
        {
            var next = _next;
            var more = true;
            foreach (var item in span)
            {
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
            if (_skipping)
            {
                if (_predicate(input))
                    return true;
                _skipping = false;
            }
            return _next.ProcessNext(input);
        }
    }

    public partial struct Pull<T, NodeT, TInEnumerator>
    {
        bool _skipping;

        partial void Start() => _skipping = true;

        // the element that ended the skipping has had its Current read by the predicate, so it is handed out from here
        [AllowNull] private T _first;
        bool _holdingFirst;

        public bool MoveNext()
        {
            while(_skipping)
            {
                if (!_enumerator.MoveNext())
                    return false;

                var current = _enumerator.Current;
                if (!_predicate(current))
                {
                    _skipping = false;
                    _first = current;
                    _holdingFirst = true;
                    return true;
                }
            }

            _holdingFirst = false;
            return _enumerator.MoveNext();
        }

        public T Current => _holdingFirst ? _first : _enumerator.Current;
    }
}
