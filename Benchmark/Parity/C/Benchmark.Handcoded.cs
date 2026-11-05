using BenchmarkDotNet.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class C
    {
        [Benchmark]
        public int C1a_Handcoded() =>
            CountOf(_set);

        [Benchmark]
        public int C1b_Handcoded() =>
            CountOf(_nonGeneric);

        // System.Linq's result needs the runtime type tests: a count read where the source has one
        [Benchmark]
        public int C2a_Handcoded() =>
            NonEnumeratedCount(_set);

        [Benchmark]
        public int C2b_Handcoded() =>
            NonEnumeratedCount(_nonGeneric);

        [Benchmark]
        public bool C3a_Handcoded()
        {
            foreach (var x in _set)
                return true;

            return false;
        }

        [Benchmark]
        public bool C3b_Handcoded()
        {
            foreach (var x in _iterator)
                return true;

            return false;
        }

        [Benchmark]
        public int C4_Handcoded()
        {
            foreach (var x in _collection)
                return x;

            throw new InvalidOperationException();
        }

        [Benchmark]
        public int C5_Handcoded()
        {
            var found = false;
            var last = 0;
            foreach (var x in _collection)
            {
                found = true;
                last = x;
            }

            return found ? last : throw new InvalidOperationException();
        }

        [Benchmark]
        public int C6_Handcoded()
        {
            var found = false;
            var last = 0;
            foreach (var x in _collection)
            {
                if (_predicate(x))
                {
                    found = true;
                    last = x;
                }
            }

            return found ? last : throw new InvalidOperationException();
        }

        [Benchmark]
        public int C7_Handcoded()
        {
            var index = 0;
            foreach (var x in _collection)
            {
                if (index == Length / 2)
                    return x;

                ++index;
            }

            throw new ArgumentOutOfRangeException("index");
        }

        [Benchmark]
        public int C8a_Handcoded() =>
            FromEnd(_collection, 10);

        [Benchmark]
        public int C8b_Handcoded() =>
            FromEnd(_set, 10);

        [Benchmark]
        public int C9_Handcoded()
        {
            var found = false;
            var single = 0;
            foreach (var x in _single)
            {
                if (found)
                    throw new InvalidOperationException();

                found = true;
                single = x;
            }

            return found ? single : throw new InvalidOperationException();
        }

        [Benchmark]
        public bool C10a_Handcoded()
        {
            foreach (var x in _set)
                if (x == _present)
                    return true;

            return false;
        }

        [Benchmark]
        public bool C10b_Handcoded()
        {
            foreach (var x in _collection)
                if (x == _present)
                    return true;

            return false;
        }

        // System.Linq's result needs the runtime type test: the set's own comparer ignores case
        [Benchmark]
        public bool C10c_Handcoded()
        {
            if (_words is ICollection<string> collection)
                return collection.Contains(_wordAsked);

            foreach (var x in _words)
                if (EqualityComparer<string>.Default.Equals(x, _wordAsked))
                    return true;

            return false;
        }

        [Benchmark]
        public bool C11_Handcoded()
        {
            var comparer = EqualityComparer<int>.Default;
            foreach (var x in _collection)
                if (comparer.Equals(x, _present))
                    return true;

            return false;
        }

        [Benchmark]
        public int[] C12a_Handcoded() =>
            ListOf(_set).ToArray();

        [Benchmark]
        public int[] C12b_Handcoded() =>
            ListOf(_collection).ToArray();

        [Benchmark]
        public List<int> C13a_Handcoded() =>
            ListOf(_set);

        [Benchmark]
        public List<int> C13b_Handcoded() =>
            ListOf(_collection);

        [Benchmark]
        public int C14a_Handcoded() =>
            DictionaryOf(_set).Count;

        [Benchmark]
        public int C14b_Handcoded() =>
            DictionaryOf(_emptyCollection).Count;

        [Benchmark]
        public bool C16a_Handcoded()
        {
            if (_array.Length != _arrayCopy.Length)
                return false;

            for (var i = 0; i < _array.Length; ++i)
                if (_array[i] != _arrayCopy[i])
                    return false;

            return true;
        }

        [Benchmark]
        public bool C16b_Handcoded()
        {
            if (_listA.Count != _listB.Count)
                return false;

            for (var i = 0; i < _listA.Count; ++i)
                if (_listA[i] != _listB[i])
                    return false;

            return true;
        }

        [Benchmark]
        public bool C17a_Handcoded() =>
            SameSequence(_set, _setOther);

        [Benchmark]
        public bool C17b_Handcoded() =>
            SameSequence(_collection, _collectionCopy);

        [Benchmark]
        public bool C17c_Handcoded()
        {
            using var first = _iterator.GetEnumerator();
            using var second = _iterator.GetEnumerator();
            while (true)
            {
                var hasFirst = first.MoveNext();
                if (hasFirst != second.MoveNext())
                    return false;

                if (!hasFirst)
                    return true;

                if (_keySelector(first.Current) != _keySelector(second.Current))
                    return false;
            }
        }

        [Benchmark]
        public int[] C19a_Handcoded()
        {
            var last = new Queue<int>();
            foreach (var x in _collection)
            {
                if (last.Count == 10)
                    last.Dequeue();

                last.Enqueue(x);
            }

            return last.ToArray();
        }

        [Benchmark]
        public int C19b_Handcoded()
        {
            var count = CountOf(_set);
            return count > 10 ? count - 10 : 0;
        }

        [Benchmark]
        public int C20_Handcoded()
        {
            var total = 0;
            for (var start = 0; start < _array.Length; start += 100)
                total += Math.Min(100, _array.Length - start);

            return total;
        }

        static int CountOf(IEnumerable<int> source)
        {
            var count = 0;
            foreach (var x in source)
                ++count;

            return count;
        }

        static int NonEnumeratedCount(IEnumerable<int> source) =>
            source is ICollection<int> collection ? collection.Count
            : source is ICollection nonGeneric ? nonGeneric.Count
            : -1;

        static int FromEnd(IEnumerable<int> source, int fromEnd)
        {
            var list = ListOf(source);
            return list[list.Count - fromEnd];
        }

        static List<int> ListOf(IEnumerable<int> source)
        {
            var list = new List<int>();
            foreach (var x in source)
                list.Add(x);

            return list;
        }

        Dictionary<int, int> DictionaryOf(IEnumerable<int> source)
        {
            var dictionary = new Dictionary<int, int>();
            foreach (var x in source)
                dictionary.Add(_keySelector(x), x);

            return dictionary;
        }

        static bool SameSequence(IEnumerable<int> left, IEnumerable<int> right)
        {
            using var first = left.GetEnumerator();
            using var second = right.GetEnumerator();
            while (true)
            {
                var hasFirst = first.MoveNext();
                if (hasFirst != second.MoveNext())
                    return false;

                if (!hasFirst)
                    return true;

                if (first.Current != second.Current)
                    return false;
            }
        }
    }
}
