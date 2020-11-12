using System;
using System.Collections.Generic;
using System.Threading;

namespace Cistern.ValueLinq.Containers
{
    struct ConcatFastEnumerator<T, Finish>
        : IFastEnumerator<T>
        where Finish : INode
    {
        private FastEnumerator<T> _current;
        private Finish _finish;
        int _state;

        public ConcatFastEnumerator(FastEnumerator<T> start, in Finish finish) => (_current, _finish, _state) = (start, finish, 0);

        public void Dispose()
        {
            if (_state < 2)
                _current.Dispose();
        }

        public bool TryGetNext(out T current)
        {
            tryAgain:
            if (_current.TryGetNext(out current))
                return true;

            if (TryGetNextFalse())
                goto tryAgain;

            return false;
        }

        private bool TryGetNextFalse()
        {
            switch (_state)
            {
                case 0:
                    _current.Dispose();
                    _current = Nodes<T>.CreateValueEnumerator(_finish).FastEnumerator;
                    _state = 1;
                    return true;

                case 1:
                    _current.Dispose();
                    _state = 2;
                    return false;

                default:
                    return false;
            }
        }
    }

    struct ConcatFastListEnumerator<T>
        : IFastEnumerator<T>
    {
        private FastEnumerator<T> _current;
        private List<EnumerableNode<T>> _remaining;
        int _idx;

        public ConcatFastListEnumerator(List<EnumerableNode<T>> nodes) => (_current, _remaining, _idx) = (EmptyFastEnumerator<T>.Instance, nodes, 0);

        public void Dispose()
        {
            if (_idx < _remaining.Count)
                _current.Dispose();
        }

        public bool TryGetNext(out T current)
        {
        tryAgain:
            if (_current.TryGetNext(out current))
                return true;

            if (TryGetNextFalse())
                goto tryAgain;

            return false;
        }

        private bool TryGetNextFalse()
        {
            _current.Dispose();

            if (_idx >= _remaining.Count)
            {
                _current = EmptyFastEnumerator<T>.Instance;
                return false;
            }
            else
            {
                _current = Nodes<T>.CreateValueEnumerator(_remaining[_idx++]).FastEnumerator;
                return true;
            }
        }
    }


