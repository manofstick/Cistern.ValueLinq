using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B11
    {
        [Benchmark]
        public int B78a_Handcoded() =>
            _list.Count == 0 ? -1 : _list[0];

        [Benchmark]
        public int B78b_Handcoded() =>
            _list.Count == 0 ? -1 : _list[_list.Count - 1];

        [Benchmark]
        public int B78c_Handcoded()
        {
            // an empty list is the single element -1
            var count = _list.Count == 0 ? 1 : _list.Count;
            if (Length / 2 >= count)
                throw new ArgumentOutOfRangeException("index");

            return _list.Count == 0 ? -1 : _list[Length / 2];
        }

        [Benchmark]
        public int B78d_Handcoded() =>
            _list.Count == 0 ? 1 : _list.Count;

        [Benchmark]
        public int[] B78e_Handcoded() =>
            _list.Count == 0 ? new[] { -1 } : _list.ToArray();

        [Benchmark]
        public List<int> B78f_Handcoded()
        {
            if (_list.Count == 0)
                return new List<int> { -1 };

            return new List<int>(_list);
        }

        [Benchmark]
        public bool B79a_Handcoded() =>
            _set.Count == 0 ? _present == -1 : _set.Contains(_present);

        [Benchmark]
        public bool B79b_Handcoded() =>
            _empty.Count == 0 ? true : _empty.Contains(-1);

        [Benchmark]
        public int B80a_Handcoded() =>
            _arrayList.Count;

        [Benchmark]
        public int[] B80b_Handcoded()
        {
            var result = new int[_arrayList.Count];
            for (var i = 0; i < result.Length; ++i)
                result[i] = (int)_arrayList[i];

            return result;
        }

        [Benchmark]
        public List<int> B80c_Handcoded()
        {
            var result = new List<int>(_arrayList.Count);
            for (var i = 0; i < _arrayList.Count; ++i)
                result.Add((int)_arrayList[i]);

            return result;
        }

        [Benchmark]
        public int B80d_Handcoded() =>
            _arrayList.Count;

        [Benchmark]
        public int B81a_Handcoded()
        {
            var count = 0;
            foreach (var inner in _jagged)
                count += inner.Length;

            return count;
        }

        [Benchmark]
        public bool B81b_Handcoded()
        {
            foreach (var inner in _jagged)
                foreach (var x in inner)
                    if (x == _presentInJagged)
                        return true;

            return false;
        }

        [Benchmark]
        public int[] B81c_Handcoded()
        {
            var count = 0;
            foreach (var inner in _jagged)
                count += inner.Length;

            var result = new int[count];
            var index = 0;
            foreach (var inner in _jagged)
            {
                inner.CopyTo(result, index);
                index += inner.Length;
            }

            return result;
        }

        [Benchmark]
        public List<int> B81d_Handcoded()
        {
            var result = new List<int>();
            foreach (var inner in _jagged)
                result.AddRange(inner);

            return result;
        }

        [Benchmark]
        public int B82a_Handcoded() =>
            Group(_array, _keySelector).Count;

        [Benchmark]
        public int B82b_Handcoded()
        {
            var count = 0;
            foreach (var group in Group(_array, _keySelector))
            {
                _resultSelector(group.Key, group.Value);
                ++count;
            }

            return count;
        }

        [Benchmark]
        public int B82c_Handcoded()
        {
            var groups = Group(_array, _keySelector);
            var result = new List<int>[groups.Count];
            groups.Values.CopyTo(result, 0);
            return result.Length;
        }

        [Benchmark]
        public int B82d_Handcoded() =>
            new List<List<int>>(Group(_array, _keySelector).Values).Count;

        [Benchmark]
        public int B82e_Handcoded() =>
            Group(_array, _keySelector).Count;

        [Benchmark]
        public bool B82f_Handcoded()
        {
            foreach (var x in _linqGrouping)
                if (x == _inGroup)
                    return true;

            return false;
        }

        [Benchmark]
        public int B82g_Handcoded()
        {
            var count = 0;
            foreach (var x in _linqGrouping)
                ++count;

            return count;
        }

        [Benchmark]
        public int B82h_Handcoded()
        {
            var index = 0;
            foreach (var x in _linqGrouping)
            {
                if (index == 500)
                    return x;

                ++index;
            }

            throw new ArgumentOutOfRangeException("index");
        }

        [Benchmark]
        public int[] B82i_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _linqGrouping)
                result.Add(x);

            return result.ToArray();
        }

        [Benchmark]
        public int B82j_Handcoded() =>
            _linqLookup.Count;

        [Benchmark]
        public int B82k_Handcoded()
        {
            var result = new List<System.Linq.IGrouping<int, int>>();
            foreach (var group in _linqLookup)
                result.Add(group);

            return result.Count;
        }

        [Benchmark]
        public int B82l_Handcoded()
        {
            var inner = Group(_empty, _keySelector);
            var count = 0;
            foreach (var a in _array)
                if (inner.TryGetValue(_keySelector(a), out var matches))
                    count += matches.Count;

            return count;
        }

        [Benchmark]
        public int B82m_Handcoded()
        {
            var inner = Group(_array, _keySelector);
            var count = 0;
            foreach (var a in _empty)
                if (inner.TryGetValue(_keySelector(a), out var matches))
                    count += matches.Count;

            return count;
        }

        [Benchmark]
        public int B82n_Handcoded()
        {
            var inner = Group(_array, _keySelector);
            var count = 0;
            foreach (var a in _empty)
            {
                inner.TryGetValue(_keySelector(a), out var matches);
                ++count;
            }

            return count;
        }

        [Benchmark]
        public int B82o_Handcoded()
        {
            var inner = Group(_array, _keySelector);
            var count = 0;
            foreach (var a in _empty)
                count += inner.TryGetValue(_keySelector(a), out var matches) ? matches.Count : 1;

            return count;
        }

        [Benchmark]
        public int B82p_Handcoded()
        {
            var outer = Group(_array, _keySelector);
            var count = 0;
            foreach (var b in _empty)
                count += outer.TryGetValue(_keySelector(b), out var matches) ? matches.Count : 1;

            return count;
        }

        [Benchmark]
        public int B82q_Handcoded() =>
            FullJoinCount(_emptyArray, _array, _keySelector, _keySelector);

        [Benchmark]
        public int B82r_Handcoded() =>
            FullJoinCount(_array, _array, x => x, x => x);

        static Dictionary<int, List<int>> Group(int[] source, Func<int, int> keySelector)
        {
            var groups = new Dictionary<int, List<int>>();
            foreach (var x in source)
            {
                var key = keySelector(x);
                if (!groups.TryGetValue(key, out var group))
                    groups.Add(key, group = new List<int>());

                group.Add(x);
            }

            return groups;
        }

        static Dictionary<int, List<int>> Group(List<int> source, Func<int, int> keySelector)
        {
            var groups = new Dictionary<int, List<int>>();
            foreach (var x in source)
            {
                var key = keySelector(x);
                if (!groups.TryGetValue(key, out var group))
                    groups.Add(key, group = new List<int>());

                group.Add(x);
            }

            return groups;
        }

        // the number of results of a FullJoin: a pair per match, and one result for each unmatched element
        static int FullJoinCount(int[] outer, int[] inner, Func<int, int> outerKey, Func<int, int> innerKey)
        {
            var outerGroups = Group(outer, outerKey);
            var matched = new HashSet<int>();
            var count = 0;
            foreach (var b in inner)
            {
                var key = innerKey(b);
                if (outerGroups.TryGetValue(key, out var matches))
                {
                    count += matches.Count;
                    matched.Add(key);
                }
                else
                    ++count;
            }

            foreach (var group in outerGroups)
                if (!matched.Contains(group.Key))
                    count += group.Value.Count;

            return count;
        }
    }
}
