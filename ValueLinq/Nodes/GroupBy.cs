using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cistern.ValueLinq.Nodes
{
    internal struct LookupEnumerator<TKey, TElement>
        : IFastEnumerator<System.Linq.IGrouping<TKey, TElement>>
    {
        private Grouping<TKey, TElement> _lastGrouping;
        private Grouping<TKey, TElement> _g;

        public LookupEnumerator(Grouping<TKey, TElement> lastGrouping) =>
            (_lastGrouping, _g) = (lastGrouping, lastGrouping);

        public bool TryGetNext(out System.Linq.IGrouping<TKey, TElement> current)
        {
            current = _g;
            if (_g == null)
                return false;

            _g = _g._next;
            if (_g == _lastGrouping)
                _g = null;

            return true;
        }

        public void Dispose() => _g = _lastGrouping = null;
    }

    internal struct LookupResultEnumerator<TKey, TElement, TResult>
        : IFastEnumerator<TResult>
    {
        Func<TKey, IEnumerable<TElement>, TResult> _resultSelector;
        private Grouping<TKey, TElement> _lastGrouping;
        private Grouping<TKey, TElement> _g;

        public LookupResultEnumerator(Func<TKey, IEnumerable<TElement>, TResult> resultSelector, Grouping<TKey, TElement> lastGrouping) =>
            (_resultSelector, _lastGrouping, _g) = (resultSelector, lastGrouping, lastGrouping);

        public bool TryGetNext(out TResult current)
        {
            if (_g == null)
            {
                current = default;
                return false;
            }

            current = _resultSelector(_g.Key, _g);

            _g = _g._next;
            if (_g == _lastGrouping)
                _g = null;

            return true;
        }

        public void Dispose() => _g = _lastGrouping = null;
    }


    [DebuggerDisplay("Count = {Count}")]
    internal abstract partial class Lookup<TKey, TElement>
        : INode<System.Linq.IGrouping<TKey, TElement>>
        , System.Linq.ILookup<TKey, TElement>
        , IValueEnumerable<System.Linq.IGrouping<TKey, TElement>>
    {
        GroupingArrayPool<TElement> _poolOrNull;

        internal GroupingInternal<TKey, TElement>[] _groupings;
        internal GroupingInternal<TKey, TElement> _lastGrouping;

        internal Lookup()
        {
            _groupings = new GroupingInternal<TKey, TElement>[7];
            _poolOrNull = null; // Initialize lazily, as only required for larger groupings
        }

        internal GroupingArrayPool<TElement> Pool =>
            _poolOrNull ??= new GroupingArrayPool<TElement>();

        public int Count { get; protected set; }

        public IEnumerable<TElement> this[TKey key] =>
            GetGrouping(key, create: false) ?? Containers.InstanceOfEmpty<TElement>.AsEnumerable;

        public bool Contains(TKey key) => GetGrouping(key, create: false) != null;

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => ((IEnumerable<System.Linq.IGrouping<TKey, TElement>>)this).GetEnumerator();

        IEnumerator<System.Linq.IGrouping<TKey, TElement>> IEnumerable<System.Linq.IGrouping<TKey, TElement>>.GetEnumerator() =>
            new FastEnumeratorToEnumerator<System.Linq.IGrouping<TKey, TElement>, LookupEnumerator<TKey, TElement>>(new(_lastGrouping));

        ValueEnumerator<System.Linq.IGrouping<TKey, TElement>> IValueEnumerable<System.Linq.IGrouping<TKey, TElement>>.GetEnumerator() =>
            new(new FastEnumerator<LookupEnumerator<TKey, TElement>, System.Linq.IGrouping<TKey, TElement>>(new(_lastGrouping)));

        internal abstract GroupingInternal<TKey, TElement> GetGrouping(TKey key, bool create);

        private GroupingInternal<TKey, TElement>[] Resize()
        {
            int newSize = checked((Count * 2) + 1);
            GroupingInternal<TKey, TElement>[] newGroupings = new GroupingInternal<TKey, TElement>[newSize];
            GroupingInternal<TKey, TElement> g = _lastGrouping;
            do
            {
                g = g._next;
                int index = g._hashCode % newSize;
                g._hashNext = newGroupings[index];
                newGroupings[index] = g;
            }
            while (g != _lastGrouping);

            return newGroupings;
        }

        protected GroupingInternal<TKey, TElement> Create(TKey key, int hashCode)
        {
            if (Count == _groupings.Length)
            {
                _groupings = Resize();
            }

            int index = hashCode % _groupings.Length;
            GroupingInternal<TKey, TElement> g = new GroupingInternal<TKey, TElement>(this);
            g._key = key;
            g._hashCode = hashCode;
            g._hashNext = _groupings[index];
            _groupings[index] = g;
            if (_lastGrouping == null)
            {
                g._next = g;
            }
            else
            {
                g._next = _lastGrouping._next;
                _lastGrouping._next = g;
            }

            _lastGrouping = g;
            Count++;
            return g;
        }

        public TResult CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<System.Linq.IGrouping<TKey, TElement>> =>
            GroupByNode.FastEnumerate<TKey, TElement, TResult, FEnumerator>(this, fenum);

        public void GetCountInformation(out CountInformation info) => info = new CountInformation();

        public CreationType CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            // TODO
            throw new NotImplementedException();
        }

        public CreationType CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Nodes>(ref Nodes nodes, ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
            where Nodes : INodes
        {
            // TODO:
            throw new NotImplementedException();
        }

        public bool TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) where Nodes : INodes
        {
            creation = default;
            return false;
        }

        public bool TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }
    }

    [DebuggerDisplay("Count = {Count}")]
    internal sealed partial class LookupWithComparer<TKey, TElement> : Lookup<TKey, TElement>
    {
        private readonly IEqualityComparer<TKey> _comparer;

        private GroupingInternal<TKey, TElement> _last;
        private bool _same;

        internal LookupWithComparer(IEqualityComparer<TKey> comparer) =>
            _comparer = comparer;

        internal sealed override GroupingInternal<TKey, TElement> GetGrouping(TKey key, bool create)
        {
            if (_same && _comparer.Equals(_last._key, key))
            {
                return _last;
            }

            int hashCode = (key == null) ? 0 : _comparer.GetHashCode(key) & 0x7FFFFFFF;
            GroupingInternal<TKey, TElement> g = _groupings[hashCode % _groupings.Length];
            while (true)
            {
                if (g == null)
                {
                    _same = false;
                    return create ? Create(key, hashCode) : null;
                }

                if (g._hashCode == hashCode && _comparer.Equals(g._key, key))
                {
                    _same = ReferenceEquals(_last, g);
                    _last = g;
                    return g;
                }

                g = g._hashNext;
            }
        }
    }

    [DebuggerDisplay("Count = {Count}")]
    internal sealed partial class LookupDefaultComparer<TKey, TElement> : Lookup<TKey, TElement>
    {
        private readonly EqualityComparer<TKey> _comparer = EqualityComparer<TKey>.Default;

        private GroupingInternal<TKey, TElement> _last;
        private bool _same;

        internal sealed override GroupingInternal<TKey, TElement> GetGrouping(TKey key, bool create)
        {
            if (_same && _comparer.Equals(_last._key, key))
            {
                return _last;
            }

            int hashCode = (key == null) ? 0 : _comparer.GetHashCode(key) & 0x7FFFFFFF;
            GroupingInternal<TKey, TElement> g = _groupings[hashCode % _groupings.Length];
            while (true)
            {
                if (g == null)
                {
                    _same = false;
                    return create ? Create(key, hashCode) : null;
                }

                if (g._hashCode == hashCode && _comparer.Equals(g._key, key))
                {
                    _same = ReferenceEquals(_last, g);
                    _last = g;
                    return g;
                }

                g = g._hashNext;
            }
        }
    }

    // Grouping is a publically exposed class, so we provide this class get the Consumable
    [DebuggerDisplay("Key = {Key}")]
    //    [DebuggerTypeProxy(typeof(SystemLinq_GroupingDebugView<,>))]
    internal class GroupingInternal<TKey, TElement>
        : Grouping<TKey, TElement>
        , INode<TElement>
    {
        internal GroupingInternal(Lookup<TKey, TElement> owner) : base(owner) { }

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) =>
            this._count switch
            {
                0 => EmptyNode.Create<TElement, Nodes<Head, Tail>, CreationType>(ref nodes),
                1 => ReturnNode.Create<TElement, CreationType, Head, Tail>(_element, ref nodes),
                var count => ArrayNode.Create<TElement, Nodes<Head, Tail>, CreationType>(_elementsOrNull, 0, count, ref nodes)
            };

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Nodes>(ref Nodes nodes, ref Enumerator enumerator)
            => throw new InvalidOperationException();
        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        TResult INode<TElement>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum) =>
            this._count switch
            {
                0 => EmptyNode.FastEnumerate<TElement, TResult, FEnumerator>(fenum),
                1 => ReturnNode.FastEnumerate<TElement, TResult, FEnumerator>(_element, fenum),
                var count => MemoryNode.FastEnumerate<TElement, TResult, FEnumerator>(_elementsOrNull.AsMemory(0, count), fenum)
            };

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_count, true);

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) =>
            this._count switch
            {
                0 => EmptyNode.TryPushOptimization<TElement, TRequest, TResult>(in request, out result),
                1 => ReturnNode.TryPushOptimization<TElement, TRequest, TResult>(_element, in request, out result),
                var count => MemoryNode.TryPushOptimization<TElement, TRequest, TResult>(_elementsOrNull.AsMemory(0, count), in request, out result)
            };
    }

    // It is (unfortunately) common to databind directly to Grouping.Key.
    // Because of this, we have to declare this internal type public so that we
    // can mark the Key property for public reflection.
    //
    // To limit the damage, the toolchain makes this type appear in a hidden assembly.
    // (This is also why it is no longer a nested type of Lookup<,>).
    [DebuggerDisplay("Key = {Key}")]
