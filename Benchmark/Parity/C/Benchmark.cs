using BenchmarkDotNet.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Cistern.Benchmarks.Parity
{
    // Audit section C: consumer-side dispatch on the source's type. System.Linq's aggregations called on a plain
    // IEnumerable<T> check the runtime type (ICollection<T>, IList<T>, the non-generic ICollection) and take a
    // shortcut: a count read, an indexer, a CopyTo. One method per entry (a letter per source or operation where an
    // entry covers several) and library, named by the entry's ID. Every source is declared as IEnumerable<T>, so
    // neither library sees its static type, except where an entry is about the static type (C16, C20). The sources are
    // a HashSet<T> (an ICollection<T> that is no IList<T>), a Collection<T> (an IList<T> that is neither List<T> nor
    // an array), a collection that is only the non-generic ICollection, and an iterator, which none of them can see
    // through.
    [MemoryDiagnoser]
    public partial class C
    {
        const int Length = 10_000;

        int[] _array;
        IEnumerable<int> _set;
        IEnumerable<int> _collection;
        IEnumerable<int> _iterator;
        IEnumerable<int> _nonGeneric;
        IEnumerable<int> _single;
        IEnumerable<int> _emptyCollection;
        IEnumerable<string> _words;
        string _wordAsked;
        int[] _arrayCopy;
        List<int> _listA;
        List<int> _listB;
        IEnumerable<int> _setOther;
        IEnumerable<int> _collectionCopy;
        int _present;
        Func<int, bool> _predicate;
        Func<int, int> _keySelector;

        public C()
        {
            _array = Data.Values(Length);

            // about a thousand distinct values
            _set = new HashSet<int>(_array);
            _collection = new Collection<int>(new List<int>(_array));
            _iterator = Data.Iterator(_array);
            _nonGeneric = new NonGenericCollection(_array);
            _single = new Collection<int>(new List<int> { 42 });
            _emptyCollection = new Collection<int>(new List<int>());

            // present only when the comparison ignores case
            var words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < 1000; ++i)
                words.Add("w" + i);
            _words = words;
            _wordAsked = "W998";

            _arrayCopy = (int[])_array.Clone();
            _listA = new List<int>(_array);
            _listB = new List<int>(_array);

            // a different count
            var setOther = new HashSet<int>(_array);
            setOther.Remove(_array[0]);
            _setOther = setOther;
            _collectionCopy = new Collection<int>(new List<int>(_array));

            // near the end of the data
            _present = _array[Length - 10];

            _predicate = Data.Passing(Selectivity.Ten);
            _keySelector = x => x;
        }

        // an ICollection that is not an ICollection<int>
        sealed class NonGenericCollection : IEnumerable<int>, ICollection
        {
            readonly int[] _values;

            public NonGenericCollection(int[] values) =>
                _values = values;

            public int Count => _values.Length;

            public object SyncRoot => this;

            public bool IsSynchronized => false;

            public void CopyTo(Array array, int index) =>
                _values.CopyTo(array, index);

            public IEnumerator<int> GetEnumerator() =>
                ((IEnumerable<int>)_values).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                GetEnumerator();
        }

        internal static void SanityCheck()
        {
            var check = new C();

            Check.Same(nameof(C1a_Linq), check.C1a_Linq(), check.C1a_CisternValueLinq());
            Check.Same(nameof(C1b_Linq), check.C1b_Linq(), check.C1b_CisternValueLinq());
            Check.Same(nameof(C2a_Linq), check.C2a_Linq(), check.C2a_CisternValueLinq());
            Check.Same(nameof(C2b_Linq), check.C2b_Linq(), check.C2b_CisternValueLinq());
            Check.Same(nameof(C3a_Linq), check.C3a_Linq() ? 1 : 0, check.C3a_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(C3b_Linq), check.C3b_Linq() ? 1 : 0, check.C3b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(C4_Linq), check.C4_Linq(), check.C4_CisternValueLinq());
            Check.Same(nameof(C5_Linq), check.C5_Linq(), check.C5_CisternValueLinq());
            Check.Same(nameof(C6_Linq), check.C6_Linq(), check.C6_CisternValueLinq());
            Check.Same(nameof(C7_Linq), check.C7_Linq(), check.C7_CisternValueLinq());
            Check.Same(nameof(C8a_Linq), check.C8a_Linq(), check.C8a_CisternValueLinq());
            Check.Same(nameof(C8b_Linq), check.C8b_Linq(), check.C8b_CisternValueLinq());
            Check.Same(nameof(C9_Linq), check.C9_Linq(), check.C9_CisternValueLinq());
            Check.Same(nameof(C10a_Linq), check.C10a_Linq() ? 1 : 0, check.C10a_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(C10b_Linq), check.C10b_Linq() ? 1 : 0, check.C10b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(C10c_Linq), check.C10c_Linq() ? 1 : 0, check.C10c_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(C11_Linq), check.C11_Linq() ? 1 : 0, check.C11_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(C12a_Linq), check.C12a_Linq(), check.C12a_CisternValueLinq());
            Check.Same(nameof(C12b_Linq), check.C12b_Linq(), check.C12b_CisternValueLinq());
            Check.Same(nameof(C13a_Linq), check.C13a_Linq(), check.C13a_CisternValueLinq());
            Check.Same(nameof(C13b_Linq), check.C13b_Linq(), check.C13b_CisternValueLinq());
            Check.Same(nameof(C14a_Linq), check.C14a_Linq(), check.C14a_CisternValueLinq());
            Check.Same(nameof(C14b_Linq), check.C14b_Linq(), check.C14b_CisternValueLinq());
            Check.Same(nameof(C16a_Linq), check.C16a_Linq() ? 1 : 0, check.C16a_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(C16b_Linq), check.C16b_Linq() ? 1 : 0, check.C16b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(C17a_Linq), check.C17a_Linq() ? 1 : 0, check.C17a_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(C17b_Linq), check.C17b_Linq() ? 1 : 0, check.C17b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(C17c_Linq), check.C17c_Linq() ? 1 : 0, check.C17c_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(C19a_Linq), check.C19a_Linq(), check.C19a_CisternValueLinq());
            Check.Same(nameof(C19b_Linq), check.C19b_Linq(), check.C19b_CisternValueLinq());
            Check.Same(nameof(C20_Linq), check.C20_Linq(), check.C20_CisternValueLinq());

            Check.Same(nameof(C1a_ZLinq), check.C1a_Linq(), check.C1a_ZLinq());
            Check.Same(nameof(C1b_ZLinq), check.C1b_Linq(), check.C1b_ZLinq());
            Check.Same(nameof(C2a_ZLinq), check.C2a_Linq(), check.C2a_ZLinq());
            Check.Same(nameof(C2b_ZLinq), check.C2b_Linq(), check.C2b_ZLinq());
            Check.Same(nameof(C3a_ZLinq), check.C3a_Linq() ? 1 : 0, check.C3a_ZLinq() ? 1 : 0);
            Check.Same(nameof(C3b_ZLinq), check.C3b_Linq() ? 1 : 0, check.C3b_ZLinq() ? 1 : 0);
            Check.Same(nameof(C4_ZLinq), check.C4_Linq(), check.C4_ZLinq());
            Check.Same(nameof(C5_ZLinq), check.C5_Linq(), check.C5_ZLinq());
            Check.Same(nameof(C6_ZLinq), check.C6_Linq(), check.C6_ZLinq());
            Check.Same(nameof(C7_ZLinq), check.C7_Linq(), check.C7_ZLinq());
            Check.Same(nameof(C8a_ZLinq), check.C8a_Linq(), check.C8a_ZLinq());
            Check.Same(nameof(C8b_ZLinq), check.C8b_Linq(), check.C8b_ZLinq());
            Check.Same(nameof(C9_ZLinq), check.C9_Linq(), check.C9_ZLinq());
            Check.Same(nameof(C10a_ZLinq), check.C10a_Linq() ? 1 : 0, check.C10a_ZLinq() ? 1 : 0);
            Check.Same(nameof(C10b_ZLinq), check.C10b_Linq() ? 1 : 0, check.C10b_ZLinq() ? 1 : 0);
            Check.Same(nameof(C10c_ZLinq), check.C10c_Linq() ? 1 : 0, check.C10c_ZLinq() ? 1 : 0);
            Check.Same(nameof(C11_ZLinq), check.C11_Linq() ? 1 : 0, check.C11_ZLinq() ? 1 : 0);
            Check.Same(nameof(C12a_ZLinq), check.C12a_Linq(), check.C12a_ZLinq());
            Check.Same(nameof(C12b_ZLinq), check.C12b_Linq(), check.C12b_ZLinq());
            Check.Same(nameof(C13a_ZLinq), check.C13a_Linq(), check.C13a_ZLinq());
            Check.Same(nameof(C13b_ZLinq), check.C13b_Linq(), check.C13b_ZLinq());
            Check.Same(nameof(C14a_ZLinq), check.C14a_Linq(), check.C14a_ZLinq());
            Check.Same(nameof(C14b_ZLinq), check.C14b_Linq(), check.C14b_ZLinq());
            Check.Same(nameof(C16a_ZLinq), check.C16a_Linq() ? 1 : 0, check.C16a_ZLinq() ? 1 : 0);
            Check.Same(nameof(C16b_ZLinq), check.C16b_Linq() ? 1 : 0, check.C16b_ZLinq() ? 1 : 0);
            Check.Same(nameof(C17a_ZLinq), check.C17a_Linq() ? 1 : 0, check.C17a_ZLinq() ? 1 : 0);
            Check.Same(nameof(C17b_ZLinq), check.C17b_Linq() ? 1 : 0, check.C17b_ZLinq() ? 1 : 0);
            Check.Same(nameof(C17c_ZLinq), check.C17c_Linq() ? 1 : 0, check.C17c_ZLinq() ? 1 : 0);
            Check.Same(nameof(C19a_ZLinq), check.C19a_Linq(), check.C19a_ZLinq());
            Check.Same(nameof(C19b_ZLinq), check.C19b_Linq(), check.C19b_ZLinq());
            Check.Same(nameof(C20_ZLinq), check.C20_Linq(), check.C20_ZLinq());

            Check.Same(nameof(C1a_Handcoded), check.C1a_Linq(), check.C1a_Handcoded());
            Check.Same(nameof(C1b_Handcoded), check.C1b_Linq(), check.C1b_Handcoded());
            Check.Same(nameof(C2a_Handcoded), check.C2a_Linq(), check.C2a_Handcoded());
            Check.Same(nameof(C2b_Handcoded), check.C2b_Linq(), check.C2b_Handcoded());
            Check.Same(nameof(C3a_Handcoded), check.C3a_Linq() ? 1 : 0, check.C3a_Handcoded() ? 1 : 0);
            Check.Same(nameof(C3b_Handcoded), check.C3b_Linq() ? 1 : 0, check.C3b_Handcoded() ? 1 : 0);
            Check.Same(nameof(C4_Handcoded), check.C4_Linq(), check.C4_Handcoded());
            Check.Same(nameof(C5_Handcoded), check.C5_Linq(), check.C5_Handcoded());
            Check.Same(nameof(C6_Handcoded), check.C6_Linq(), check.C6_Handcoded());
            Check.Same(nameof(C7_Handcoded), check.C7_Linq(), check.C7_Handcoded());
            Check.Same(nameof(C8a_Handcoded), check.C8a_Linq(), check.C8a_Handcoded());
            Check.Same(nameof(C8b_Handcoded), check.C8b_Linq(), check.C8b_Handcoded());
            Check.Same(nameof(C9_Handcoded), check.C9_Linq(), check.C9_Handcoded());
            Check.Same(nameof(C10a_Handcoded), check.C10a_Linq() ? 1 : 0, check.C10a_Handcoded() ? 1 : 0);
            Check.Same(nameof(C10b_Handcoded), check.C10b_Linq() ? 1 : 0, check.C10b_Handcoded() ? 1 : 0);
            Check.Same(nameof(C10c_Handcoded), check.C10c_Linq() ? 1 : 0, check.C10c_Handcoded() ? 1 : 0);
            Check.Same(nameof(C11_Handcoded), check.C11_Linq() ? 1 : 0, check.C11_Handcoded() ? 1 : 0);
            Check.Same(nameof(C12a_Handcoded), check.C12a_Linq(), check.C12a_Handcoded());
            Check.Same(nameof(C12b_Handcoded), check.C12b_Linq(), check.C12b_Handcoded());
            Check.Same(nameof(C13a_Handcoded), check.C13a_Linq(), check.C13a_Handcoded());
            Check.Same(nameof(C13b_Handcoded), check.C13b_Linq(), check.C13b_Handcoded());
            Check.Same(nameof(C14a_Handcoded), check.C14a_Linq(), check.C14a_Handcoded());
            Check.Same(nameof(C14b_Handcoded), check.C14b_Linq(), check.C14b_Handcoded());
            Check.Same(nameof(C16a_Handcoded), check.C16a_Linq() ? 1 : 0, check.C16a_Handcoded() ? 1 : 0);
            Check.Same(nameof(C16b_Handcoded), check.C16b_Linq() ? 1 : 0, check.C16b_Handcoded() ? 1 : 0);
            Check.Same(nameof(C17a_Handcoded), check.C17a_Linq() ? 1 : 0, check.C17a_Handcoded() ? 1 : 0);
            Check.Same(nameof(C17b_Handcoded), check.C17b_Linq() ? 1 : 0, check.C17b_Handcoded() ? 1 : 0);
            Check.Same(nameof(C17c_Handcoded), check.C17c_Linq() ? 1 : 0, check.C17c_Handcoded() ? 1 : 0);
            Check.Same(nameof(C19a_Handcoded), check.C19a_Linq(), check.C19a_Handcoded());
            Check.Same(nameof(C19b_Handcoded), check.C19b_Linq(), check.C19b_Handcoded());
            Check.Same(nameof(C20_Handcoded), check.C20_Linq(), check.C20_Handcoded());

            // the entries whose saving is user code not run: Cistern should call it no more often than System.Linq
            Action<Func<int, bool>> usePredicate = p => check._predicate = p;
            Check.NoMoreCalls(nameof(C6_Linq), check._predicate, usePredicate, () => check.C6_Linq(), () => check.C6_CisternValueLinq());

            Action<Func<int, int>> useKey = k => check._keySelector = k;
            Check.NoMoreCalls(nameof(C14a_Linq), check._keySelector, useKey, () => check.C14a_Linq(), () => check.C14a_CisternValueLinq());
            Check.NoMoreCalls(nameof(C14b_Linq), check._keySelector, useKey, () => check.C14b_Linq(), () => check.C14b_CisternValueLinq());
        }
    }
}
