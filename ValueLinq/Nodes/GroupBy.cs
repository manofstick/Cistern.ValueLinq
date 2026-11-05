namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public static partial class GroupBy
{
    // a GroupBy builds its lookup when its enumerator is created or it is pushed through, and runs as the lookup from
    // there
    [Materialized]
    public partial struct Node<TSource, TKey, NodeT>
        : INode<System.Linq.IGrouping<TKey, TSource>>
        where NodeT : INode<TSource>
    {
        internal NodeT _nodeT;
        internal Func<TSource, TKey> _keySelector;
        internal IEqualityComparer<TKey>? _comparer;

        public Node(in NodeT nodeT, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            _nodeT = nodeT;
            _keySelector = keySelector;
            _comparer = comparer;
        }

        public void GetCountInformation(out CountInformation info) => info = new CountInformation();

        internal Lookup<TKey, TSource> Materialize(in ChainOptions options) =>
            _nodeT.CreateViaPush<Lookup<TKey, TSource>, ToLookup<TSource, TKey>, Construct<ToLookup<TSource, TKey>, ToLookup<TSource, TKey>.Args>>(in options, new Construct<ToLookup<TSource, TKey>, ToLookup<TSource, TKey>.Args>(new ToLookup<TSource, TKey>.Args(_comparer, _keySelector)));
    }

    public partial struct Pull<TSource, TKey, NodeT>
    {
        private LookupEnumerator<TKey, TSource> _enumerator;
    }

    [Materialized]
    public partial struct Node<TSource, TKey, TElement, NodeT>
        : INode<System.Linq.IGrouping<TKey, TElement>>
        where NodeT : INode<TSource>
    {
        internal NodeT _nodeT;
        internal Func<TSource, TKey> _keySelector;
        internal Func<TSource, TElement> _elementSelector;
        internal IEqualityComparer<TKey>? _comparer;

        public Node(in NodeT nodeT, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null)
                throw new ArgumentNullException(nameof(elementSelector));

            _nodeT = nodeT;
            _keySelector = keySelector;
            _elementSelector = elementSelector;
            _comparer = comparer;
        }

        public void GetCountInformation(out CountInformation info) => info = new CountInformation();

        internal Lookup<TKey, TElement> Materialize(in ChainOptions options) =>
            _nodeT.CreateViaPush<Lookup<TKey, TElement>, ToLookup<TSource, TKey, TElement>, Construct<ToLookup<TSource, TKey, TElement>, ToLookup<TSource, TKey, TElement>.Args>>(in options, new Construct<ToLookup<TSource, TKey, TElement>, ToLookup<TSource, TKey, TElement>.Args>(new ToLookup<TSource, TKey, TElement>.Args(_comparer, _keySelector, _elementSelector)));
    }

    public partial struct Pull<TSource, TKey, TElement, NodeT>
    {
        private LookupEnumerator<TKey, TElement> _enumerator;
    }

    internal static Lookup<TKey, TElement> CreateLookup<TElement, TKey>(IEqualityComparer<TKey>? comparer)
    {
        var lookup = new LookupWithComparer<TKey, TElement>(comparer ?? EqualityComparer<TKey>.Default);
        return lookup;
    }

    // a lookup's push: each grouping in turn, from the first added
    private static TResult FastEnumerate<TKey, TElement, TResult, TPushEnumerator>(Lookup<TKey, TElement> lookup, TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<System.Linq.IGrouping<TKey, TElement>>
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

    private static void Loop<TKey, TElement, TPushEnumerator>(Lookup<TKey, TElement> lookup, ref TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<System.Linq.IGrouping<TKey, TElement>>
    {
        var last = lookup._lastGrouping?._next;
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

    [Puller]
    internal partial struct LookupEnumerator<TKey, TElement>
    {
        private Grouping<TKey, TElement>? _lastGrouping;
        private Grouping<TKey, TElement>? _g;

        public LookupEnumerator(Grouping<TKey, TElement>? lastGrouping)
        {
            _lastGrouping = lastGrouping;
            _g = lastGrouping;
        }

        // the groupings are a ring through _next from the last added, so enumeration starts at the first
        private static LookupEnumerator<TKey, TElement> Create(in Lookup<TKey, TElement> lookup)
            => new(lookup._lastGrouping?._next);

        private Grouping<TKey, TElement>? _current;

        public bool MoveNext()
        {
            if (_g == null)
                return false;

            _current = _g;
            _g = _g._next;
            if (_g == _lastGrouping)
                _g = null;

            return true;
        }

        public System.Linq.IGrouping<TKey, TElement> Current => _current!;

        static TBuilt IPullEnumerator<System.Linq.IGrouping<TKey, TElement>>.Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder)
            => throw new InvalidOperationException();

        public void Dispose() => _g = _lastGrouping = null;
    }

    [DebuggerDisplay("Count = {Count}")]
    internal abstract partial class Lookup<TKey, TElement>
        : INode<System.Linq.IGrouping<TKey, TElement>>
        , System.Linq.ILookup<TKey, TElement>
        , ICollection<System.Linq.IGrouping<TKey, TElement>>
        , IReadOnlyCollection<System.Linq.IGrouping<TKey, TElement>>
    {
        GroupingArrayPool<TElement>? _poolOrNull;

        internal GroupingInternal<TKey, TElement>?[] _groupings;
        internal GroupingInternal<TKey, TElement>? _lastGrouping;

        internal Lookup()
        {
            _groupings = new GroupingInternal<TKey, TElement>?[7];
            _poolOrNull = null; // Initialize lazily, as only required for larger groupings
        }

        internal GroupingArrayPool<TElement> Pool =>
            _poolOrNull ??= new GroupingArrayPool<TElement>();

        public int Count { get; protected set; }

        public IEnumerable<TElement> this[TKey key] =>
            GetGrouping(key, create: false) ?? Containers.CisternEmpty.InstanceOf<TElement>.AsEnumerable;

        public bool Contains(TKey key) => GetGrouping(key, create: false) != null;

        // a read-only collection of the groupings, as System.Linq's lookup is, so a consumer reads its Count and copies it
        bool ICollection<System.Linq.IGrouping<TKey, TElement>>.IsReadOnly => true;

        void ICollection<System.Linq.IGrouping<TKey, TElement>>.Add(System.Linq.IGrouping<TKey, TElement> item) => ThrowHelper.ThrowNotSupportedException();

        void ICollection<System.Linq.IGrouping<TKey, TElement>>.Clear() => ThrowHelper.ThrowNotSupportedException();

        bool ICollection<System.Linq.IGrouping<TKey, TElement>>.Remove(System.Linq.IGrouping<TKey, TElement> item)
        {
            ThrowHelper.ThrowNotSupportedException();

            return false;
        }

        bool ICollection<System.Linq.IGrouping<TKey, TElement>>.Contains(System.Linq.IGrouping<TKey, TElement> item)
        {
            ArgumentNullException.ThrowIfNull(item);

            return GetGrouping(item.Key, create: false) is {} grouping
                && grouping == item;
        }

        void ICollection<System.Linq.IGrouping<TKey, TElement>>.CopyTo(System.Linq.IGrouping<TKey, TElement>[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);
            ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length);
            ArgumentOutOfRangeException.ThrowIfLessThan(array.Length - arrayIndex, Count, nameof(arrayIndex));

            var g = _lastGrouping;
            if (g == null)
                return;

            do
            {
                g = g._next;
                array[arrayIndex] = g;
                ++arrayIndex;
            }
            while (g != _lastGrouping);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => ((IEnumerable<System.Linq.IGrouping<TKey, TElement>>)this).GetEnumerator();

        IEnumerator<System.Linq.IGrouping<TKey, TElement>> IEnumerable<System.Linq.IGrouping<TKey, TElement>>.GetEnumerator() =>
            new PullEnumerator<LookupEnumerator<TKey, TElement>, System.Linq.IGrouping<TKey, TElement>>(new LookupEnumerator<TKey, TElement>(_lastGrouping?._next));

        internal abstract GroupingInternal<TKey, TElement>? GetGrouping(TKey key, bool create);

        private GroupingInternal<TKey, TElement>?[] Resize()
        {
            int newSize = checked((Count * 2) + 1);
            GroupingInternal<TKey, TElement>?[] newGroupings = new GroupingInternal<TKey, TElement>?[newSize];
            GroupingInternal<TKey, TElement> g = _lastGrouping!;
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

        public TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            where TPushEnumerator : IPushEnumerator<System.Linq.IGrouping<TKey, TElement>>
            where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct =>
            FastEnumerate<TKey, TElement, TPushResult, TPushEnumerator>(this, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));

        public void GetCountInformation(out CountInformation info)
            => info = new CountInformation(Count, Count > 0);

        bool INode.TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
        {
            result = default;
            return false;
        }
    }

    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(SystemLinq_LookupDebugView<,>))]
    internal sealed partial class LookupWithComparer<TKey, TElement> : Lookup<TKey, TElement>
    {
        private readonly IEqualityComparer<TKey> _comparer;

        [AllowNull] private GroupingInternal<TKey, TElement> _last;
        private bool _same;

        internal LookupWithComparer(IEqualityComparer<TKey> comparer) =>
            _comparer = comparer;

        internal sealed override GroupingInternal<TKey, TElement>? GetGrouping(TKey key, bool create)
        {
            if (_same && _comparer.Equals(_last._key, key))
            {
                return _last;
            }

            int hashCode = (key == null) ? 0 : _comparer.GetHashCode(key) & 0x7FFFFFFF;
            GroupingInternal<TKey, TElement>? g = _groupings[hashCode % _groupings.Length];
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
    [DebuggerTypeProxy(typeof(SystemLinq_LookupDebugView<,>))]
    internal sealed partial class LookupDefaultComparer<TKey, TElement> : Lookup<TKey, TElement>
    {
        private readonly EqualityComparer<TKey> _comparer = EqualityComparer<TKey>.Default;

        [AllowNull] private GroupingInternal<TKey, TElement> _last;
        private bool _same;

        internal sealed override GroupingInternal<TKey, TElement>? GetGrouping(TKey key, bool create)
        {
            if (_same && _comparer.Equals(_last._key, key))
            {
                return _last;
            }

            int hashCode = (key == null) ? 0 : _comparer.GetHashCode(key) & 0x7FFFFFFF;
            GroupingInternal<TKey, TElement>? g = _groupings[hashCode % _groupings.Length];
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

    [DebuggerDisplay("Key = {Key}")]
    [DebuggerTypeProxy(typeof(SystemLinq_GroupingDebugView<,>))]
    internal class GroupingInternal<TKey, TElement>
        : Grouping<TKey, TElement>
        , INode<TElement>
    {
        internal GroupingInternal(Lookup<TKey, TElement> owner) : base(owner) { }

        TPushResult INode<TElement>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler) =>
            this._count switch
            {
                0 => CisternEmpty.ExecutePush<TElement, TPushResult, TPushEnumerator>(PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler)),
                1 => CisternReturn.ExecutePush<TElement, TPushResult, TPushEnumerator>(_element, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler)),
                var count => SystemMemory.ExecutePush<TElement, TPushResult, TPushEnumerator>(_elementsOrNull.AsMemory(0, count), PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler))
            };

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_count, true);

        bool INode.TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result) =>
            this._count switch
            {
                0 => new CisternEmpty.Node<TElement>().TryPushOptimization<TRequest, TRequestResult>(in request, out result),
                1 => new CisternReturn.Node<TElement>(_element).TryPushOptimization<TRequest, TRequestResult>(in request, out result),
                var count => new SystemMemory.Node<TElement>(_elementsOrNull.AsMemory(0, count)).TryPushOptimization<TRequest, TRequestResult>(in request, out result)
            };
    }

    // It is (unfortunately) common to databind directly to Grouping.Key.
    // Because of this, we have to declare this internal type public so that we
    // can mark the Key property for public reflection.
    //
    // To limit the damage, the toolchain makes this type appear in a hidden assembly.
    // (This is also why it is no longer a nested type of Lookup<,>).
    [DebuggerDisplay("Key = {Key}")]
    [DebuggerTypeProxy(typeof(SystemLinq_GroupingDebugView<,>))]
    public abstract class Grouping<TKey, TElement>
        : System.Linq.IGrouping<TKey, TElement>
        , IList<TElement>
    {
        [AllowNull] internal TKey _key;
        internal int _hashCode;

        Lookup<TKey, TElement> _owner;
        internal int _count;
        /// <summary>
        /// for single elements buckets we don't allocate a seperate array, rather we use
        /// this slot to store the value.
        /// NB. _element is only valid when _count = 1
        /// </summary>
        [AllowNull] internal TElement _element;
        /// <summary>
        /// NB. _elementArray is not valid when _count = 1
        /// </summary>
        [AllowNull] internal TElement[] _elementsOrNull;

        internal GroupingInternal<TKey, TElement>? _hashNext;
        [AllowNull] internal GroupingInternal<TKey, TElement> _next;

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

        internal ReadOnlySpan<TElement> Elements
        {
            get
            {
                var elements =
                    _count == 1
                        ? new ReadOnlySpan<TElement>(in _element)
                        : _elementsOrNull.AsSpan(0, _count);

                return elements;
            }
        }

        private void Trim()
        {
            if (_elementsOrNull != null && _elementsOrNull.Length != _count)
            {
                Array.Resize(ref _elementsOrNull, _count);
            }
        }

        public IEnumerator<TElement> GetEnumerator() => new Enumerator(this);

        // resettable, as System.Linq's grouping enumerator is
        sealed class Enumerator
            : IEnumerator<TElement>
        {
            private readonly Grouping<TKey, TElement> _grouping;
            private int _index;
            [AllowNull] private TElement _current;

            public Enumerator(Grouping<TKey, TElement> grouping)
            {
                _grouping = grouping;
                _index = -1;
                _current = default;
            }

            public TElement Current => _current;

            object? System.Collections.IEnumerator.Current => _current;

            public void Dispose() { }

            public bool MoveNext()
            {
                var next = _index + 1;
                var found = next < _grouping._count;
                if (found)
                {
                    _index = next;
                    _current =
                        _grouping._count == 1
                            ? _grouping._element
                            : _grouping._elementsOrNull[next];
                }
                else
                {
                    _index = _grouping._count;
                    _current = default;
                }

                return found;
            }

            public void Reset() => (_index, _current) = (-1, default);
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

        private (TElement[]?, TElement[]?) _bucket_1;
        private (TElement[]?, TElement[]?) _bucket_2;
        private (TElement[]?, TElement[]?) _bucket_3;
        private (TElement[]?, TElement[]?) _bucket_4;

        private GroupingArrayPool<TElement>? _nextPool;
        private GroupingArrayPool<TElement> NextPool => _nextPool ?? (_nextPool = new GroupingArrayPool<TElement>());

        private static void TryPush(ref (TElement[]?, TElement[]?) store, TElement[] toStore)
        {
            if (store.Item2 != null)
                return;

            Array.Clear(toStore, 0, toStore.Length);

            store.Item2 = store.Item1;
            store.Item1 = toStore;
        }

        private static TElement[]? TryPop(ref (TElement[]?, TElement[]?) store)
        {
            var head = store.Item1;

            if (head != null)
            {
                store.Item1 = store.Item2;
                store.Item2 = null;
            }

            return head;
        }

        private static TElement[] Upgrade(ref (TElement[]?, TElement[]?) pushStore, ref (TElement[]?, TElement[]?) popStore, TElement[] currentElements)
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

    internal sealed class SystemLinq_LookupDebugView<TKey, TElement>
    {
        private readonly Lookup<TKey, TElement> _lookup;
        private System.Linq.IGrouping<TKey, TElement>[]? _cachedGroupings;

        public SystemLinq_LookupDebugView(Lookup<TKey, TElement> lookup) => _lookup = lookup;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public System.Linq.IGrouping<TKey, TElement>[] Groupings =>
            _cachedGroupings ??= System.Linq.Enumerable.ToArray((IEnumerable<System.Linq.IGrouping<TKey, TElement>>)_lookup);
    }

    internal sealed class SystemLinq_GroupingDebugView<TKey, TElement>
    {
        private readonly Grouping<TKey, TElement> _grouping;
        private TElement[]? _cachedValues;

        public SystemLinq_GroupingDebugView(Grouping<TKey, TElement> grouping) => _grouping = grouping;

        public TKey Key => _grouping.Key;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TElement[] Values => _cachedValues ??= System.Linq.Enumerable.ToArray((IEnumerable<TElement>)_grouping);
    }
}
