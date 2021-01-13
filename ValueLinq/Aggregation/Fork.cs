using System;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Aggregation
{
    public abstract class Fork<T>
        : INode<T>
    {
        public virtual CreationType CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Nodes>(ref Nodes nodes, ref Enumerator enumerator)
            where Enumerator : struct, IPullEnumerator<EnumeratorElement>
            where Nodes : INodes
        {
            InvalidOperation();
            return default;
        }

        public virtual CreationType CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
            where TNodes : INodes
        {
            InvalidOperation();
            return default;
        }

        public abstract TResult CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum) where TPushEnumerator : IPushEnumerator<T>;
        public abstract void GetCountInformation(out CountInformation info);

        public virtual bool TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) where Nodes : INodes
        {
            creation = default;
            return false;
        }

        public abstract bool TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result);

        protected void InvalidOperation() => throw new InvalidOperationException("Unexpected usage of fork");
    }

    public class Fork<T, Inner>
        : Fork<T>
        where Inner : INode<T>
    {
        protected Inner inner;

        internal Fork(in Inner inner) => this.inner = inner;

        public override void GetCountInformation(out CountInformation info) => inner.GetCountInformation(out info);

        public override TResult CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            => inner.CreateViaPush<TResult, TPushEnumerator>(fenum);
        public override bool TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => inner.TryPushOptimization<TRequest, TResult>(in request, out result);
    }

    sealed class Fork<T, U, V, Inner>
        : Fork<T, Inner>
        where Inner : INode<T>
    {
        bool _completeSucessfully;
        Func<ValueEnumerable<T, Fork<T>>, V> _getSecondValue;
        private V secondValue;

        public V SecondValue
        {
            get
            {
                if (!_completeSucessfully)
                    InvalidOperation();

                return secondValue;
            }
        }

        public Fork(in Inner inner, Func<ValueEnumerable<T, Fork<T>>, V> getSecondValue) : base(in inner) =>
            (_getSecondValue, _completeSucessfully) = (getSecondValue, false);

        public override TResult CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
        {
            var fork = new Fork<T, U, V, Inner, TPushEnumerator>(inner, fenum);
            secondValue = _getSecondValue(new(fork));
            _completeSucessfully = true;
            return (TResult)(object)fork.FirstValue;
        }

        public override bool TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (this.inner.TryPushOptimization<TRequest, TResult>(in request, out result))
            {
                secondValue = _getSecondValue(new (new Fork<T, Inner>(inner)));
                _completeSucessfully = true;
                return true;
            }
            result = default;
            return false;
        }
    }

    sealed class Fork<T, U, V, Inner, TPushEnumerator1>
        : Fork<T, Inner>
        where Inner : INode<T>
        where TPushEnumerator1 : IPushEnumerator<T>
    {
        private TPushEnumerator1 FirstValueEnumerator;

        public U FirstValue { get; private set; }

        public Fork(in Inner inner, in TPushEnumerator1 fEnumerator1) : base(in inner) => (this.inner, this.FirstValueEnumerator) = (inner, fEnumerator1);

        public override TResult CreateViaPush<TResult, TPushEnumerator2>(in TPushEnumerator2 fenum)
        {
            var (firstValue, secondValue) = inner.CreateViaPush<(U, V), ForkForward<T, U, V, TPushEnumerator1, TPushEnumerator2>>(new ForkForward<T, U, V, TPushEnumerator1, TPushEnumerator2>(this.FirstValueEnumerator, fenum));
            FirstValue = firstValue;
            return (TResult)(object)secondValue;
        }

        public override bool TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (inner.TryPushOptimization<TRequest, TResult>(in request, out result))
            {
                FirstValue = inner.CreateViaPush<U, TPushEnumerator1>(FirstValueEnumerator);
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

        internal static BatchProcessResult ProcessBatch<TElement, TObject, TPushEnumerator1, TPushEnumerator2>(TObject obj, Containers.GetSpan<TObject, TElement> getSpan, ref TPushEnumerator1 t2u, ref bool t2uMoreData, ref TPushEnumerator2 t2v, ref bool t2vMoreData)
            where TPushEnumerator1 : IPushEnumerator<TElement>
            where TPushEnumerator2 : IPushEnumerator<TElement>
        {
            var uResult = TryProcessBatch<TElement, TObject, TPushEnumerator1>(obj, getSpan, ref t2u, ref t2uMoreData);
            var vResult = TryProcessBatch<TElement, TObject, TPushEnumerator2>(obj, getSpan, ref t2v, ref t2vMoreData);

            if (!t2uMoreData && !t2vMoreData)
                return BatchProcessResult.SuccessAndHalt;
            else if (uResult != BatchProcessResult.Unavailable && vResult != BatchProcessResult.Unavailable)
                return BatchProcessResult.SuccessAndContinue;

            var span = getSpan(obj);

            if (uResult == BatchProcessResult.Unavailable)
                t2uMoreData = ProcessSpan(span, ref t2u);
            if (vResult == BatchProcessResult.Unavailable)
                t2vMoreData = ProcessSpan(span, ref t2v);
            if (!t2uMoreData && !t2vMoreData)
                return BatchProcessResult.SuccessAndHalt;

            return BatchProcessResult.SuccessAndContinue;
        }

        private static BatchProcessResult TryProcessBatch<T, TObject, T2U>(TObject obj, Containers.GetSpan<TObject, T> getSpan, ref T2U forwardEnumerator, ref bool moreData)
            where T2U : IPushEnumerator<T>
        {
            if (!moreData)
                return BatchProcessResult.SuccessAndHalt;

            var uResult = forwardEnumerator.TryProcessBatch<TObject, Containers.GetSpan<TObject, T>>(obj, getSpan);
            if (uResult == BatchProcessResult.SuccessAndHalt)
                moreData = false;
            return uResult;
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

        public ForkForward(T2U t2u, T2V t2v) => (_t2u, _t2u_more_data, _t2v, _t2v_more_data) = (t2u, true, t2v, true);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request)
        {
            if (typeof(TRequest) == typeof(Containers.GetSpan<TObject, T>))
            {
                var getSpan = (Containers.GetSpan<TObject, T>)(object)request;
                return ForkImpl.ProcessBatch(obj, getSpan, ref _t2u, ref _t2u_more_data, ref _t2v, ref _t2v_more_data);
            }
            return BatchProcessResult.Unavailable;
        }

        public void Dispose() { try { _t2u.Dispose(); } finally { _t2v.Dispose(); } }
        TResult IPushEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

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
