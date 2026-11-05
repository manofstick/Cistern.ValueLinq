using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Aggregation
{
    // a Fork is only ever pushed: the chains applied to it are aggregations
    [Puller]
    public partial struct ForkEnumerator<T>
    {
        private static ForkEnumerator<T> Create(in Fork<T> node)
            => throw new InvalidOperationException("Unexpected usage of fork");

        public bool MoveNext() => false;
        public T Current => default!;
    }

    public abstract class Fork<T>
        : INode<T>
    {
        internal abstract TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            where TPushEnumerator : IPushEnumerator<T>
            where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct;
        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);
        public abstract void GetCountInformation(out CountInformation info);

        public abstract bool TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
            where TRequest : notnull;

        [DoesNotReturn]
        protected void InvalidOperation() => throw new InvalidOperationException("Unexpected usage of fork");
    }

    public class Fork<T, Inner>
        : Fork<T>
        where Inner : INode<T>
    {
        protected Inner inner;

        internal Fork(in Inner inner) => this.inner = inner;

        public override void GetCountInformation(out CountInformation info) => inner.GetCountInformation(out info);

        internal override TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => inner.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);
        public override bool TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
            => inner.TryPushOptimization<TRequest, TRequestResult>(in request, out result);
    }

    sealed class Fork<T, U, V, Inner>
        : Fork<T, Inner>
        where Inner : INode<T>
    {
        bool _completeSucessfully;
        Func<ValueEnumerable<T, Fork<T>, ForkEnumerator<T>>, V> _getSecondValue;
        [AllowNull] private V secondValue;

        public V SecondValue
        {
            get
            {
                if (!_completeSucessfully)
                    InvalidOperation();

                return secondValue;
            }
        }

        public Fork(in Inner inner, Func<ValueEnumerable<T, Fork<T>, ForkEnumerator<T>>, V> getSecondValue) : base(in inner)
        {
            _getSecondValue = getSecondValue;
            _completeSucessfully = false;
        }

        // once complete (its second value found when a request was answered), a fork is its source: a fork nested over it
        // pushes its own aggregation through it afterwards
        internal override TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            if (_completeSucessfully)
                return inner.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);

            var fork = new Fork<T, U, V, Inner, TPushEnumerator>(inner, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
            try
            {
                secondValue = _getSecondValue(new(fork));
            }
            finally
            {
                fork.Release();
            }
            _completeSucessfully = true;
            return Impl.ResultCast<U, TPushResult>(fork.FirstValue);
        }

        public override bool TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
        {
            if (_completeSucessfully)
                return inner.TryPushOptimization<TRequest, TRequestResult>(in request, out result);

            if (this.inner.TryPushOptimization<TRequest, TRequestResult>(in request, out result))
            {
                secondValue = _getSecondValue(new (new Fork<T, Inner>(inner)));
                _completeSucessfully = true;
                return true;
            }
            result = default;
            return false;
        }
    }

    sealed class Fork<T, U, V, Inner, TEnumerator1>
        : Fork<T, Inner>
        where Inner : INode<T>
        where TEnumerator1 : IPushEnumerator<T>
    {
        private TEnumerator1 FirstValueEnumerator;

        [AllowNull] public U FirstValue { get; private set; }

        // the first chain goes to the first push through the fork, which owns it from then on; a later push is the second
        // aggregation's alone
        private bool _handedOn;

        public Fork(in Inner inner, in TEnumerator1 fEnumerator1) : base(in inner)
        {
            this.inner = inner;
            this.FirstValueEnumerator = fEnumerator1;
        }

        // the first chain, when no push took it (the second aggregation threw first, or never pushed), is disposed
        internal void Release()
        {
            if (_handedOn)
                return;

            _handedOn = true;
            FirstValueEnumerator.Dispose();
        }

        internal override TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            if (_handedOn)
                return inner.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);

            _handedOn = true;
            var forward = new ForkForward<T, U, V, TEnumerator1, TPushEnumerator>(this.FirstValueEnumerator, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));

            // both chains are the inner push's once it has started them; disposed here when it fails first
            var started = false;
            try
            {
                var downstream = new Downstream<ForkForward<T, U, V, TEnumerator1, TPushEnumerator>>(in forward);
                var guard = new Guard<ForkForward<T, U, V, TEnumerator1, TPushEnumerator>, Downstream<ForkForward<T, U, V, TEnumerator1, TPushEnumerator>>>(ref started, in downstream);
                var (firstValue, secondValue) = inner.CreateViaPush<(U, V), ForkForward<T, U, V, TEnumerator1, TPushEnumerator>, Guard<ForkForward<T, U, V, TEnumerator1, TPushEnumerator>, Downstream<ForkForward<T, U, V, TEnumerator1, TPushEnumerator>>>>(in options, in guard);
                FirstValue = firstValue;
                return Impl.ResultCast<V, TPushResult>(secondValue);
            }
            finally
            {
                if (!started)
                    forward.Dispose();
            }
        }

        public override bool TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
        {
            if (inner.TryPushOptimization<TRequest, TRequestResult>(in request, out result))
            {
                if (!_handedOn)
                {
                    _handedOn = true;

                    var started = false;
                    try
                    {
                        var downstream = new Downstream<TEnumerator1>(in FirstValueEnumerator);
                        var guard = new Guard<TEnumerator1, Downstream<TEnumerator1>>(ref started, in downstream);
                        FirstValue = inner.CreateViaPush<U, TEnumerator1, Guard<TEnumerator1, Downstream<TEnumerator1>>>(default(ChainOptions), in guard);
                    }
                    finally
                    {
                        if (!started)
                            FirstValueEnumerator.Dispose();
                    }
                }

                return true;
            }
            return false;
        }
    }

    static class ForkImpl
    {
        internal static bool ProcessSpan<TElement, TPushEnumerator>(ReadOnlySpan<TElement> span, ref TPushEnumerator e)
            where TPushEnumerator : IPushEnumerator<TElement>
        {
            foreach (var item in span)
            {
                if (!e.ProcessNext(item))
                    return false;
            }
            return true;
        }

        internal static bool ProcessBatch<TElement, TPushEnumerator1, TPushEnumerator2>(scoped ref Batch<TElement> batch, ref TPushEnumerator1 t2u, ref bool t2uMoreData, ref TPushEnumerator2 t2v, ref bool t2vMoreData)
            where TPushEnumerator1 : IPushEnumerator<TElement>
            where TPushEnumerator2 : IPushEnumerator<TElement>
        {
            ProcessBranch(ref batch, ref t2u, ref t2uMoreData);
            ProcessBranch(ref batch, ref t2v, ref t2vMoreData);

            return t2uMoreData || t2vMoreData;
        }

        // the fork takes a batch either branch takes, so a branch that does not take it has its elements looped in
        private static void ProcessBranch<T, T2U>(scoped ref Batch<T> batch, ref T2U forwardEnumerator, ref bool moreData)
            where T2U : IPushEnumerator<T>
        {
            if (!moreData)
                return;

            moreData =
                T2U.AllowBatchesWhen.Takes(batch.Requires)
                    ? forwardEnumerator.ProcessBatch(ref batch)
                    : ProcessSpan(batch.GetSpan(), ref forwardEnumerator);
        }
    }

    struct ForkForward<T, U, V, T2U, T2V>
        : IPushEnumerator<T>
        where T2U : IPushEnumerator<T>
        where T2V : IPushEnumerator<T>
    {
        private T2U _t2u;
        private bool _t2u_more_data;
        private T2V _t2v;
        private bool _t2v_more_data;

        public ForkForward(T2U t2u, T2V t2v)
        {
            _t2u = t2u;
            _t2u_more_data = true;
            _t2v = t2v;
            _t2v_more_data = true;
        }

        // a span either branch takes, as the other has it looped in; a cold batch only both take, as looping it means
        // copying it out
        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen =>
            ((T2U.AllowBatchesWhen | T2V.AllowBatchesWhen) & ~BatchKinds.Cold)
            | (T2U.AllowBatchesWhen & T2V.AllowBatchesWhen & BatchKinds.Cold);

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch) =>
            ForkImpl.ProcessBatch(ref batch, ref _t2u, ref _t2u_more_data, ref _t2v, ref _t2v_more_data);

        public static bool AllowInitialise => T2U.AllowInitialise || T2V.AllowInitialise;

        public void Initialise()
        {
            if (T2U.AllowInitialise)
                _t2u.Initialise();
            if (T2V.AllowInitialise)
                _t2v.Initialise();
        }

        public void Dispose() { try { _t2u.Dispose(); } finally { _t2v.Dispose(); } }
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public (U, V) GetResult() => (_t2u.GetResult<U>(), _t2v.GetResult<V>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (_t2u_more_data) _t2u_more_data = _t2u.ProcessNext(input);
            if (_t2v_more_data) _t2v_more_data = _t2v.ProcessNext(input);
            return _t2u_more_data || _t2v_more_data;
        }
    }
}
