using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct ConcatPullEnumerator<T, Finish>
        : IPullEnumerator<T>
        where Finish : INode
    {
        private PullEnumerator<T> _current;
        private Finish _finish;
        int _state;

        public ConcatPullEnumerator(PullEnumerator<T> start, in Finish finish) => (_current, _finish, _state) = (start, finish, 0);

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
                    _current = Nodes<T>.CreatePullEnumerator(_finish);
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

    struct ConcatListPullEnumerator<T>
        : IPullEnumerator<T>
    {
        private PullEnumerator<T> _current;
        private List<EnumerableNode<T>> _remaining;
        int _idx;

        public ConcatListPullEnumerator(List<EnumerableNode<T>> nodes) => (_current, _remaining, _idx) = (InstanceOfEmptyPullEnumerator<T>.Instance, nodes, 0);

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
                _current = InstanceOfEmptyPullEnumerator<T>.Instance;
                return false;
            }
            else
            {
                _current = Nodes<T>.CreatePullEnumerator(_remaining[_idx++]);
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
            _countInfo.LengthIsImmutable &= rhs.LengthIsImmutable;
            _countInfo.IsStale = !_countInfo.LengthIsImmutable || !rhs.LengthIsImmutable;
            _countInfo.Depth = max(_countInfo.Depth, rhs.Depth) + 1;

#if PROPERLY_RIPPLE_POTENTIAL_SIDE_EFFECTS
            _countInfo.PotentialSideEffects |= rhs.PotentialSideEffects;
#else
            _countInfo.PotentialSideEffects = false;
#endif

            (_start, _finish) = (start, finish);

            static int? max(int? depth1, int? depth2) =>
                depth1.HasValue && depth2.HasValue
                    ? Math.Max(depth1.Value, depth2.Value)
                    : default;
        }


        List<EnumerableNode<T>> TryCollectNodes()
        {
            (EnumerableNode<T>, EnumerableNode<T>) items;
            if (!(_start is EnumerableNode<T> && _finish is EnumerableNode<T> && Optimizations.SplitConcat.TrySplit<T, Start>(ref _start, out items)))
                return null;

            var heads = new List<EnumerableNode<T>>(_countInfo.Depth ?? byte.MaxValue);
            var tails = new List<EnumerableNode<T>>(_countInfo.Depth ?? byte.MaxValue);

            EnumerableNode<T> head = items.Item1;
            bool headHasValue = true;
            tails.Add(items.Item2);
            while (headHasValue)
            {
                while (Optimizations.SplitConcat.TrySplit<T, EnumerableNode<T>>(ref head, out items))
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

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
        {
            var components = TryCollectNodes();
            if (components != null)
            {
                return components.Count switch
                {
                    0 => EmptyNode.Create<T, TNodes, CreationType>(ref nodes),
                    1 => Helper.CreateViaPullDescend<EnumerableNode<T>, CreationType, TNodes>(components[0], ref nodes),
                    2 => ConcatNode.Create<T, EnumerableNode<T>, EnumerableNode<T>, TNodes, CreationType>(components[0], components[1], ref nodes),
                    _ => ConcatNode.Create<T, TNodes, CreationType>(components, ref nodes)
                };
            }
            return ConcatNode.Create<T, Start, Finish, TNodes, CreationType>(in _start, in _finish, ref nodes);
        }

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __)
            => throw new InvalidOperationException();

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                var countInfo = (Optimizations.Count)(object)request;
                result = (TResult)(object)Count(countInfo.IgnorePotentialSideEffects);
                return true;
            }

            // recursive concats, which this is designed to find, should of been defined through IEnumerable<T>, which will mean that
            // they are of type EnumerableNode<T>
            if (typeof(TRequest) == typeof(Optimizations.SplitConcat)
                && _start is EnumerableNode<T>
                && _finish is EnumerableNode<T>)
            {
                result = (TResult)(object)(_start, _finish);
                return true;
            }

            result = default;
            return false;
        }

        private int Count(bool ignorePotentialSideEffects)
        {
            checked
            {
                var components = TryCollectNodes();
                if (components == null)
                    return NodeImpl.Count<T, Start>(_start, ignorePotentialSideEffects) + NodeImpl.Count<T, Finish>(_finish, ignorePotentialSideEffects);

                var count = 0;
                foreach (var item in components)
                    count += NodeImpl.Count<T, EnumerableNode<T>>(item, ignorePotentialSideEffects);

                return count;
            }
        }

        TResult INode<T>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
        {
            var components = TryCollectNodes();
            if (components != null)
            {
                return components.Count switch
                {
                    0 => Helper.CreateViaPush<EmptyNode<T>, T, TResult, TPushEnumerator>(new EmptyNode<T>(), in fenum),
                    1 => Helper.CreateViaPush<EnumerableNode<T>, T, TResult, TPushEnumerator>(components[0], in fenum),
                    2 => Helper.CreateViaPush<EnumerableNode<T>, T, TResult, ConcatStartFoward<T, EnumerableNode<T>, TPushEnumerator>>(components[0], new ConcatStartFoward<T, EnumerableNode<T>, TPushEnumerator>(fenum, components[1])),
                    _ => Helper.CreateViaPush<EnumerableNode<T>, T, TResult, ConcatListFoward<T, TPushEnumerator>>(components[0], new ConcatListFoward<T, TPushEnumerator>(fenum, components))
                };
            }

            return _start.CreateViaPush<TResult, ConcatStartFoward<T, Finish, TPushEnumerator>>(new ConcatStartFoward<T, Finish, TPushEnumerator>(fenum, _finish));
        }
    }

    static class ConcatNode
    {
        public static CreationType Create<T, Start, Finish, TNodes, CreationType>(in Start start, in Finish finish, ref TNodes nodes)
            where TNodes : INodes
            where Start : INode
            where Finish : INode
        {
            var startEnumerator = Nodes<T>.CreatePullEnumerator(start);

            var e = new ConcatPullEnumerator<T, Finish>(startEnumerator, finish);
            return nodes.CreateObject<CreationType, T, ConcatPullEnumerator<T, Finish>>(ref e);
        }

        internal static CreationType Create<T, TNodes, CreationType>(List<EnumerableNode<T>> components, ref TNodes nodes)
            where TNodes : INodes
        {
            var e = new ConcatListPullEnumerator<T>(components);
            return nodes.CreateObject<CreationType, T, ConcatListPullEnumerator<T>>(ref e);
        }
    }

    struct ConcatFinishFoward<T, Next>
        : IPushEnumerator<T>
        where Next : IPushEnumerator<T>
    {
        Next _next;

        public ConcatFinishFoward(in Next prior) => (_next) = (prior);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request)
            => _next.TryProcessBatch<TObject, TRequest>(obj, in request);

        public void Dispose() => _next.Dispose();

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input) => _next.ProcessNext(input);
    }

    struct ConcatStartFoward<T, Finish, Next>
        : IPushEnumerator<T>
        where Next : IPushEnumerator<T>
        where Finish : INode<T>
    {
        Next _next;
        Finish _finish;
        bool _disposedOrOntoFinish;

        public ConcatStartFoward(in Next prior, in Finish finish) => (_next, _finish, _disposedOrOntoFinish) = (prior, finish, false);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) =>
            _next.TryProcessBatch(obj, in request);

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
            return _finish.CreateViaPush<TResult, ConcatFinishFoward<T, Next>>(new ConcatFinishFoward<T, Next>(in _next));
        }

        public bool ProcessNext(T input) => _next.ProcessNext(input);
    }

    sealed class ConcatCommonNext<T, Next>
        where Next : IPushEnumerator<T>
    {
        private Next _next;

        public ConcatCommonNext(in Next next) => _next = next;

        public bool ProcessNext(T input) => _next.ProcessNext(input);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => _next.TryProcessBatch(obj, in request);
        public void Dispose() { }
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();
    }

    struct ConcatNextForward<T, Next>
        : IPushEnumerator<T>
        where Next : IPushEnumerator<T>
    {
        ConcatCommonNext<T, Next> _next;
        private bool _processNext;

        public ConcatNextForward(ConcatCommonNext<T, Next> next) => (_next, _processNext) = (next, true);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request)
        {
            var result = _next.TryProcessBatch(obj, in request);
            if (result == BatchProcessResult.SuccessAndHalt)
                _processNext = false;
            return result;
        }

        public void Dispose() { }
        TResult IPushEnumerator<T>.GetResult<TResult>() => (TResult)(object)_processNext;

        bool IPushEnumerator<T>.ProcessNext(T input) => _processNext = _next.ProcessNext(input);
    }

    struct ConcatListFoward<T, Next>
        : IPushEnumerator<T>
        where Next : IPushEnumerator<T>
    {
        List<EnumerableNode<T>> _nodes;
        ConcatCommonNext<T, Next> _common;

        public ConcatListFoward(in Next prior, List<EnumerableNode<T>> nodes) => (_nodes, _common) = (nodes, _common = new ConcatCommonNext<T, Next>(prior));

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request)
            => _common.TryProcessBatch(obj, in request);

        public void Dispose() => _common.Dispose();

        public TResult GetResult<TResult>()
        {
            for (var i = 1; i < _nodes.Count; ++i)
            {
                if (!Helper.CreateViaPush<EnumerableNode<T>, T, bool, ConcatNextForward<T, Next>>(_nodes[i], new ConcatNextForward<T, Next>(_common)))
                    break;
            }
            return _common.GetResult<TResult>();
        }

        public bool ProcessNext(T input) => _common.ProcessNext(input);
    }

}
