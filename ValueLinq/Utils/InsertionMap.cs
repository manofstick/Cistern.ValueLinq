using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cistern.ValueLinq.Utils
{
    // A map that is only added to, enumerated in insertion order, over arrays rented from the pool: CountBy's and
    // AggregateBy's, which never hand it out.
    //
    // Based on ZLinq's DictionarySlim (https://github.com/Cysharp/ZLinq, src/ZLinq/Internal/DictionarySlim.cs at commit
    // e262466f419f6587e2bdf7dd4bb1cb4c535f56f5, MIT licence, Copyright (c) Yoshifumi Kawai, Copyright (c) Cysharp, Inc.;
    // see THIRD-PARTY-NOTICES.TXT): an entry array in insertion order chained from an array of 1-based bucket indexes,
    // both rented and returned, the chains rebuilt on growth.
    // Added to it:
    // - bucket counts are primes indexed by Lemire's fastmod, as Dictionary's are, in place of its power-of-two mask,
    //   which puts keys sharing their low bits (k << 10) in one chain;
    // - a string key is indexed through a Dictionary, whose non-randomised string hashing is faster than the randomised
    //   hash any comparer outside it has;
    // - a value type's default comparer is called directly, so it devirtualises;
    // - a null key throws ArgumentNullException, as Dictionary's does.
    internal sealed class InsertionMap<TKey, TValue>
        where TKey : notnull
    {
        private const int InitialSize = 17;

        private struct Entry
        {
            public uint Hash;
            public int Next;     // the next entry of the bucket's chain, -1 at its end
            public TKey Key;
            public TValue Value;
        }

        private readonly IEqualityComparer<TKey>? _comparer;   // null for a value type's default comparer, so it devirtualises
        private readonly Dictionary<TKey, int>? _indexes;
        private Entry[]? _entries;
        private int[]? _buckets;      // 1-based entry indexes, 0 for an empty bucket
        private int _size;            // the bucket count, and the entries held before growing
        private ulong _multiplier;
        private int _count;

        public InsertionMap(IEqualityComparer<TKey>? comparer)
        {
            if (typeof(TKey) == typeof(string))
                _indexes = new Dictionary<TKey, int>(comparer);
            else if (!typeof(TKey).IsValueType)
                _comparer = comparer ?? EqualityComparer<TKey>.Default;
            else if (comparer is not null && comparer != EqualityComparer<TKey>.Default)
                _comparer = comparer;

            Allocate(InitialSize);
        }

        public int Count => _count;

        public ref TValue GetValueRefOrAddDefault(TKey key, out bool exists)
        {
            if (!typeof(TKey).IsValueType && _indexes is not null)
            {
                ref var index = ref CollectionsMarshal.GetValueRefOrAddDefault(_indexes, key, out exists);
                if (!exists)
                    index = Append(key, 0);

                return ref _entries![index].Value;
            }

            if (key is null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);

            var hash =
                (uint)(typeof(TKey).IsValueType && _comparer is null
                    ? EqualityComparer<TKey>.Default.GetHashCode(key!)
                    : _comparer!.GetHashCode(key!));

            var entries = _entries!;
            var i = _buckets![BucketOf(hash)] - 1;
            while ((uint)i < (uint)entries.Length)
            {
                ref var candidate = ref entries[i];
                if (candidate.Hash == hash
                    && (typeof(TKey).IsValueType && _comparer is null
                        ? EqualityComparer<TKey>.Default.Equals(candidate.Key, key)
                        : _comparer!.Equals(candidate.Key, key)))
                {
                    exists = true;

                    return ref candidate.Value;
                }

                i = candidate.Next;
            }

            exists = false;

            var added = Append(key!, hash);
            ref var bucket = ref _buckets![BucketOf(hash)];
            ref var entry = ref _entries![added];
            entry.Next = bucket - 1;
            bucket = added + 1;

            return ref entry.Value;
        }

        // the caller links the entry into its bucket, after any growth
        private int Append(TKey key, uint hash)
        {
            if (_count == _size)
                Grow();

            var index = _count++;
            ref var entry = ref _entries![index];
            entry.Hash = hash;
            entry.Key = key;
            entry.Value = default!;

            return index;
        }

        private int BucketOf(uint hash) =>
            (int)(uint)(((((_multiplier * hash) >> 32) + 1) * (uint)_size) >> 32);

        private void Allocate(int size)
        {
            _size = size;
            _multiplier = ulong.MaxValue / (uint)size + 1;
            _entries = ArrayPool<Entry>.Shared.Rent(size);
            if (_indexes is null)
            {
                _buckets = ArrayPool<int>.Shared.Rent(size);
                _buckets.AsSpan(0, size).Clear();
            }
        }

        private void Grow()
        {
            var entries = _entries!;
            var buckets = _buckets;

            Allocate(NextPrime(checked(_count * 2)));

            Array.Copy(entries, _entries!, _count);
            if (buckets is not null)
            {
                var rehashed = _entries!;
                for (var i = 0; i < _count; ++i)
                {
                    ref var bucket = ref _buckets![BucketOf(rehashed[i].Hash)];
                    rehashed[i].Next = bucket - 1;
                    bucket = i + 1;
                }
            }

            Return(entries, buckets);
        }

        private static int NextPrime(int min)
        {
            for (var candidate = min | 1; ; candidate += 2)
            {
                var prime = true;
                for (var divisor = 3; divisor * divisor <= candidate; divisor += 2)
                {
                    if (candidate % divisor == 0)
                    {
                        prime = false;
                        break;
                    }
                }

                if (prime)
                    return candidate;
            }
        }

        private static void Return(Entry[] entries, int[]? buckets)
        {
            ArrayPool<Entry>.Shared.Return(entries, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<Entry>());
            if (buckets is not null)
                ArrayPool<int>.Shared.Return(buckets);
        }

        // the arrays go back to the pool; a map whose build threw is left to the collector, its arrays with it
        public void Release()
        {
            if (_entries is null)
                return;

            Return(_entries, _buckets);
            _entries = null;
            _buckets = null;
        }

        public KeyValuePair<TKey, TValue>[] ToArrayAndRelease()
        {
            var result = new KeyValuePair<TKey, TValue>[_count];
            var entries = _entries!;
            for (var i = 0; i < result.Length; ++i)
                result[i] = new(entries[i].Key, entries[i].Value);

            Release();

            return result;
        }

        public Enumerator GetEnumerator() => new(this);

        // releases the map when disposed
        public struct Enumerator
            : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly InsertionMap<TKey, TValue>? _map;
            private int _index;

            public Enumerator(InsertionMap<TKey, TValue> map) =>
                (_map, _index) = (map, -1);

            public bool MoveNext() =>
                ++_index < _map!._count;

            public readonly KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    ref var entry = ref _map!._entries![_index];

                    return new(entry.Key, entry.Value);
                }
            }

            readonly object System.Collections.IEnumerator.Current => Current;

            public readonly void Dispose() => _map?.Release();

            public void Reset() => _index = -1;
        }
    }
}
