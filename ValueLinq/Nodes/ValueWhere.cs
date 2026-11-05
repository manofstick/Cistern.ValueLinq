namespace Cistern.ValueLinq.Nodes;

using System;
using System.Runtime.CompilerServices;

public static partial class ValueWhere
{
    [Operator]
    public partial struct Node<T, NodeT, Predicate>
        : INode<T>
        where NodeT : INode<T>
        where Predicate : IFunc<T, bool>
    {
        internal NodeT _nodeT;
        [Propagate] internal Predicate _predicate;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }
    }

    partial struct Push<T, Next, Predicate>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input)
        {
            if (_predicate.Invoke(input))
                return _next.ProcessNext(input);
            return true;
        }

        // a cold batch only over a downstream that reads every element, as one does that takes cold
        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen =>
            BatchKinds.Container
            | BatchKinds.Forwarder
            | ((Next.AllowBatchesWhen & BatchKinds.Cold) != 0
                ? BatchKinds.ColdSpan
                : BatchKinds.None);

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            for (var i=0; i < span.Length; ++i)
            {
                if (_predicate.Invoke(span[i]))
                    if (!_next.ProcessNext(span[i]))
                        return false;
            }
            return true;
        }
    }

    public partial struct Pull<T, NodeT, TInEnumerator, Predicate>
    {
        [AllowNull] private T _current;

        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                var current = _enumerator.Current;
                if (_predicate.Invoke(current))
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
