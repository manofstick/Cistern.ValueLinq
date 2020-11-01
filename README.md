# Cistern.ValueLinq

In the tradition of [Kevin Montrose's LinqAF](https://github.com/kevin-montrose/LinqAF), a version of Linq that has minimal allocations. (Another rendition on this there is [@reegeek's StructLinq](https://github.com/reegeek/StructLinq)).

NOTE THAT THIS A WIP; NOT ALL OPERATORS ARE SUPPORTED (ALTHOUGH THEY ALL "WORK" BECAUSE THEY ARE PATCHED TO SYSTEM.LINQ)

So given that there area already a couple of version of a value based linq, what does this one bring to the table?

- Easy conversion from `System.Linq` (just change the using declaration)
- Different performance profile (there are winners and losers)
- Different memory usage profile (but zero allocations on many streams)
- Performance remains on execution even when stream is returned to an `IEnumerable<>` (but obviously this creates an allocation)
- Fast across all source lengths (i.e. no heavy creation phase)
- If you want to get ugly, performance can be close to hand written loops (and actually beat it sometimes. Huh? Say what? Run the example! Depends on input data, so I'm assuming just "luck" of branch prediction going in my favour, but would really love someone with skills to analyse assembly to debunk or confirm this)

Anyway, here is simple example which shows:
- faster than linq (and actually System.Linq is only as fast here because it "knows" about `Select().Where()` as a pattern, if we were to do a `Select` with an index, or even a `Where().Select()` then the performance of Linq is *much* worse. ValueLinq has no such special case, although it does support some optimizations for things like `Last` or `Count`)
- the ugly


```csharp
        [Benchmark(Baseline = true)]
        public double Handcoded()
        {
            return _ints switch
            {
                int[] asArray => AsArray(asArray),
                _ => throw new NotSupportedException(),
            };

            static int AsArray(int[] asArray)
            {
                var max = int.MinValue;
                foreach (var n in asArray)
                {
                    var x = n / 2;
                    if ((x & 1) == 0)
                    {
                        if (x > max)
                            max = x;
                    }
                }
                return max;
            }
        }
```

OK; Pretty simple, the linq transform of this is:

```csharp
        [Benchmark]
        public int Linq() =>
            _ints
            .Select(x => x / 2)
            .Where(x => (x & 1) == 0)
            .Max();
```

And, the Cistern.ValueLinq looks exactly the same:

```csharp
        [Benchmark]
        public double CisternValueLinq_normal() =>
            _ints
            .Select(x => x / 2)
            .Where(x => (x & 1) == 0)
            .Max();
```

OK, and here is an example of the 'ugly' where we use value-type lambdas to perform the actions. These are pure functions. They could copy some state around, but lead to some perverse outcomes.

```csharp
        struct HalveAnInt : IFunc<int, int> { public int Invoke(int t) => t / 2; } 
        struct FilterEvenInts : IFunc<int, bool> { public bool Invoke(int t) => (t & 1) == 0; }
```

And then we can use them in my new Linq as follows

```csharp
        [Benchmark]
        public int CisternValueLinq_ValueFuncs() =>
            _ints
            .Select(new HalveAnInt(), default(int)) // ug, sugar please + better type inference...
            .Where(new FilterEvenInts()) // ug, sugar please
            .Max();
```

Notice that we have to supply the output type for the where (via `default(int)`) due to the way that c# handles type inference of generic arguments with constraints.

Now, the following doesn't exist, but I'm imagining you could have an additional syntax something like the following, where a >=> would create a value type IFunc used in the above example, and allow a layer of type-inference to do some magic. But hey, one can but dream:

```
        [Benchmark]
        public double CisternValueLinq_struct_in_dream_land() =>
            _ints
            .Select(t >=> t / 2)
            .Where(t >=> (t & 1) == 0)
            .Max();
```

And finally a "nothing up my sleave" version that seemly switches between the value type representation and the `IEnumerable<>` for an example of the trivial interop with existing code. It also shows that segments created in different parts can all be just put together with an Aggregating function, and hence all the magic that ties together everything at runtime.

```csharp
        [Benchmark]
        public double CisternValueLinq_struct_nothing_up_my_sleve()
        {
            IEnumerable<int> collection = GetCollection();
            IEnumerable<int> andSelect = AddSelect(collection);
            IEnumerable<int> withWhere = AddWhere(andSelect);

            var result = andSelect.Max();

            return result;

            IEnumerable<int> GetCollection() => _ints;
            IEnumerable<int> AddSelect(IEnumerable<int> stuff) => stuff.Select(x => x / 2);
            IEnumerable<int> AddWhere(IEnumerable<int> stuff) => stuff.Where(x => (x & 1) == 0);
        }
```
|                                      Method |  Length | ContainerType |              Mean |           Error |          StdDev |            Median | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------------------------------- |-------- |-------------- |------------------:|----------------:|----------------:|------------------:|------:|--------:|-------:|------:|------:|----------:|
|                 CisternValueLinq_ValueFuncs |       1 |         Array |        109.014 ns |       0.0963 ns |       0.0804 ns |        109.001 ns | 12.33 |    0.02 |      - |     - |     - |         - |
|                            CisternValueLinq |       1 |         Array |         92.106 ns |       0.3890 ns |       0.3448 ns |         91.936 ns | 10.42 |    0.05 |      - |     - |     - |         - |
| CisternValueLinq_struct_nothing_up_my_sleve |       1 |         Array |        337.995 ns |       0.7011 ns |       0.6215 ns |        337.797 ns | 38.24 |    0.11 | 0.0286 |     - |     - |     120 B |
|                                   Handcoded |       1 |         Array |          8.838 ns |       0.0154 ns |       0.0128 ns |          8.835 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|                                        Linq |       1 |         Array |        179.817 ns |       0.4103 ns |       0.3838 ns |        179.715 ns | 20.34 |    0.04 | 0.0248 |     - |     - |     104 B |
|                                             |         |               |                   |                 |                 |                   |       |         |        |       |       |           |
|                 CisternValueLinq_ValueFuncs |     100 |         Array |        467.568 ns |       0.6051 ns |       0.5364 ns |        467.620 ns |  2.42 |    0.00 |      - |     - |     - |         - |
|                            CisternValueLinq |     100 |         Array |      1,256.297 ns |       0.5981 ns |       0.4995 ns |      1,256.220 ns |  6.49 |    0.01 |      - |     - |     - |         - |
| CisternValueLinq_struct_nothing_up_my_sleve |     100 |         Array |      1,677.269 ns |       9.3588 ns |       8.7542 ns |      1,672.959 ns |  8.67 |    0.05 | 0.0286 |     - |     - |     120 B |
|                                   Handcoded |     100 |         Array |        193.526 ns |       0.3986 ns |       0.3328 ns |        193.607 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|                                        Linq |     100 |         Array |      2,107.560 ns |      15.9628 ns |      14.9316 ns |      2,107.047 ns | 10.90 |    0.07 | 0.0229 |     - |     - |     104 B |
|                                             |         |               |                   |                 |                 |                   |       |         |        |       |       |           |
|                 CisternValueLinq_ValueFuncs | 1000000 |         Array |  3,928,467.738 ns |  78,228.5717 ns | 201,932.9239 ns |  3,799,725.391 ns |  0.63 |    0.02 |      - |     - |     - |         - |
|                            CisternValueLinq | 1000000 |         Array | 14,837,932.500 ns |  38,233.4876 ns |  35,763.6280 ns | 14,843,329.688 ns |  2.24 |    0.01 |      - |     - |     - |         - |
| CisternValueLinq_struct_nothing_up_my_sleve | 1000000 |         Array | 16,760,601.339 ns | 143,574.7964 ns | 127,275.3173 ns | 16,745,145.312 ns |  2.53 |    0.02 |      - |     - |     - |     120 B |
|                                   Handcoded | 1000000 |         Array |  6,623,467.344 ns |  32,653.3972 ns |  30,544.0080 ns |  6,618,369.531 ns |  1.00 |    0.00 |      - |     - |     - |       5 B |
|                                        Linq | 1000000 |         Array | 20,094,558.259 ns | 242,776.7486 ns | 215,215.2640 ns | 20,076,793.750 ns |  3.04 |    0.03 |      - |     - |     - |     104 B |