    public struct ConcatNode<T, Start, Finish>
        : INode<T>
        where Start : INode<T>
        where Finish : INode<T>
    {
        public void GetCountInformation(out CountInformation info) => info = _countInfo;

        private Start _start;
        private Finish _finish;
        CountInformation _countInfo;

        public ConcatNode(in Start start, in Finish finish)
        {
            start.GetCountInformation(out _countInfo);
            finish.GetCountInformation(out var rhs);

            _countInfo.MaximumLength += rhs.MaximumLength;
            _countInfo.ActualLengthIsMaximumLength &= rhs.ActualLengthIsMaximumLength;
            _countInfo.IsImmutable &= rhs.IsImmutable;
            _countInfo.IsStale = !_countInfo.IsImmutable || !rhs.IsImmutable;

#if PROPERLY_RIPPLE_POTENTIAL_SIDE_EFFECTS
            _countInfo.PotentialSideEffects |= rhs.PotentialSideEffects;
#else
            _countInfo.PotentialSideEffects = false;
#endif

            (_start, _finish) = (start, finish);
        }

        List<EnumerableNode<T>> TryCollectNodes()
        {
            (EnumerableNode<T>, EnumerableNode<T>) items;
            if (!(_start is EnumerableNode<T> && _finish is EnumerableNode<T> && _start.CheckForOptimization(new Optimizations.SplitConcat<T>(), out items)))
                return null;

            var heads = new List<EnumerableNode<T>>();
            var tails = new List<EnumerableNode<T>>();

            EnumerableNode<T> head = items.Item1;
            bool headHasValue = true;
            tails.Add(items.Item2);
            while (headHasValue)
            {
                while (Helper.CheckForOptimization(in head, new Optimizations.SplitConcat<T>(), out items))
                {
                    head = items.Item1;
                    tails.Add(items.Item2);
                }

                ConditionallyAdd(heads, head);

                if (tails.Count == 0)
                {
                    headHasValue = false;
                }
                else
                {
                    var lastIdx = tails.Count - 1;
                    head = tails[lastIdx];
                    tails.RemoveAt(lastIdx);
                }
            }

            ConditionallyAdd(heads, (EnumerableNode<T>)(object)_finish);

            return heads;

            static void ConditionallyAdd(List<EnumerableNode<T>> heads, EnumerableNode<T>? node)
            {
                node.Value.GetCountInformation(out var countInfo);
                var actualSize = countInfo.ActualSize;
                if (!actualSize.HasValue || actualSize > 0)
                    heads.Add(node.Value);
            }
        }

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var components = TryCollectNodes();
            if (components != null)
            {
                return components.Count switch
                {
                    0 => EmptyNode.Create<T, Head, Tail, CreationType>(ref nodes),
                    1 => Helper.CreateObjectDescent<EnumerableNode<T>, CreationType, Head, Tail>(components[0], ref nodes),
                    2 => ConcatNode.Create<T, EnumerableNode<T>, EnumerableNode<T>, Head, Tail, CreationType>(components[0], components[1], ref nodes),
                    _ => ConcatNode.Create<T, Head, Tail, CreationType>(components, ref nodes)
                };
            }
            return ConcatNode.Create<T, Start, Finish, Head, Tail, CreationType>(in _start, in _finish, ref nodes);
        }

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __)
            => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                var countInfo = (Optimizations.Count)(object)request;
                result = (TResult)(object)Count(countInfo.IgnorePotentialSideEffects);
                return true;
            }

            // recursive concats, which this is designed to find, should of been defined through IEnumerable<T>, which will mean that
            // they are of type EnumerableNode<T>
            if (typeof(TRequest) == typeof(Optimizations.SplitConcat<T>) && _start is EnumerableNode<T> && _finish is EnumerableNode<T>)
            {
                result = (TResult)(object)(_start, _finish);
                return true;
            }

            result = default;
            return false;
        }

        private readonly int Count(bool ignorePotentialSideEffects)
        {
            checked { return Enumerable.Count<T, Start>(_start, ignorePotentialSideEffects) + Enumerable.Count<T, Finish>(_finish, ignorePotentialSideEffects); }
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
        {
            var components = TryCollectNodes();
            if (components != null)
            {
                return components.Count switch
                {
                    0 => Helper.CreateObjectViaFastEnumerator<EmptyNode<T>, T, TResult, FEnumerator>(new EmptyNode<T>(), in fenum),
                    1 => Helper.CreateObjectViaFastEnumerator<EnumerableNode<T>, T, TResult, FEnumerator>(components[0], in fenum),
                    2 => Helper.CreateObjectViaFastEnumerator<EnumerableNode<T>, T, TResult, ConcatStartFoward<T, EnumerableNode<T>, FEnumerator>>(components[0], new ConcatStartFoward<T, EnumerableNode<T>, FEnumerator>(fenum, components[1])),
                    _ => Helper.CreateObjectViaFastEnumerator<EnumerableNode<T>, T, TResult, ConcatListFoward<T, FEnumerator>>(components[0], new ConcatListFoward<T, FEnumerator>(fenum, components, 1))
                };
            }

            return _start.CreateObjectViaFastEnumerator<TResult, ConcatStartFoward<T, Finish, FEnumerator>>(new ConcatStartFoward<T, Finish, FEnumerator>(fenum, _finish));
        }
    }

    static class ConcatNode
    {
        public static CreationType Create<T, Start, Finish, Head, Tail, CreationType>(in Start start, in Finish finish, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
            where Start : INode
            where Finish : INode
        {
            var startEnumerator = Nodes<T>.CreateValueEnumerator(start);

            var e = new ConcatFastEnumerator<T, Finish>(startEnumerator.FastEnumerator, finish);
            return nodes.CreateObject<CreationType, T, ConcatFastEnumerator<T, Finish>>(ref e);
        }

        internal static CreationType Create<T, Head, Tail, CreationType>(List<EnumerableNode<T>> components, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var e = new ConcatFastListEnumerator<T>(components);
            return nodes.CreateObject<CreationType, T, ConcatFastListEnumerator<T>>(ref e);
        }
    }

    struct ConcatFinishFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;

        public ConcatFinishFoward(in Next prior) => (_next) = (prior);

        public void Dispose() => _next.Dispose();

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input) => _next.ProcessNext(input);
    }

    struct ConcatStartFoward<T, Finish, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
        where Finish : INode<T>
    {
        Next _next;
        Finish _finish;
        bool _disposedOrOntoFinish;

        public ConcatStartFoward(in Next prior, in Finish finish) => (_next, _finish, _disposedOrOntoFinish) = (prior, finish, false);

        public void Dispose()
        {
            if (!_disposedOrOntoFinish)
            {
                _disposedOrOntoFinish = true;
                _next.Dispose();
            }
        }

        public TResult GetResult<TResult>()
        {
            _disposedOrOntoFinish = true;
            return _finish.CreateObjectViaFastEnumerator<TResult, ConcatFinishFoward<T, Next>>(new ConcatFinishFoward<T, Next>(in _next));
        }

        public bool ProcessNext(T input) => _next.ProcessNext(input);
    }

    struct ConcatListFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;
        List<EnumerableNode<T>> _nodes;
        int _idx;

        public ConcatListFoward(in Next prior, List<EnumerableNode<T>> nodes, int idx) => (_next, _nodes, _idx) = (prior, nodes, idx);

        public void Dispose()
        {
            if (_nodes.Count == _idx)
                _next.Dispose();
        }

        public TResult GetResult<TResult>()
        {
            if (_nodes.Count == _idx)
                return _next.GetResult<TResult>();

            return Helper.CreateObjectViaFastEnumerator<EnumerableNode<T>, T, TResult, ConcatListFoward<T, Next>>(_nodes[_idx], new ConcatListFoward<T, Next>(in _next, _nodes, _idx+1));
        }

        public bool ProcessNext(T input) => _next.ProcessNext(input);
    }

}