//    [DebuggerTypeProxy(typeof(SystemLinq_GroupingDebugView<,>))]
    public abstract class Grouping<TKey, TElement>
        : System.Linq.IGrouping<TKey, TElement>
        , IList<TElement>
    {
        internal TKey _key;
        internal int _hashCode;

        Lookup<TKey, TElement> _owner;
        internal int _count;
        /// <summary>
        /// for single elements buckets we don't allocate a seperate array, rather we use
        /// this slot to store the value. 
        /// NB. _element is only valid when _count = 1
        /// </summary>
        internal TElement _element;
        /// <summary>
        /// NB. _elementArray is not valid when _count = 1
        /// </summary>
        internal TElement[] _elementsOrNull;

        internal GroupingInternal<TKey, TElement> _hashNext;
        internal GroupingInternal<TKey, TElement> _next;

        internal Grouping(Lookup<TKey, TElement> owner)
        {
            _owner = owner;
            _elementsOrNull = null;
        }

        internal void Add(TElement element)
        {
            if (_count == 0)
            {
                _element = element;
                _count = 1;
            }
            else
            {
                if (_count == 1)
                {
                    _elementsOrNull = _owner.Pool.Alloc();
                    _elementsOrNull[0] = _element;
                    _element = default(TElement);
                }

                if (_elementsOrNull.Length == _count)
                {
                    _elementsOrNull = _owner.Pool.Upgrade(_elementsOrNull);
                }

                _elementsOrNull[_count] = element;
                _count++;
            }
        }

        private void Trim()
        {
            if (_elementsOrNull != null && _elementsOrNull.Length != _count)
            {
                Array.Resize(ref _elementsOrNull, _count);
            }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            if (_count == 1)
            {
                yield return _element;
            }
            else
            {
                for (int i = 0; i < _count; i++)
                {
                    yield return _elementsOrNull[i];
                }
            }
        }

        internal IList<TElement> GetEfficientList(bool canTrim)
        {
            if (_count <= 1 || (!canTrim && _count != _elementsOrNull.Length))
            {
                return this;
            }

            Trim();

            return _elementsOrNull;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        // DDB195907: implement IGrouping<>.Key implicitly
        // so that WPF binding works on this property.
        public TKey Key => _key;

        int ICollection<TElement>.Count => _count;

        bool ICollection<TElement>.IsReadOnly => true;

        void ICollection<TElement>.Add(TElement item) => ThrowHelper.ThrowNotSupportedException();

        void ICollection<TElement>.Clear() => ThrowHelper.ThrowNotSupportedException();

        bool ICollection<TElement>.Contains(TElement item)
        {
            return _count switch
            {
                0 => false,
                1 => EqualityComparer<TElement>.Default.Equals(item, _element),
                _ => Array.IndexOf(_elementsOrNull, item, 0, _count) >= 0
            };
        }

        void ICollection<TElement>.CopyTo(TElement[] array, int arrayIndex)
        {
            switch (_count)
            {
                case 0:
                    break;

                case 1:
                    array[arrayIndex] = _element;
                    break;

                default:
                    Array.Copy(_elementsOrNull, 0, array, arrayIndex, _count);
                    break;
            }
        }

        bool ICollection<TElement>.Remove(TElement item)
        {
            ThrowHelper.ThrowNotSupportedException();
            return false;
        }

        int IList<TElement>.IndexOf(TElement item)
        {
            return _count switch
            {
                0 => -1,
                1 => EqualityComparer<TElement>.Default.Equals(item, _element) ? 0 : -1,
                _ => Array.IndexOf(_elementsOrNull, item, 0, _count),
            };
        }

        void IList<TElement>.Insert(int index, TElement item) => ThrowHelper.ThrowNotSupportedException();

        void IList<TElement>.RemoveAt(int index) => ThrowHelper.ThrowNotSupportedException();

        TElement IList<TElement>.this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index);
                }

                if (_count == 1)
                    return _element;

                return _elementsOrNull[index];
            }

            set
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }
    }
    internal sealed class GroupingArrayPool<TElement>
    {
        const int MinLength = 4; // relates to MinShift
        const int MinShift = 2; // relates to MinLength

        const int Buckets = 4;

        private (TElement[], TElement[]) _bucket_1;
        private (TElement[], TElement[]) _bucket_2;
        private (TElement[], TElement[]) _bucket_3;
        private (TElement[], TElement[]) _bucket_4;

        private GroupingArrayPool<TElement> _nextPool;
        private GroupingArrayPool<TElement> NextPool => _nextPool ?? (_nextPool = new GroupingArrayPool<TElement>());

        private static void TryPush(ref (TElement[], TElement[]) store, TElement[] toStore)
        {
            if (store.Item2 != null)
                return;

            Array.Clear(toStore, 0, toStore.Length);

            store.Item2 = store.Item1;
            store.Item1 = toStore;
        }

        private static TElement[] TryPop(ref (TElement[], TElement[]) store)
        {
            var head = store.Item1;

            if (head != null)
            {
                store.Item1 = store.Item2;
                store.Item2 = null;
            }

            return head;
        }

        private static TElement[] Upgrade(ref (TElement[], TElement[]) pushStore, ref (TElement[], TElement[]) popStore, TElement[] currentElements)
        {
            var newElements = TryPop(ref popStore);
            if (newElements == null)
            {
                newElements = new TElement[checked(currentElements.Length * 2)];
            }
            currentElements.CopyTo(newElements, 0);
            TryPush(ref pushStore, currentElements);
            return newElements;
        }

        private TElement[] FindBucketAndUpgrade(TElement[] currentElements, int shiftedLength)
        {
            if (shiftedLength <= 0x8)
            {
                switch (shiftedLength)
                {
                    case 1: return Upgrade(ref _bucket_1, ref _bucket_2, currentElements);
                    case 2: return Upgrade(ref _bucket_2, ref _bucket_3, currentElements);
                    case 4: return Upgrade(ref _bucket_3, ref _bucket_4, currentElements);
                    case 8: return Upgrade(ref _bucket_4, ref NextPool._bucket_1, currentElements);
                }
            }
            return NextPool.FindBucketAndUpgrade(currentElements, shiftedLength >> Buckets);
        }

        private static bool IsPowerOf2(int n) => n > 0 && (n & (n - 1)) == 0;

        public TElement[] Upgrade(TElement[] currentElements)
        {
            var length = currentElements.Length;

            Debug.Assert(IsPowerOf2(length), "Only powers of 2 lengths should be accepted");
            Debug.Assert(length >= MinLength, "Minimum size should be 4");

            var shiftedLength = length >> MinShift;

            return FindBucketAndUpgrade(currentElements, shiftedLength);
        }

        public TElement[] Alloc() => TryPop(ref _bucket_1) ?? new TElement[MinLength];
    }



    public struct GroupByNode<TSource, TKey, NodeT>
        : INode<System.Linq.IGrouping<TKey, TSource>>
        where NodeT : INode<TSource>
    {
        private NodeT _nodeT;
        private Func<TSource, TKey> _keySelector;
        private IEqualityComparer<TKey> _comparer;

        public void GetCountInformation(out CountInformation info) => info = new CountInformation();

        public GroupByNode(in NodeT nodeT, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            (_nodeT, _keySelector, _comparer) = (nodeT, keySelector, comparer);
        }

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var lookup = _nodeT.CreateViaPush<Lookup<TKey, TSource>, LookupFoward<TSource, TKey>>(new LookupFoward<TSource, TKey>(_comparer, _keySelector));
            var enumerator = new LookupEnumerator<TKey, TSource>(lookup._lastGrouping);
            return nodes.CreateObject<CreationType, System.Linq.IGrouping<TKey, TSource>, LookupEnumerator<TKey, TSource>>(ref enumerator);
        }

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, CreationType, Tail>(in TRequest request, ref Tail tail, out CreationType creation)
        {
            creation = default;
            return false;
        }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<System.Linq.IGrouping<TKey, TSource>>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum)
        {
            var lookup = _nodeT.CreateViaPush<Lookup<TKey, TSource>, LookupFoward<TSource, TKey>>(new LookupFoward<TSource, TKey>(_comparer, _keySelector));
            return GroupByNode.FastEnumerate<TKey, TSource, TResult, FEnumerator>(lookup, fenum);
        }
    }

    public struct GroupByNode<TSource, TKey, TElement, NodeT>
        : INode<System.Linq.IGrouping<TKey, TElement>>
        where NodeT : INode<TSource>
    {
        private NodeT _nodeT;
        private Func<TSource, TKey> _keySelector;
        private Func<TSource, TElement> _elementSelector;
        private IEqualityComparer<TKey> _comparer;

        public void GetCountInformation(out CountInformation info) => info = new CountInformation();

        public GroupByNode(in NodeT nodeT, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null)
                throw new ArgumentNullException(nameof(elementSelector));

            (_nodeT, _keySelector, _elementSelector, _comparer) = (nodeT, keySelector, elementSelector, comparer);
        }

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var lookup = _nodeT.CreateViaPush<Lookup<TKey, TElement>, LookupFoward<TSource, TKey, TElement>>(new LookupFoward<TSource, TKey, TElement>(_comparer, _keySelector, _elementSelector));
            var enumerator = new LookupEnumerator<TKey, TElement>(lookup._lastGrouping);
            return nodes.CreateObject<CreationType, System.Linq.IGrouping<TKey, TElement>, LookupEnumerator<TKey, TElement>>(ref enumerator);
        }

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, CreationType, Tail>(in TRequest request, ref Tail tail, out CreationType creation)
        {
            creation = default;
            return false;
        }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<System.Linq.IGrouping<TKey, TElement>>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum)
        {
            var lookup = _nodeT.CreateViaPush<Lookup<TKey, TElement>, LookupFoward<TSource, TKey, TElement>>(new LookupFoward<TSource, TKey, TElement>(_comparer, _keySelector, _elementSelector));
            return GroupByNode.FastEnumerate<TKey, TElement, TResult, FEnumerator>(lookup, fenum);
        }
    }

    public struct GroupByResultNode<TSource, TKey, TResult, NodeT>
        : INode<TResult>
        where NodeT : INode<TSource>
    {
        private NodeT _nodeT;
        private Func<TSource, TKey> _keySelector;
        private Func<TKey, IEnumerable<TSource>, TResult> _resultSelector;
        private IEqualityComparer<TKey> _comparer;

        public void GetCountInformation(out CountInformation info) => info = new CountInformation();

        public GroupByResultNode(in NodeT nodeT, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            (_nodeT, _keySelector, _resultSelector, _comparer) = (nodeT, keySelector, resultSelector, comparer);
        }

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var lookup = _nodeT.CreateViaPush<Lookup<TKey, TSource>, LookupFoward<TSource, TKey>>(new LookupFoward<TSource, TKey>(_comparer, _keySelector));
            var enumerator = new LookupResultEnumerator<TKey, TSource, TResult>(_resultSelector, lookup._lastGrouping?._next); // why start with _next? to get same result as System.Linq...
            return nodes.CreateObject<CreationType, TResult, LookupResultEnumerator<TKey, TSource, TResult>>(ref enumerator);
        }

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, CreationType, Tail>(in TRequest request, ref Tail tail, out CreationType creation)
        {
            creation = default;
            return false;
        }

        bool INode.TryPushOptimization<TRequest, TPushResult>(in TRequest request, out TPushResult result)
        {
            result = default;
            return false;
        }

        TPushResult INode<TResult>.CreateViaPush<TPushResult, FEnumerator>(in FEnumerator fenum)
        {
            var lookup = _nodeT.CreateViaPush<Lookup<TKey, TSource>, LookupFoward<TSource, TKey>>(new LookupFoward<TSource, TKey>(_comparer, _keySelector));
            return GroupByResultNode.FastEnumerate<TKey, TSource, TResult, TPushResult, FEnumerator>(lookup, _resultSelector, fenum);
        }
    }

    public struct GroupByResultNode<TSource, TKey, TElement, TResult, NodeT>
        : INode<TResult>
        where NodeT : INode<TSource>
    {
        private NodeT _nodeT;
        private Func<TSource, TKey> _keySelector;
        private Func<TSource, TElement> _elementSelector;
        private Func<TKey, IEnumerable<TElement>, TResult> _resultSelector;
        private IEqualityComparer<TKey> _comparer;

        public void GetCountInformation(out CountInformation info) => info = new CountInformation();

        public GroupByResultNode(in NodeT nodeT, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null)
                throw new ArgumentNullException(nameof(elementSelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            (_nodeT, _keySelector, _elementSelector, _resultSelector, _comparer) = (nodeT, keySelector, elementSelector, resultSelector, comparer);
        }

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var lookup = _nodeT.CreateViaPush<Lookup<TKey, TElement>, LookupFoward<TSource, TKey, TElement>>(new LookupFoward<TSource, TKey, TElement>(_comparer, _keySelector, _elementSelector));
            var enumerator = new LookupResultEnumerator<TKey, TElement, TResult>(_resultSelector, lookup._lastGrouping?._next); // why start with _next? to get same result as System.Linq...
            return nodes.CreateObject<CreationType, TResult, LookupResultEnumerator<TKey, TElement, TResult>>(ref enumerator);
        }

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, CreationType, Tail>(in TRequest request, ref Tail tail, out CreationType creation)
        {
            creation = default;
            return false;
        }

        bool INode.TryPushOptimization<TRequest, TPushResult>(in TRequest request, out TPushResult result)
        {
            result = default;
            return false;
        }

        TPushResult INode<TResult>.CreateViaPush<TPushResult, FEnumerator>(in FEnumerator fenum)
        {
            var lookup = _nodeT.CreateViaPush<Lookup<TKey, TElement>, LookupFoward<TSource, TKey, TElement>>(new LookupFoward<TSource, TKey, TElement>(_comparer, _keySelector, _elementSelector));
            return GroupByResultNode.FastEnumerate<TKey, TElement, TResult, TPushResult, FEnumerator>(lookup, _resultSelector, fenum);
        }
    }

    static class GroupByNode
    {
        public static Lookup<TKey, TElement> CreateLookup<TElement, TKey>(IEqualityComparer<TKey> comparer)
        {
            var lookup = new LookupWithComparer<TKey, TElement>(comparer ?? EqualityComparer<TKey>.Default);
            return lookup;
        }

        internal static TResult FastEnumerate<TKey, TElement, TResult, FEnumerator>(Lookup<TKey, TElement> lookup, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<System.Linq.IGrouping<TKey, TElement>>
        {
            try
            {
                Loop(lookup, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        private static void Loop<TKey, TElement, FEnumerator>(Lookup<TKey, TElement> lookup, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<System.Linq.IGrouping<TKey, TElement>>
        {
            var last = lookup._lastGrouping;
            var current = last;
            if (current != null)
            {
                do
                {
                    if (!fenum.ProcessNext(current))
                        break;
                    current = current._next;
                } while (current != last);
            }
        }
    }
    static class GroupByResultNode
    {
        internal static TResultx FastEnumerate<TKey, TElement, TResult, TResultx, FEnumerator>(Lookup<TKey, TElement> lookup, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TResult>
        {
            try
            {
                Loop(lookup, resultSelector, ref fenum);
                return fenum.GetResult<TResultx>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        private static void Loop<TKey, TElement, TResult, FEnumerator>(Lookup<TKey, TElement> lookup, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TResult>
        {
            var last = lookup._lastGrouping?._next;
            var current = last;
            if (current != null)
            {
                do
                {
                    if (!fenum.ProcessNext(resultSelector(current.Key, current)))
                        break;
                    current = current._next;
                } while (current != last);
            }
        }
    }


    struct LookupFoward<TSource, TKey>
        : IForwardEnumerator<TSource>
    {
        Lookup<TKey, TSource> _lookup;
        Func<TSource, TKey> _keySelector;

        public LookupFoward(IEqualityComparer<TKey> comparer, Func<TSource, TKey> keySelector)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            (_keySelector, _lookup) = (keySelector, GroupByNode.CreateLookup<TSource, TKey>(comparer));
        }

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }

        public TResult GetResult<TResult>() => (TResult)(object)GetResult();
        public Lookup<TKey, TSource> GetResult() => _lookup;

        public bool ProcessNext(TSource input)
        {
            _lookup.GetGrouping(_keySelector(input), true).Add(input);
            return true;
        }
    }

    struct LookupFoward<TSource, TKey, TElement>
            : IForwardEnumerator<TSource>
    {
        Lookup<TKey, TElement> _lookup;
        Func<TSource, TKey> _keySelector;
        Func<TSource, TElement> _elementSelector;

        public LookupFoward(IEqualityComparer<TKey> comparer, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            (_keySelector, _elementSelector, _lookup) = (keySelector, elementSelector, GroupByNode.CreateLookup<TElement, TKey>(comparer));
        }

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }

        public TResult GetResult<TResult>() => (TResult)(object)GetResult();
        public Lookup<TKey, TElement> GetResult() => _lookup;

        public bool ProcessNext(TSource input)
        {
            _lookup.GetGrouping(_keySelector(input), true).Add(_elementSelector(input));
            return true;
        }
    }
}
